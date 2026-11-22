namespace VoltDock.Core.Battery;

using VoltDock.Core.Devices;

/// <summary>
/// Interface for battery information providers.
/// Each provider reads battery levels from a specific source (GATT, HID, XInput, etc.).
/// </summary>
public interface IBatteryProvider : IAsyncDisposable
{
    /// <summary>
    /// Gets the unique identifier of this provider.
    /// </summary>
    string ProviderId { get; }

    /// <summary>
    /// Gets the human-readable name of this provider.
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Gets the battery source type this provider reads from.
    /// </summary>
    BatterySource SourceType { get; }

    /// <summary>
    /// Gets the priority of this provider (higher = more preferred).
    /// </summary>
    int Priority { get; }

    /// <summary>
    /// Gets whether this provider can read battery for the given device.
    /// </summary>
    /// <param name="device">The device to check.</param>
    /// <returns>True if this provider can read battery for the device.</returns>
    bool CanReadBattery(Device device);

    /// <summary>
    /// Gets the supported device types for this provider.
    /// </summary>
    IReadOnlyList<DeviceType> SupportedDeviceTypes { get; }

    /// <summary>
    /// Reads the current battery level for a device.
    /// </summary>
    /// <param name="device">The device to read battery for.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A battery reading, or null if reading failed.</returns>
    Task<BatteryReading?> ReadBatteryAsync(Device device, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads battery levels for all batteries of a multi-battery device (e.g., TWS earbuds).
    /// </summary>
    /// <param name="device">The device to read batteries for.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of battery readings for each component.</returns>
    Task<IReadOnlyList<BatteryReading>> ReadAllBatteriesAsync(Device device, CancellationToken cancellationToken = default);

    /// <summary>
    /// Subscribes to battery change notifications for a device (if supported).
    /// </summary>
    /// <param name="device">The device to monitor.</param>
    /// <param name="callback">Callback invoked when battery changes.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A subscription handle that can be disposed to unsubscribe.</returns>
    Task<IDisposable?> SubscribeToChangesAsync(Device device, Action<BatteryReading> callback, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if the provider supports real-time notifications for the device.
    /// </summary>
    /// <param name="device">The device to check.</param>
    /// <returns>True if notifications are supported.</returns>
    bool SupportsNotifications(Device device);
}

/// <summary>
/// Interface for manufacturer-specific battery providers.
/// </summary>
public interface IManufacturerBatteryProvider : IBatteryProvider
{
    /// <summary>
    /// Gets the manufacturer name this provider supports.
    /// </summary>
    string ManufacturerName { get; }

    /// <summary>
    /// Gets the supported model identifiers (VID/PID pairs or model strings).
    /// </summary>
    IReadOnlyList<ManufacturerModelId> SupportedModels { get; }

    /// <summary>
    /// Tries to create a device-specific battery reader for the given device.
    /// </summary>
    /// <param name="device">The device to create a reader for.</param>
    /// <returns>A device-specific reader, or null if not supported.</returns>
    IDeviceBatteryReader? CreateDeviceReader(Device device);
}

/// <summary>
/// Represents a manufacturer-specific model identifier.
/// </summary>
public readonly record struct ManufacturerModelId
{
    /// <summary>
    /// USB Vendor ID.
    /// </summary>
    public ushort? VendorId { get; init; }

    /// <summary>
    /// USB Product ID.
    /// </summary>
    public ushort? ProductId { get; init; }

    /// <summary>
    /// Bluetooth device name pattern (regex).
    /// </summary>
    string? BluetoothNamePattern { get; init; }

    /// <summary>
    /// Device model string pattern (regex).
    /// </summary>
    string? ModelPattern { get; init; }

    /// <summary>
    /// Creates a model ID from VID/PID.
    /// </summary>
    public static ManufacturerModelId FromUsb(ushort vendorId, ushort productId) =>
        new() { VendorId = vendorId, ProductId = productId };

    /// <summary>
    /// Creates a model ID from Bluetooth name pattern.
    /// </summary>
    public static ManufacturerModelId FromBluetoothName(string pattern) =>
        new() { BluetoothNamePattern = pattern };

    /// <summary>
    /// Creates a model ID from model string pattern.
    /// </summary>
    public static ManufacturerModelId FromModel(string pattern) =>
        new() { ModelPattern = pattern };

    /// <summary>
    /// Checks if this model ID matches the given device.
    /// </summary>
    public bool Matches(Device device)
    {
        if (VendorId.HasValue && ProductId.HasValue &&
            device.UsbVendorId == VendorId && device.UsbProductId == ProductId)
            return true;

        if (!string.IsNullOrEmpty(BluetoothNamePattern) &&
            !string.IsNullOrEmpty(device.Name) &&
            System.Text.RegularExpressions.Regex.IsMatch(device.Name, BluetoothNamePattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase))
            return true;

        if (!string.IsNullOrEmpty(ModelPattern) &&
            !string.IsNullOrEmpty(device.Model) &&
            System.Text.RegularExpressions.Regex.IsMatch(device.Model, ModelPattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase))
            return true;

        return false;
    }
}

/// <summary>
/// Interface for device-specific battery readers.
/// Allows manufacturer providers to implement custom reading logic per device model.
/// </summary>
public interface IDeviceBatteryReader : IAsyncDisposable
{
    /// <summary>
    /// Gets the device this reader is for.
    /// </summary>
    Device Device { get; }

    /// <summary>
    /// Reads the battery level.
    /// </summary>
    Task<BatteryReading?> ReadAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads all batteries (for multi-battery devices).
    /// </summary>
    Task<IReadOnlyList<BatteryReading>> ReadAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Subscribes to battery changes.
    /// </summary>
    Task<IDisposable?> SubscribeAsync(Action<BatteryReading> callback, CancellationToken cancellationToken = default);
}