namespace VoltDock.Core.Devices.Providers;

using VoltDock.Core.Devices;
using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices;

/// <summary>
/// Discovers the system's internal laptop battery using Windows Power API.
/// </summary>
public sealed class WindowsPowerDiscoveryProvider : DiscoveryProviderBase
{
    private readonly Dictionary<string, Device> _discoveredDevices = new();
    private readonly Timer? _pollTimer;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private bool _hasBattery = false;

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

            // Check if we have a battery using native API
            _hasBattery = HasSystemBattery();
            if (!_hasBattery)
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
            UpdateBatteryInfo(device);

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
        if (_discoveredDevices.TryGetValue("windows-power:battery", out var device))
        {
            UpdateBatteryInfo(device);
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

    private bool HasSystemBattery()
    {
        try
        {
            var status = new SYSTEM_POWER_STATUS();
            return GetSystemPowerStatus(ref status) && status.BatteryFlag != 128; // 128 = no battery
        }
        catch
        {
            return false;
        }
    }

    private void UpdateBatteryInfo(Device device)
    {
        try
        {
            var status = new SYSTEM_POWER_STATUS();
            if (!GetSystemPowerStatus(ref status))
                return;

            var level = status.BatteryLifePercent;
            if (level == 255) level = 0; // 255 = unknown

            var chargeState = status.ACLineStatus == 1 ? BatteryChargeState.Charging : BatteryChargeState.Discharging;
            if (level >= 100) chargeState = BatteryChargeState.Full;

            device.BatteryInfo.PrimaryReading = new BatteryReading(
                level,
                chargeState,
                BatteryHealth.Unknown,
                BatterySource.WindowsPower,
                componentId: "main",
                componentName: "Battery");

            // Add remaining time if available
            if (status.BatteryLifeTime > 0 && status.BatteryLifeTime != -1)
            {
                device.Metadata["RemainingTime"] = TimeSpan.FromSeconds(status.BatteryLifeTime);
            }

            device.Touch();
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

    // Native Windows API for battery status
    [StructLayout(LayoutKind.Sequential)]
    private struct SYSTEM_POWER_STATUS
    {
        public byte ACLineStatus;        // 0 = offline, 1 = online, 255 = unknown
        public byte BatteryFlag;         // Battery status flags
        public byte BatteryLifePercent;  // 0-100, 255 = unknown
        public byte Reserved1;
        public int BatteryLifeTime;      // Seconds remaining, -1 = unknown
        public int BatteryFullLifeTime;  // Seconds when full, -1 = unknown
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool GetSystemPowerStatus(ref SYSTEM_POWER_STATUS lpSystemPowerStatus);
}