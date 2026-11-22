namespace VoltDock.Core.Devices;

/// <summary>
/// Resolves icons for devices.
/// </summary>
public interface IDeviceIconResolver
{
    /// <summary>
    /// Gets the icon for a device.
    /// </summary>
    /// <param name="device">The device.</param>
    /// <param name="size">Desired icon size (optional).</param>
    /// <returns>Icon image source or null.</returns>
    Task<object?> GetIconAsync(Device device, int? size = null);

    /// <summary>
    /// Gets the icon for a device type.
    /// </summary>
    /// <param name="type">The device type.</param>
    /// <param name="size">Desired icon size.</param>
    /// <returns>Icon image source or null.</returns>
    Task<object?> GetIconAsync(DeviceType type, int? size = null);

    /// <summary>
    /// Gets the charging animation icon for a device.
    /// </summary>
    /// <param name="device">The device.</param>
    /// <returns>Animated icon or null.</returns>
    Task<object?> GetChargingIconAsync(Device device);

    /// <summary>
    /// Preloads icons for a device (for smooth animations).
    /// </summary>
    /// <param name="device">The device.</param>
    Task PreloadIconsAsync(Device device);

    /// <summary>
    /// Clears cached icons.
    /// </summary>
    void ClearCache();
}