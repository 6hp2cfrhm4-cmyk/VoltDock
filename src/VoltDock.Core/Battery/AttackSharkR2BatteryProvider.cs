namespace VoltDock.Core.Battery;

using VoltDock.Core.Devices;
using Microsoft.Extensions.Logging;
using Windows.Devices.HumanInterfaceDevice;
using Windows.Storage.Streams;

/// <summary>
/// Battery provider for Attack Shark R2 mouse.
/// Supports both Bluetooth and 2.4GHz USB receiver modes.
/// </summary>
public sealed class AttackSharkR2BatteryProvider : IManufacturerBatteryProvider
{
    private readonly ILogger<AttackSharkR2BatteryProvider> _logger;

    public string ProviderId => "attackshark-r2";
    public string ProviderName => "Attack Shark R2 Battery";
    public BatterySource SourceType => BatterySource.ManufacturerProtocol;
    public int Priority => 110;
    public string ManufacturerName => "Attack Shark";
    public IReadOnlyList<DeviceType> SupportedDeviceTypes { get; } = [DeviceType.Mouse];

    public IReadOnlyList<ManufacturerModelId> SupportedModels { get; } = new[]
    {
        // USB 2.4GHz receiver VID/PID (to be determined from actual device)
        ManufacturerModelId.FromUsb(0x2D1F, 0x0002), // Placeholder - need real VID/PID
        // Bluetooth name pattern
        ManufacturerModelId.FromBluetoothName("Attack Shark R2"),
        ManufacturerModelId.FromModel("R2")
    };

    public AttackSharkR2BatteryProvider(ILogger<AttackSharkR2BatteryProvider> logger)
    {
        _logger = logger;
    }

    public bool CanReadBattery(Device device)
    {
        return SupportedDeviceTypes.Contains(device.Type) &&
               SupportedModels.Any(m => m.Matches(device));
    }

    public async Task<BatteryReading?> ReadBatteryAsync(Device device, CancellationToken cancellationToken = default)
    {
        if (!CanReadBattery(device))
            return null;

        // Try USB HID first (2.4GHz mode)
        if (device.UsbVendorId.HasValue && device.UsbProductId.HasValue &&
            device.DeviceInstanceId?.StartsWith("hid:") == true)
        {
            var reading = await ReadUsbHidBatteryAsync(device, cancellationToken);
            if (reading.HasValue)
                return reading;
        }

        // Bluetooth GATT handled by generic provider

        return null;
    }

    public async Task<IReadOnlyList<BatteryReading>> ReadAllBatteriesAsync(Device device, CancellationToken cancellationToken = default)
    {
        var reading = await ReadBatteryAsync(device, cancellationToken);
        return reading.HasValue ? [reading.Value] : [];
    }

    public async Task<IDisposable?> SubscribeToChangesAsync(Device device, Action<BatteryReading> callback, CancellationToken cancellationToken = default)
    {
        return null;
    }

    public bool SupportsNotifications(Device device) => false;

    public IDeviceBatteryReader? CreateDeviceReader(Device device)
    {
        if (!CanReadBattery(device))
            return null;

        return new AttackSharkR2Reader(device, _logger);
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }

    private async Task<BatteryReading?> ReadUsbHidBatteryAsync(Device device, CancellationToken cancellationToken)
    {
        try
        {
            var deviceId = device.DeviceInstanceId?.Substring(4);
            if (string.IsNullOrEmpty(deviceId))
                return null;

            var hidDevice = await HidDevice.FromIdAsync(deviceId, FileAccessMode.Read);
            if (hidDevice == null)
                return null;

            // Attack Shark R2 uses specific HID report for battery
            // This would need to be reverse-engineered from official software
            var report = await hidDevice.GetInputReportAsync(0);
            if (report == null)
                return null;

            var dataReader = DataReader.FromBuffer(report.Data);
            var bytes = new byte[report.Data.Length];
            dataReader.ReadBytes(bytes);

            var level = ParseAttackSharkBatteryReport(bytes);
            if (level.HasValue)
            {
                return new BatteryReading(
                    level.Value,
                    BatteryChargeState.Discharging,
                    BatteryHealth.Unknown,
                    BatterySource.ManufacturerProtocol,
                    componentId: "main",
                    componentName: "Мышь",
                    rawValue: bytes);
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read Attack Shark R2 USB HID battery");
            return null;
        }
    }

    private int? ParseAttackSharkBatteryReport(byte[] report)
    {
        // Attack Shark R2 HID report parsing
        // Placeholder - actual implementation requires protocol analysis
        // from official Attack Shark software
        //
        // Typical gaming mouse battery report structure:
        // Report ID: 0x02 (or similar)
        // Byte 0: Battery level (0-100)
        // Byte 1: Charging status (0 = discharging, 1 = charging, 2 = full)
        // Byte 2: DPI level or other status
        //
        // For now, return null
        return null;
    }

    private sealed class AttackSharkR2Reader : IDeviceBatteryReader
    {
        private readonly Device _device;
        private readonly ILogger _logger;
        private HidDevice? _hidDevice;

        public Device Device => _device;

        public AttackSharkR2Reader(Device device, ILogger logger)
        {
            _device = device;
            _logger = logger;
        }

        public async Task<BatteryReading?> ReadAsync(CancellationToken cancellationToken = default)
        {
            return null;
        }

        public async Task<IReadOnlyList<BatteryReading>> ReadAllAsync(CancellationToken cancellationToken = default)
        {
            return [];
        }

        public async Task<IDisposable?> SubscribeAsync(Action<BatteryReading> callback, CancellationToken cancellationToken = default)
        {
            return null;
        }

        public async ValueTask DisposeAsync()
        {
            _hidDevice?.Dispose();
        }
    }
}