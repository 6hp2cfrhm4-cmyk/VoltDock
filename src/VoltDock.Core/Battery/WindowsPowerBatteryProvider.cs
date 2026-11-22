namespace VoltDock.Core.Battery;

using VoltDock.Core.Devices;
using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices;

/// <summary>
/// Reads battery level from Windows Power API (laptop internal battery) using native kernel32.
/// </summary>
public sealed class WindowsPowerBatteryProvider : BatteryProviderBase
{
    public override string ProviderId => "windows-power-battery";
    public override string ProviderName => "Windows Power API Battery";
    public override BatterySource SourceType => BatterySource.WindowsPower;
    public override int Priority => 100;
    public override IReadOnlyList<DeviceType> SupportedDeviceTypes { get; } = [DeviceType.LaptopBattery];

    public WindowsPowerBatteryProvider(ILogger<WindowsPowerBatteryProvider> logger) : base(logger) { }

    public override bool CanReadBattery(Device device)
    {
        return device.Type == DeviceType.LaptopBattery &&
               device.Id == "windows-power:battery";
    }

    public override async Task<BatteryReading?> ReadBatteryAsync(Device device, CancellationToken cancellationToken = default)
    {
        if (!CanReadBattery(device))
            return null;

        try
        {
            var status = new SYSTEM_POWER_STATUS();
            if (!GetSystemPowerStatus(ref status))
                return null;

            var level = status.BatteryLifePercent;
            if (level == 255) level = 0; // 255 = unknown

            var chargeState = status.ACLineStatus == 1 ? BatteryChargeState.Charging : BatteryChargeState.Discharging;
            if (level >= 100) chargeState = BatteryChargeState.Full;

            return new BatteryReading(
                level,
                chargeState,
                BatteryHealth.Unknown,
                BatterySource.WindowsPower,
                componentId: "main",
                componentName: "Battery");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read Windows Power battery");
            return null;
        }
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