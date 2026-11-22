namespace VoltDock.Core.Devices;

/// <summary>
/// Resolves and merges device identities from multiple discovery providers.
/// Handles deduplication of the same physical device appearing through multiple interfaces.
/// </summary>
public interface IDeviceIdentityResolver
{
    /// <summary>
    /// Registers a device from a discovery provider.
    /// Returns the canonical device instance (either existing or new).
    /// </summary>
    /// <param name="device">The device discovered by a provider.</param>
    /// <param name="providerId">The provider that discovered it.</param>
    /// <returns>The canonical device instance.</returns>
    Device RegisterDevice(Device device, string providerId);

    /// <summary>
    /// Unregisters a device from a provider.
    /// If no other providers reference the device, it's removed entirely.
    /// </summary>
    /// <param name="deviceId">The device ID.</param>
    /// <param name="providerId">The provider that lost the device.</param>
    /// <returns>True if the device was completely removed.</returns>
    bool UnregisterDevice(string deviceId, string providerId);

    /// <summary>
    /// Updates a device's properties from a provider.
    /// </summary>
    /// <param name="deviceId">The canonical device ID.</param>
    /// <param name="providerId">The provider providing the update.</param>
    /// <param name="updateAction">Action to apply updates to the canonical device.</param>
    /// <returns>True if the device was updated.</returns>
    bool UpdateDevice(string deviceId, string providerId, Action<Device> updateAction);

    /// <summary>
    /// Gets the canonical device by ID.
    /// </summary>
    Device? GetDevice(string deviceId);

    /// <summary>
    /// Gets all canonical devices.
    /// </summary>
    IReadOnlyList<Device> GetAllDevices();

    /// <summary>
    /// Gets all provider IDs that contribute to a device.
    /// </summary>
    IReadOnlyList<string> GetProviderIdsForDevice(string deviceId);

    /// <summary>
    /// Finds devices matching a predicate.
    /// </summary>
    IReadOnlyList<Device> FindDevices(Func<Device, bool> predicate);

    /// <summary>
    /// Event raised when a new canonical device is created.
    /// </summary>
    event EventHandler<DeviceEventArgs>? DeviceCreated;

    /// <summary>
    /// Event raised when a canonical device is removed.
    /// </summary>
    event EventHandler<DeviceEventArgs>? DeviceRemoved;

    /// <summary>
    /// Event raised when a canonical device is updated.
    /// </summary>
    event EventHandler<DeviceUpdatedEventArgs>? DeviceUpdated;
}

/// <summary>
/// Event arguments for device identity events.
/// </summary>
public sealed class DeviceEventArgs : EventArgs
{
    public Device Device { get; }
    public DeviceEventArgs(Device device) => Device = device;
}

/// <summary>
/// Event arguments for device updates.
/// </summary>
public sealed class DeviceUpdatedEventArgs : EventArgs
{
    public Device Device { get; }
    public IReadOnlyList<string> UpdatedProperties { get; }
    public DeviceUpdatedEventArgs(Device device, IEnumerable<string> updatedProperties)
    {
        Device = device;
        UpdatedProperties = updatedProperties.ToList().AsReadOnly();
    }
}