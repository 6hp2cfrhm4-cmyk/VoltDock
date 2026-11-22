namespace VoltDock.Core.Battery;

using VoltDock.Core.Devices;
using Microsoft.Extensions.Logging;
using Windows.Devices.Enumeration;

/// <summary>
/// Reads battery level from Windows Device Enumeration properties (System.Devices.BatteryLevel).
/// </summary>
public sealed class DevicePropertiesBatteryProvider : BatteryProviderBase
{
    public override string ProviderId => "device-properties";
    public override string ProviderName => "Device Properties Battery";
    public override BatterySource SourceType => BatterySource.DeviceProperties;
    public override int Priority => 50; // Lower priority, used as fallback
    public override IReadOnlyList<DeviceType> SupportedDeviceTypes { get; } =
        [DeviceType.Mouse, DeviceType.Keyboard, DeviceType.Headphones, DeviceType.TwsEarbuds, DeviceType.GameController, DeviceType.Trackpad, DeviceType.Presenter, DeviceType.Stylus, DeviceType.Wearable, DeviceType.Smartphone, DeviceType.Tablet, DeviceType.GenericHid];

    public DevicePropertiesBatteryProvider(ILogger<DevicePropertiesBatteryProvider> logger) : base(logger) { }

    public override bool CanReadBattery(Device device)
    {
        return SupportedDeviceTypes.Contains(device.Type) &&
               !string.IsNullOrEmpty(device.DeviceInstanceId);
    }

    public override async Task<BatteryReading?> ReadBatteryAsync(Device device, CancellationToken cancellationToken = default)
    {
        if (!CanReadBattery(device))
            return null;

        try
        {
            var deviceInfo = await DeviceInformation.CreateFromIdAsync(device.DeviceInstanceId!, ["System.Devices.BatteryLevel", "System.Devices.BatteryIsCharging"]);
            if (deviceInfo == null)
                return null;

            var level = deviceInfo.Properties.TryGetValue("System.Devices.BatteryLevel", out var levelObj) && levelObj is int l ? l : (int?)null;
            var isCharging = deviceInfo.Properties.TryGetValue("System.Devices.BatteryIsCharging", out var chargingObj) && chargingObj is bool c && c;

            if (!level.HasValue)
                return null;

            var chargeState = isCharging ? BatteryChargeState.Charging : BatteryChargeState.Discharging;
            if (level >= 100) chargeState = BatteryChargeState.Full;

            return new BatteryReading(
                level.Value,
                chargeState,
                BatteryHealth.Unknown,
                BatterySource.DeviceProperties,
                componentId: "main",
                componentName: "Battery");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read device properties battery for {DeviceId}", device.Id);
            return null;
        }
    }
}