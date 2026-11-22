namespace VoltDock.Core.Battery;

using VoltDock.Core.Devices;
using Microsoft.Extensions.Logging;
using Windows.System.Power;

/// <summary>
/// Reads battery level from Windows Power API (laptop internal battery).
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
            var status = BatteryStatus.GetDefault();
            if (status == null)
                return null;

            var level = (int)Math.Round(status.RemainingChargePercent * 100);
            var chargeState = status.PowerSource switch
            {
                PowerSource.AC => BatteryChargeState.Charging,
                PowerSource.Battery => BatteryChargeState.Discharging,
                _ => BatteryChargeState.Unknown
            };

            if (status.RemainingChargePercent >= 1.0)
                chargeState = BatteryChargeState.Full;

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
}