namespace VoltDock.Core.Battery;

using VoltDock.Core.Devices;
using Microsoft.Extensions.Logging;
using Windows.Devices.HumanInterfaceDevice;
using Windows.Storage;
using Windows.Storage.Streams;

/// <summary>
/// Reads battery level from HID Battery Report (Usage Page 0x000C, Usage 0x0001).
/// </summary>
public sealed class HidBatteryProvider : BatteryProviderBase
{
    public override string ProviderId => "hid-battery";
    public override string ProviderName => "HID Battery Report";
    public override BatterySource SourceType => BatterySource.HidReport;
    public override int Priority => 100;
    public override IReadOnlyList<DeviceType> SupportedDeviceTypes { get; } =
        [DeviceType.Mouse, DeviceType.Keyboard, DeviceType.Headphones, DeviceType.TwsEarbuds, DeviceType.GameController, DeviceType.Trackpad, DeviceType.Presenter, DeviceType.Stylus, DeviceType.GenericHid];

    public HidBatteryProvider(ILogger<HidBatteryProvider> logger) : base(logger) { }

    public override bool CanReadBattery(Device device)
    {
        return SupportedDeviceTypes.Contains(device.Type) &&
               !string.IsNullOrEmpty(device.DeviceInstanceId) &&
               device.DeviceInstanceId.StartsWith("hid:");
    }

    public override async Task<BatteryReading?> ReadBatteryAsync(Device device, CancellationToken cancellationToken = default)
    {
        if (!CanReadBattery(device) || string.IsNullOrEmpty(device.DeviceInstanceId))
            return null;

        try
        {
            var deviceId = device.DeviceInstanceId.Substring(4); // Remove "hid:" prefix
            var hidDevice = await HidDevice.FromIdAsync(deviceId, FileAccessMode.Read);
            if (hidDevice == null)
                return null;

            return await ReadBatteryFromDeviceAsync(hidDevice, device, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read HID battery for {DeviceId}", device.Id);
            return null;
        }
    }

    private async Task<BatteryReading?> ReadBatteryFromDeviceAsync(HidDevice device, Device deviceInfo, CancellationToken cancellationToken)
    {
        try
        {
            // Get input report for battery
            var report = await device.GetInputReportAsync(0);
            if (report == null)
                return null;

            var dataReader = DataReader.FromBuffer(report.Data);
            var bytes = new byte[report.Data.Length];
            dataReader.ReadBytes(bytes);

            // Parse HID battery report
            // This is a simplified parser - real implementation would use HidParser
            var level = ParseBatteryLevel(bytes);
            if (!level.HasValue)
                return null;

            var chargeState = BatteryChargeState.Discharging;
            // Could parse charging state from report if available

            return new BatteryReading(
                level.Value,
                chargeState,
                BatteryHealth.Unknown,
                BatterySource.HidReport,
                componentId: "main",
                componentName: "Battery",
                rawValue: bytes);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error parsing HID battery report");
            return null;
        }
    }

    private int? ParseBatteryLevel(byte[] reportData)
    {
        // Simplified battery level parsing
        // Real implementation would properly parse HID report descriptor
        // For now, return null to indicate unknown
        // TODO: Implement proper HID report parsing using Windows.Devices.HumanInterfaceDevice.HidParser
        return null;
    }
}