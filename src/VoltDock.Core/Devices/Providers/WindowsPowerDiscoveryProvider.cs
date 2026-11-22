namespace VoltDock.Core.Devices.Providers;

using VoltDock.Core.Devices;
using Microsoft.Extensions.Logging;
using Windows.System.Power;

/// <summary>
/// Discovers the system's internal laptop battery using Windows Power API.
/// </summary>
public sealed class WindowsPowerDiscoveryProvider : DiscoveryProviderBase
{
    private readonly Dictionary<string, Device> _discoveredDevices = new();
    private readonly Timer? _pollTimer;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private BatteryStatus? _lastStatus;

    public override string ProviderId => "windows-power";
    public override string ProviderName => "Windows Power API";
    public override int Priority => 70;

    public WindowsPowerDiscoveryProvider(ILogger<WindowsPowerDiscoveryProvider> logger) : base(logger)
    {
        _pollTimer = new Timer(PollBattery, null, Timeout.Infinite, Timeout.Infinite);
    }

    public override async Task StartAsync(CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            if (_isActive) return;

            // Check if we have a battery
            var status = GetBatteryStatus();
            if (status == null)
            {
                _logger.LogInformation("No battery detected, Windows Power provider not started");
                return;
            }

            // Create device
            var device = new Device("windows-power:battery")
            {
                Name = "Ноутбук",
                Type = DeviceType.LaptopBattery,
                Manufacturer = "System",
                Model = "Internal Battery",
                ConnectionState = DeviceConnectionState.Connected,
            };
            device.AddInterfacePath("windows-power:battery");
            device.AddBatterySource(BatterySource.WindowsPower);
            _discoveredDevices[device.Id] = device;

            // Initial read
            UpdateBatteryInfo(device, status);

            RaiseDeviceDiscovered(device);

            // Start polling (every 30 seconds)
            _pollTimer?.Change(TimeSpan.Zero, TimeSpan.FromSeconds(30));

            SetActive(true);
            _logger.LogInformation("Windows Power discovery started");
        }
        finally
        {
            _lock.Release();
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            if (!_isActive) return;

            _pollTimer?.Change(Timeout.Infinite, Timeout.Infinite);
            _discoveredDevices.Clear();
            SetActive(false);
            _logger.LogInformation("Windows Power discovery stopped");
        }
        finally
        {
            _lock.Release();
        }
    }

    public override IReadOnlyList<Device> GetDiscoveredDevices()
    {
        return _discoveredDevices.Values.ToList().AsReadOnly();
    }

    public override async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        var status = GetBatteryStatus();
        if (status != null && _discoveredDevices.TryGetValue("windows-power:battery", out var device))
        {
            UpdateBatteryInfo(device, status);
            RaiseDeviceUpdated(device, ["BatteryLevel", "ChargeState"]);
        }
    }

    private void PollBattery(object? state)
    {
        _ = Task.Run(async () =>
        {
            try
            {
                await RefreshAsync(CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error polling battery status");
            }
        });
    }

    private BatteryStatus? GetBatteryStatus()
    {
        try
        {
            // Try to get battery status using the modern API
            return BatteryStatus.GetDefault();
        }
        catch
        {
            return null;
        }
    }

    private void UpdateBatteryInfo(Device device, BatteryStatus status)
    {
        try
        {
            var level = (int)Math.Round(status.RemainingChargePercent * 100);
            var chargeState = status.PowerSource switch
            {
                PowerSource.AC => BatteryChargeState.Charging,
                PowerSource.Battery => BatteryChargeState.Discharging,
                _ => BatteryChargeState.Unknown
            };

            if (status.RemainingChargePercent >= 1.0)
                chargeState = BatteryChargeState.Full;

            device.BatteryInfo.PrimaryReading = new BatteryReading(
                level,
                chargeState,
                BatteryHealth.Unknown,
                BatterySource.WindowsPower,
                componentId: "main",
                componentName: "Battery");

            // Add remaining time if available
            if (status.RemainingDischargeTime.HasValue)
            {
                device.Metadata["RemainingTime"] = status.RemainingDischargeTime.Value;
            }

            device.Touch();
            _lastStatus = status;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to update battery info");
        }
    }

    public override async ValueTask DisposeAsync()
    {
        _pollTimer?.Dispose();
        await base.DisposeAsync();
    }
}