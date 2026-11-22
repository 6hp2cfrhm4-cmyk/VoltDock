namespace VoltDock.Core.Devices;

/// <summary>
/// Provides metadata for devices (names, icons, capabilities, etc.).
/// </summary>
public interface IDeviceMetadataProvider
{
    /// <summary>
    /// Gets the provider ID.
    /// </summary>
    string ProviderId { get; }

    /// <summary>
    /// Gets the priority (higher = more preferred).
    /// </summary>
    int Priority { get; }

    /// <summary>
    /// Tries to get metadata for a device.
    /// </summary>
    /// <param name="device">The device to get metadata for.</param>
    /// <param name="metadata">The metadata if found.</param>
    /// <returns>True if metadata was provided.</returns>
    bool TryGetMetadata(Device device, out DeviceMetadata metadata);

    /// <summary>
    /// Gets the default icon for a device type.
    /// </summary>
    /// <param name="type">The device type.</param>
    /// <returns>Icon resource path or null.</returns>
    string? GetDefaultIcon(DeviceType type);
}

/// <summary>
/// Metadata for a device.
/// </summary>
public sealed class DeviceMetadata
{
    /// <summary>
    /// Friendly display name.
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// Manufacturer name.
    /// </summary>
    public string? Manufacturer { get; set; }

    /// <summary>
    /// Model name/number.
    /// </summary>
    public string? Model { get; set; }

    /// <summary>
    /// Device type classification.
    /// </summary>
    public DeviceType? Type { get; set; }

    /// <summary>
    /// Icon resource path.
    /// </summary>
    public string? IconPath { get; set; }

    /// <summary>
    /// Whether this device supports multi-battery reporting (TWS).
    /// </summary>
    public bool SupportsMultiBattery { get; set; }

    /// <summary>
    /// Battery component names for multi-battery devices.
    /// </summary>
    public IReadOnlyList<string> BatteryComponentNames { get; set; } = [];

    /// <summary>
    /// Known battery source priorities for this device.
    /// </summary>
    public IReadOnlyList<BatterySource> PreferredSources { get; set; } = [];

    /// <summary>
    /// Additional custom properties.
    /// </summary>
    public Dictionary<string, object> Properties { get; } = [];
}