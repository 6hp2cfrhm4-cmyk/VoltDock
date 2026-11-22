namespace VoltDock.Core.Battery;

using VoltDock.Core.Devices;
using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices;

/// <summary>
/// Reads battery level from XInput game controllers.
/// </summary>
public sealed class XInputBatteryProvider : BatteryProviderBase
{
    public override string ProviderId => "xinput-battery";
    public override string ProviderName => "XInput Battery";
    public override BatterySource SourceType => BatterySource.XInput;
    public override int Priority => 100;
    public override IReadOnlyList<DeviceType> SupportedDeviceTypes { get; } = [DeviceType.GameController];

    public XInputBatteryProvider(ILogger<XInputBatteryProvider> logger) : base(logger) { }

    public override bool CanReadBattery(Device device)
    {
        return device.Type == DeviceType.GameController &&
               device.InterfacePaths.Any(p => p.StartsWith("xinput:"));
    }

    public override async Task<BatteryReading?> ReadBatteryAsync(Device device, CancellationToken cancellationToken = default)
    {
        if (!CanReadBattery(device))
            return null;

        try
        {
            // Extract controller index from interface path
            var xinputPath = device.InterfacePaths.FirstOrDefault(p => p.StartsWith("xinput:"));
            if (xinputPath == null)
                return null;

            var indexStr = xinputPath.Substring(7); // Remove "xinput:"
            if (!uint.TryParse(indexStr, out var index))
                return null;

            var reading = GetBatteryInformation(index);
            return reading;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read XInput battery for {DeviceId}", device.Id);
            return null;
        }
    }

    private BatteryReading? GetBatteryInformation(uint controllerIndex)
    {
        try
        {
            var result = XInputGetBatteryInformation(controllerIndex, 0, out var batteryInfo);
            if (result != 0)
                return null;

            var batteryType = batteryInfo.BatteryType;
            var batteryLevel = batteryInfo.BatteryLevel;

            if (batteryType == 0) // BATTERY_TYPE_DISCONNECTED
                return null;

            if (batteryType == 1) // BATTERY_TYPE_WIRED
            {
                return new BatteryReading(
                    100,
                    BatteryChargeState.Charging,
                    BatteryHealth.Unknown,
                    BatterySource.XInput,
                    componentId: "main",
                    componentName: "Controller");
            }

            // Battery levels: 0 = Empty, 1 = Low, 2 = Medium, 3 = Full
            var level = batteryLevel switch
            {
                0 => 0,      // BATTERY_LEVEL_EMPTY
                1 => 20,     // BATTERY_LEVEL_LOW
                2 => 50,     // BATTERY_LEVEL_MEDIUM
                3 => 100,    // BATTERY_LEVEL_FULL
                _ => (int?)null
            };

            if (!level.HasValue)
                return null;

            var chargeState = batteryLevel switch
            {
                3 => BatteryChargeState.Full,
                0 => BatteryChargeState.Discharging,
                _ => BatteryChargeState.Discharging
            };

            return new BatteryReading(
                level.Value,
                chargeState,
                BatteryHealth.Unknown,
                BatterySource.XInput,
                componentId: "main",
                componentName: "Controller");
        }
        catch
        {
            return null;
        }
    }

    [DllImport("xinput1_4.dll", CallingConvention = CallingConvention.StdCall)]
    private static extern uint XInputGetBatteryInformation(uint dwUserIndex, byte devType, out XINPUT_BATTERY_INFORMATION pBatteryInformation);

    [StructLayout(LayoutKind.Sequential)]
    private struct XINPUT_BATTERY_INFORMATION
    {
        public byte BatteryType;
        public byte BatteryLevel;
    }
}