namespace VoltDock.Core.Devices;

/// <summary>
/// Represents a physical device detected by the system.
/// </summary>
public sealed class Device : IEquatable<Device>
{
    /// <summary>
    /// Unique persistent identifier for this device instance.
    /// </summary>
    public string Id { get; init; }

    /// <summary>
    /// Human-readable name of the device.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Type/classification of the device.
    /// </summary>
    public DeviceType Type { get; set; } = DeviceType.Unknown;

    /// <summary>
    /// Manufacturer name (if available).
    /// </summary>
    public string? Manufacturer { get; set; }

    /// <summary>
    /// Model name/number (if available).
    /// </summary>
    public string? Model { get; set; }

    /// <summary>
    /// Hardware revision (if available).
    /// </summary>
    public string? HardwareRevision { get; set; }

    /// <summary>
    /// Firmware version (if available).
    /// </summary>
    public string? FirmwareVersion { get; set; }

    /// <summary>
    /// Serial number (if available).
    /// </summary>
    public string? SerialNumber { get; set; }

    /// <summary>
    /// Bluetooth MAC address (if Bluetooth device).
    /// </summary>
    public string? BluetoothAddress { get; set; }

    /// <summary>
    /// USB Vendor ID (if USB device).
    /// </summary>
    public ushort? UsbVendorId { get; set; }

    /// <summary>
    /// USB Product ID (if USB device).
    /// </summary>
    public ushort? UsbProductId { get; set; }

    /// <summary>
    /// USB device version (if available).
    /// </summary>
    public ushort? UsbVersion { get; set; }

    /// <summary>
    /// Path to the device icon resource.
    /// </summary>
    public string? IconPath { get; set; }

    /// <summary>
    /// Current connection state.
    /// </summary>
    public DeviceConnectionState ConnectionState { get; set; } = DeviceConnectionState.Unknown;

    /// <summary>
    /// Battery information for this device.
    /// </summary>
    public DeviceBatteryInfo BatteryInfo { get; init; } = new();

    /// <summary>
    /// Sources that provided battery readings for this device.
    /// </summary>
    public HashSet<BatterySource> BatterySources { get; init; } = [];

    /// <summary>
    /// Device interface paths (HID, Bluetooth, etc.) for deduplication.
    /// </summary>
    public HashSet<string> InterfacePaths { get; init; } = [];

    /// <summary>
    /// Windows Device Instance ID (for PnP devices).
    /// </summary>
    public string? DeviceInstanceId { get; set; }

    /// <summary>
    /// Windows Device Container ID (groups interfaces of same physical device).
    /// </summary>
    public string? DeviceContainerId { get; set; }

    /// <summary>
    /// Custom metadata from manufacturer-specific providers.
    /// </summary>
    public Dictionary<string, object> Metadata { get; init; } = [];

    /// <summary>
    /// Timestamp when the device was first discovered.
    /// </summary>
    public DateTimeOffset FirstSeen { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Timestamp when the device was last updated.
    /// </summary>
    public DateTimeOffset LastSeen { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Whether this device is marked as a favorite/pinned in the dock.
    /// </summary>
    public bool IsPinned { get; set; }

    /// <summary>
    /// Whether to hide this device when disconnected.
    /// </summary>
    public bool HideWhenDisconnected { get; set; }

    /// <summary>
    /// Custom display name override.
    /// </summary>
    public string? CustomName { get; set; }

    /// <summary>
    /// Creates a new device with the specified ID.
    /// </summary>
    public Device(string id)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
    }

    /// <summary>
    /// Gets the effective display name for this device.
    /// </summary>
    public string DisplayName => CustomName ?? Name ?? Id;

    /// <summary>
    /// Gets the effective icon name for this device.
    /// </summary>
    public string EffectiveIconName => IconPath ?? Type.GetDefaultIconName();

    /// <summary>
    /// Gets whether the device is currently connected and has recent battery data.
    /// </summary>
    public bool IsActive => ConnectionState == DeviceConnectionState.Connected && BatteryInfo.HasAnyLevel;

    /// <summary>
    /// Gets whether this device has any battery data available.
    /// </summary>
    public bool HasBatteryData => BatteryInfo.HasAnyLevel;

    /// <summary>
    /// Updates the last seen timestamp.
    /// </summary>
    public void Touch() => LastSeen = DateTimeOffset.UtcNow;

    /// <summary>
    /// Adds an interface path for deduplication.
    /// </summary>
    public void AddInterfacePath(string path)
    {
        if (!string.IsNullOrWhiteSpace(path))
            InterfacePaths.Add(path);
    }

    /// <summary>
    /// Adds a battery source.
    /// </summary>
    public void AddBatterySource(BatterySource source)
    {
        if (source != BatterySource.Unknown)
            BatterySources.Add(source);
    }

    /// <summary>
    /// Determines if this device represents the same physical device as another.
    /// </summary>
    public bool IsSamePhysicalDevice(Device other)
    {
        if (other == null)
            return false;

        // Same container ID is strongest indicator
        if (!string.IsNullOrEmpty(DeviceContainerId) &&
            !string.IsNullOrEmpty(other.DeviceContainerId) &&
            DeviceContainerId.Equals(other.DeviceContainerId, StringComparison.OrdinalIgnoreCase))
            return true;

        // Same Bluetooth address
        if (!string.IsNullOrEmpty(BluetoothAddress) &&
            !string.IsNullOrEmpty(other.BluetoothAddress) &&
            BluetoothAddress.Equals(other.BluetoothAddress, StringComparison.OrdinalIgnoreCase))
            return true;

        // Same USB VID/PID and serial
        if (UsbVendorId.HasValue && UsbProductId.HasValue &&
            other.UsbVendorId.HasValue && other.UsbProductId.HasValue &&
            UsbVendorId == other.UsbVendorId &&
            UsbProductId == other.UsbProductId &&
            !string.IsNullOrEmpty(SerialNumber) &&
            !string.IsNullOrEmpty(other.SerialNumber) &&
            SerialNumber.Equals(other.SerialNumber, StringComparison.OrdinalIgnoreCase))
            return true;

        // Overlapping interface paths
        if (InterfacePaths.Count > 0 && other.InterfacePaths.Count > 0 &&
            InterfacePaths.Intersect(other.InterfacePaths, StringComparer.OrdinalIgnoreCase).Any())
            return true;

        return false;
    }

    public bool Equals(Device? other) => other is not null && Id == other.Id;

    public override bool Equals(object? obj) => Equals(obj as Device);

    public override int GetHashCode() => Id.GetHashCode();

    public static bool operator ==(Device? left, Device? right) =>
        left?.Equals(right) ?? right is null;

    public static bool operator !=(Device? left, Device? right) =>
        !(left == right);
}

/// <summary>
/// Represents the connection state of a device.
/// </summary>
public enum DeviceConnectionState
{
    /// <summary>
    /// State is unknown.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Device is connected and communicating.
    /// </summary>
    Connected = 1,

    /// <summary>
    /// Device is paired but not currently connected.
    /// </summary>
    Paired = 2,

    /// <summary>
    /// Device is disconnected.
    /// </summary>
    Disconnected = 3,

    /// <summary>
    /// Device is connecting.
    /// </summary>
    Connecting = 4,

    /// <summary>
    /// Device is in low power / sleep mode.
    /// </summary>
    LowPower = 5,
}