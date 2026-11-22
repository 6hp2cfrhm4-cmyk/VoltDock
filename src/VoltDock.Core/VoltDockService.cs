namespace VoltDock.Core;

using VoltDock.Core.Devices;
using VoltDock.Core.Devices.Providers;
using VoltDock.Core.Battery;
using VoltDock.Core.Settings;
using VoltDock.Core.Notifications;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using ProviderDeviceUpdatedEventArgs = VoltDock.Core.Devices.Providers.DeviceUpdatedEventArgs;

/// <summary>
/// Main VoltDock service coordinating all components.
/// </summary>
public sealed class VoltDockService : IAsyncDisposable
{
    private readonly IServiceProvider _services;
    private readonly ILogger<VoltDockService> _logger;
    private readonly ISettingsService _settings;
    private readonly IDeviceIdentityResolver _identityResolver;
    private readonly IBatteryReadingAggregator _batteryAggregator;
    private readonly INotificationService _notificationService;
    private readonly IEnumerable<IDeviceDiscoveryProvider> _discoveryProviders;
    private readonly IEnumerable<IBatteryProvider> _batteryProviders;

    private readonly CancellationTokenSource _cts = new();
    private readonly Dictionary<string, Task> _providerTasks = new();
    private readonly Dictionary<string, IDisposable> _batterySubscriptions = new();
    private bool _disposed;

    public VoltDockService(
        IServiceProvider services,
        ILogger<VoltDockService> logger,
        ISettingsService settings,
        IDeviceIdentityResolver identityResolver,
        IBatteryReadingAggregator batteryAggregator,
        INotificationService notificationService,
        IEnumerable<IDeviceDiscoveryProvider> discoveryProviders,
        IEnumerable<IBatteryProvider> batteryProviders)
    {
        _services = services;
        _logger = logger;
        _settings = settings;
        _identityResolver = identityResolver;
        _batteryAggregator = batteryAggregator;
        _notificationService = notificationService;
        _discoveryProviders = discoveryProviders.OrderByDescending(p => p.Priority).ToList();
        _batteryProviders = batteryProviders.OrderByDescending(p => p.Priority).ToList();

        // Subscribe to settings changes
        _settings.SettingsChanged += OnSettingsChanged;

        // Subscribe to device events
        _identityResolver.DeviceCreated += OnDeviceCreated;
        _identityResolver.DeviceRemoved += OnDeviceRemoved;
        _identityResolver.DeviceUpdated += OnDeviceUpdated;
    }

    /// <summary>
    /// Starts the VoltDock service.
    /// </summary>
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting VoltDock service...");

        // Load settings
        await _settings.LoadAsync(cancellationToken);

        // Start discovery providers
        foreach (var provider in _discoveryProviders)
        {
            if (_settings.Settings.Behavior.DisabledProviders.Contains(provider.ProviderId))
            {
                _logger.LogInformation("Provider {ProviderId} is disabled, skipping", provider.ProviderId);
                continue;
            }

            try
            {
                provider.DeviceDiscovered += OnDeviceDiscovered;
                provider.DeviceLost += OnDeviceLost;
                provider.DeviceUpdated += OnProviderDeviceUpdated;

                await provider.StartAsync(cancellationToken);
                _logger.LogInformation("Started discovery provider: {ProviderName}", provider.ProviderName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start discovery provider: {ProviderName}", provider.ProviderName);
            }
        }

        // Start battery polling
        _ = Task.Run(() => BatteryPollingLoop(_cts.Token), _cts.Token);

        _logger.LogInformation("VoltDock service started");
    }

    /// <summary>
    /// Stops the VoltDock service.
    /// </summary>
    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Stopping VoltDock service...");

        _cts.Cancel();

        // Stop discovery providers
        foreach (var provider in _discoveryProviders)
        {
            try
            {
                await provider.StopAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping provider: {ProviderName}", provider.ProviderName);
            }
        }

        // Dispose battery subscriptions
        foreach (var sub in _batterySubscriptions.Values)
        {
            sub.Dispose();
        }
        _batterySubscriptions.Clear();

        // Save settings
        await _settings.SaveAsync(cancellationToken);

        _logger.LogInformation("VoltDock service stopped");
    }

    /// <summary>
    /// Gets all known devices.
    /// </summary>
    public IReadOnlyList<Device> GetDevices() => _identityResolver.GetAllDevices();

    /// <summary>
    /// Gets a device by ID.
    /// </summary>
    public Device? GetDevice(string deviceId) => _identityResolver.GetDevice(deviceId);

    /// <summary>
    /// Forces a refresh of all devices.
    /// </summary>
    public async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        foreach (var provider in _discoveryProviders)
        {
            if (provider.IsActive)
            {
                await provider.RefreshAsync(cancellationToken);
            }
        }
    }

    /// <summary>
    /// Forces a battery read for a specific device.
    /// </summary>
    public async Task<DeviceBatteryInfo?> ReadBatteryAsync(string deviceId, CancellationToken cancellationToken = default)
    {
        var device = _identityResolver.GetDevice(deviceId);
        if (device == null)
            return null;

        await ReadDeviceBatteryAsync(device, cancellationToken);
        return device.BatteryInfo;
    }

    private async Task BatteryPollingLoop(CancellationToken cancellationToken)
    {
        var settings = _settings.Settings.Behavior;

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var devices = _identityResolver.GetAllDevices()
                    .Where(d => d.ConnectionState == DeviceConnectionState.Connected)
                    .ToList();

                foreach (var device in devices)
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    await ReadDeviceBatteryAsync(device, cancellationToken);
                }

                // Adaptive polling interval
                var interval = settings.AdaptivePolling ? CalculateAdaptiveInterval(devices) : settings.PollingInterval;
                await Task.Delay(TimeSpan.FromSeconds(interval), cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in battery polling loop");
                await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);
            }
        }
    }

    private int CalculateAdaptiveInterval(IReadOnlyList<Device> devices)
    {
        var settings = _settings.Settings.Behavior;
        if (devices.Count == 0)
            return settings.MaxPollingInterval;

        // Check if any device has recent changes
        var hasRecentChanges = devices.Any(d =>
            d.BatteryInfo.LastUpdated > DateTimeOffset.UtcNow.AddMinutes(-2) &&
            d.BatteryInfo.PrimaryReading.HasLevel);

        return hasRecentChanges ? settings.PollingInterval : Math.Min(settings.PollingInterval * 2, settings.MaxPollingInterval);
    }

    private async Task ReadDeviceBatteryAsync(Device device, CancellationToken cancellationToken)
    {
        var applicableProviders = _batteryProviders.Where(p => p.CanReadBattery(device)).ToList();
        if (applicableProviders.Count == 0)
            return;

        // Read from all applicable providers concurrently (with limit)
        var semaphore = new SemaphoreSlim(_settings.Settings.Behavior.MaxConcurrentReads);
        var readTasks = applicableProviders.Select(async provider =>
        {
            await semaphore.WaitAsync(cancellationToken);
            try
            {
                var reading = await provider.ReadBatteryAsync(device, cancellationToken);
                if (reading.HasValue)
                {
                    return new ProviderBatteryReading(provider.ProviderId, reading.Value);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Battery read failed for {DeviceId} from {ProviderId}", device.Id, provider.ProviderId);
            }
            finally
            {
                semaphore.Release();
            }
            return (ProviderBatteryReading?)null;
        });

        var results = await Task.WhenAll(readTasks);
        var validReadings = results.Where(r => r.HasValue).Select(r => r.Value).ToList();

        if (validReadings.Count > 0)
        {
            _batteryAggregator.Aggregate(device, validReadings);
            CheckNotifications(device);
        }

        // Subscribe to notifications for providers that support it
        foreach (var provider in applicableProviders.Where(p => p.SupportsNotifications(device)))
        {
            var subKey = $"{device.Id}:{provider.ProviderId}";
            if (!_batterySubscriptions.ContainsKey(subKey))
            {
                var subscription = await provider.SubscribeToChangesAsync(device, reading =>
                {
                    _batteryAggregator.UpdateReading(device, new ProviderBatteryReading(provider.ProviderId, reading));
                    CheckNotifications(device);
                }, cancellationToken);

                if (subscription != null)
                {
                    _batterySubscriptions[subKey] = subscription;
                }
            }
        }
    }

    private void CheckNotifications(Device device)
    {
        if (!_settings.Settings.Notifications.Enabled || _notificationService.AreNotificationsSuppressed)
            return;

        var batteryInfo = device.BatteryInfo;
        if (!batteryInfo.HasAnyLevel)
            return;

        var level = batteryInfo.AggregatedLevel;
        if (!level.HasValue)
            return;

        var thresholds = _settings.Settings.Notifications.Thresholds
            .FirstOrDefault(t => t.DeviceId == device.Id || t.DeviceType == device.Type);
        var warningLevel = thresholds?.WarningPercent ?? _settings.Settings.Notifications.GlobalWarningPercent;
        var criticalLevel = thresholds?.CriticalPercent ?? _settings.Settings.Notifications.GlobalCriticalPercent;

        var notificationType = level <= criticalLevel ? BatteryNotificationType.Critical :
                              level <= warningLevel ? BatteryNotificationType.Warning : null;

        if (notificationType.HasValue)
        {
            var notification = new BatteryNotification
            {
                DeviceId = device.Id,
                DeviceName = device.DisplayName,
                DeviceType = device.Type,
                Type = notificationType.Value,
                BatteryLevel = level,
                ChargeState = batteryInfo.OverallChargeState,
                Message = $"{device.DisplayName}: заряд {level}%"
            };

            _ = _notificationService.ShowNotificationAsync(notification);
        }

        // Check for charging state changes
        if (batteryInfo.OverallChargeState == BatteryChargeState.Charging &&
            _settings.Settings.Notifications.NotifyOnCharging)
        {
            // TODO: Track previous state to avoid duplicate notifications
        }
    }

    private void OnDeviceDiscovered(object? sender, DeviceDiscoveredEventArgs e)
    {
        var canonicalDevice = _identityResolver.RegisterDevice(e.Device, e.ProviderId);
        _logger.LogDebug("Device discovered: {DeviceId} via {ProviderId}", canonicalDevice.Id, e.ProviderId);
    }

    private void OnDeviceLost(object? sender, DeviceLostEventArgs e)
    {
        var removed = _identityResolver.UnregisterDevice(e.DeviceId, e.ProviderId);
        if (removed)
        {
            _logger.LogDebug("Device removed: {DeviceId}", e.DeviceId);
        }

        // Clean up subscriptions
        var keysToRemove = _batterySubscriptions.Keys
            .Where(k => k.StartsWith($"{e.DeviceId}:"))
            .ToList();
        foreach (var key in keysToRemove)
        {
            _batterySubscriptions[key].Dispose();
            _batterySubscriptions.Remove(key);
        }
    }

    private void OnProviderDeviceUpdated(object? sender, ProviderDeviceUpdatedEventArgs e)
    {
        _identityResolver.UpdateDevice(e.Device.Id, e.ProviderId, d =>
        {
            foreach (var prop in e.UpdatedProperties)
            {
                // Provider-specific property updates would be handled here
            }
            d.Touch();
        });
    }

    private void OnDeviceCreated(object? sender, DeviceEventArgs e)
    {
        _logger.LogInformation("New device registered: {DeviceName} ({DeviceId})", e.Device.DisplayName, e.Device.Id);
        _ = ReadDeviceBatteryAsync(e.Device, _cts.Token);
    }

    private void OnDeviceRemoved(object? sender, DeviceEventArgs e)
    {
        _logger.LogInformation("Device removed: {DeviceName} ({DeviceId})", e.Device.DisplayName, e.Device.Id);
    }

    private void OnDeviceUpdated(object? sender, DeviceUpdatedEventArgs e)
    {
        // Device properties updated
    }

    private void OnSettingsChanged(object? sender, SettingsChangedEventArgs e)
    {
        _logger.LogDebug("Setting changed: {Key}", e.Key);
        // Handle specific setting changes
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;

        _disposed = true;
        _settings.SettingsChanged -= OnSettingsChanged;
        _identityResolver.DeviceCreated -= OnDeviceCreated;
        _identityResolver.DeviceRemoved -= OnDeviceRemoved;
        _identityResolver.DeviceUpdated -= OnDeviceUpdated;

        await StopAsync(CancellationToken.None);
        _cts.Dispose();
    }
}