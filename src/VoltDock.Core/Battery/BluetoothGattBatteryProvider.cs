namespace VoltDock.Core.Battery;

using VoltDock.Core.Devices;
using Microsoft.Extensions.Logging;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Foundation;
using Windows.Storage.Streams;

/// <summary>
/// Reads battery level from Bluetooth GATT Battery Service (0x180F).
/// </summary>
public sealed class BluetoothGattBatteryProvider : BatteryProviderBase
{
    private readonly Dictionary<string, GattDeviceService> _batteryServices = new();
    private readonly Dictionary<string, GattCharacteristic> _batteryCharacteristics = new();
    private readonly Dictionary<string, TypedEventHandler<GattCharacteristic, GattValueChangedEventArgs>> _notificationHandlers = new();

    public override string ProviderId => "bluetooth-gatt";
    public override string ProviderName => "Bluetooth GATT Battery Service";
    public override BatterySource SourceType => BatterySource.BluetoothGatt;
    public override int Priority => 90;
    public override IReadOnlyList<DeviceType> SupportedDeviceTypes { get; } =
        [DeviceType.Mouse, DeviceType.Keyboard, DeviceType.Headphones, DeviceType.TwsEarbuds, DeviceType.GameController, DeviceType.Trackpad, DeviceType.Presenter, DeviceType.Stylus, DeviceType.Wearable, DeviceType.Smartphone];

    public BluetoothGattBatteryProvider(ILogger<BluetoothGattBatteryProvider> logger) : base(logger) { }

    public override bool CanReadBattery(Device device)
    {
        return SupportedDeviceTypes.Contains(device.Type) &&
               !string.IsNullOrEmpty(device.BluetoothAddress) &&
               device.DeviceInstanceId?.StartsWith("ble:") == true;
    }

    public override async Task<BatteryReading?> ReadBatteryAsync(Device device, CancellationToken cancellationToken = default)
    {
        if (!CanReadBattery(device))
            return null;

        try
        {
            var characteristic = await GetOrCreateCharacteristicAsync(device, cancellationToken);
            if (characteristic == null)
                return null;

            var result = await characteristic.ReadValueAsync(BluetoothCacheMode.Uncached).AsTask(cancellationToken);
            if (result.Status != GattCommunicationStatus.Success)
            {
                _logger.LogDebug("GATT read failed for {DeviceId}: {Status}", device.Id, result.Status);
                return null;
            }

            var level = ParseBatteryLevel(result.Value);
            if (!level.HasValue)
                return null;

            var chargeState = BatteryChargeState.Discharging;
            // Could read battery charge state characteristic (0x2B29) if available

            return new BatteryReading(
                level.Value,
                chargeState,
                BatteryHealth.Unknown,
                BatterySource.BluetoothGatt,
                componentId: "main",
                componentName: "Battery");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read GATT battery for {DeviceId}", device.Id);
            return null;
        }
    }

    public override async Task<IReadOnlyList<BatteryReading>> ReadAllBatteriesAsync(Device device, CancellationToken cancellationToken = default)
    {
        var readings = new List<BatteryReading>();

        // For TWS earbuds, there might be multiple battery services
        // Read primary battery
        var primary = await ReadBatteryAsync(device, cancellationToken);
        if (primary.HasValue)
            readings.Add(primary.Value);

        // TODO: Enumerate multiple battery services for TWS
        // This would require device-specific logic

        return readings;
    }

    public override async Task<IDisposable?> SubscribeToChangesAsync(Device device, Action<BatteryReading> callback, CancellationToken cancellationToken = default)
    {
        if (!CanReadBattery(device))
            return null;

        try
        {
            var characteristic = await GetOrCreateCharacteristicAsync(device, cancellationToken);
            if (characteristic == null)
                return null;

            var key = device.Id;
            if (_notificationHandlers.ContainsKey(key))
                return null; // Already subscribed

            var handler = new TypedEventHandler<GattCharacteristic, GattValueChangedEventArgs>((sender, args) =>
            {
                var level = ParseBatteryLevel(args.CharacteristicValue);
                if (level.HasValue)
                {
                    var reading = new BatteryReading(
                        level.Value,
                        BatteryChargeState.Discharging,
                        BatteryHealth.Unknown,
                        BatterySource.BluetoothGatt,
                        componentId: "main",
                        componentName: "Battery");
                    callback(reading);
                }
            });

            _notificationHandlers[key] = handler;
            characteristic.ValueChanged += handler;

            var result = await characteristic.WriteClientCharacteristicConfigurationDescriptorAsync(
                GattClientCharacteristicConfigurationDescriptorValue.Notify).AsTask(cancellationToken);

            if (result != GattCommunicationStatus.Success)
            {
                _logger.LogWarning("Failed to enable GATT notifications for {DeviceId}: {Status}", device.Id, result);
                characteristic.ValueChanged -= handler;
                _notificationHandlers.Remove(key);
                return null;
            }

            return new NotificationSubscription(() =>
            {
                characteristic.ValueChanged -= handler;
                _notificationHandlers.Remove(key);
                _ = characteristic.WriteClientCharacteristicConfigurationDescriptorAsync(
                    GattClientCharacteristicConfigurationDescriptorValue.None);
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to subscribe to GATT notifications for {DeviceId}", device.Id);
            return null;
        }
    }

    public override bool SupportsNotifications(Device device) => CanReadBattery(device);

    private async Task<GattCharacteristic?> GetOrCreateCharacteristicAsync(Device device, CancellationToken cancellationToken)
    {
        var key = device.Id;

        if (_batteryCharacteristics.TryGetValue(key, out var cached))
            return cached;

        try
        {
            var bleDevice = await BluetoothLEDevice.FromIdAsync(device.DeviceInstanceId!.Substring(4));
            if (bleDevice == null)
                return null;

            // Get Battery Service (0x180F)
            var services = await bleDevice.GetGattServicesForUuidAsync(GattServiceUuids.Battery).AsTask(cancellationToken);
            if (services.Status != GattCommunicationStatus.Success || services.Services.Count == 0)
                return null;

            var batteryService = services.Services[0];
            _batteryServices[key] = batteryService;

            // Get Battery Level Characteristic (0x2A19)
            var characteristics = await batteryService.GetCharacteristicsForUuidAsync(GattCharacteristicUuids.BatteryLevel).AsTask(cancellationToken);
            if (characteristics.Status != GattCommunicationStatus.Success || characteristics.Characteristics.Count == 0)
                return null;

            var characteristic = characteristics.Characteristics[0];
            _batteryCharacteristics[key] = characteristic;

            return characteristic;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get GATT characteristic for {DeviceId}", device.Id);
            return null;
        }
    }

    private int? ParseBatteryLevel(IBuffer buffer)
    {
        if (buffer == null || buffer.Length < 1)
            return null;

        var dataReader = DataReader.FromBuffer(buffer);
        var level = dataReader.ReadByte();
        return Math.Clamp(level, 0, 100);
    }

    public override async ValueTask DisposeAsync()
    {
        foreach (var kvp in _notificationHandlers)
        {
            if (_batteryCharacteristics.TryGetValue(kvp.Key, out var characteristic))
            {
                characteristic.ValueChanged -= kvp.Value;
                await characteristic.WriteClientCharacteristicConfigurationDescriptorAsync(
                    GattClientCharacteristicConfigurationDescriptorValue.None);
            }
        }
        _notificationHandlers.Clear();
        _batteryCharacteristics.Clear();
        _batteryServices.Clear();
    }

    private sealed class NotificationSubscription : IDisposable
    {
        private readonly Action _unsubscribe;
        public NotificationSubscription(Action unsubscribe) => _unsubscribe = unsubscribe;
        public void Dispose() => _unsubscribe();
    }
}