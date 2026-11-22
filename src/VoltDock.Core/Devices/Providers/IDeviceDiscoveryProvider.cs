namespace VoltDock.Core.Devices.Providers;

using VoltDock.Core.Devices;

/// <summary>
/// Interface for device discovery providers.
/// Each provider discovers devices from a specific source (Bluetooth, USB, HID, etc.).
/// </summary>
public interface IDeviceDiscoveryProvider : IAsyncDisposable
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
    /// Gets the priority of this provider (higher = more preferred for deduplication).
    /// </summary>
    int Priority { get; }

    /// <summary>
    /// Gets whether this provider is currently active and discovering devices.
    /// </summary>
    bool IsActive { get; }

    /// <summary>
    /// Event raised when a new device is discovered.
    /// </summary>
    event EventHandler<DeviceDiscoveredEventArgs>? DeviceDiscovered;

    /// <summary>
    /// Event raised when a device is lost/removed.
    /// </summary>
    event EventHandler<DeviceLostEventArgs>? DeviceLost;

    /// <summary>
    /// Event raised when a device's properties are updated.
    /// </summary>
    event EventHandler<DeviceUpdatedEventArgs>? DeviceUpdated;

    /// <summary>
    /// Starts device discovery.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the start operation.</returns>
    Task StartAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops device discovery.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the stop operation.</returns>
    Task StopAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all currently known devices from this provider.
    /// </summary>
    /// <returns>Collection of discovered devices.</returns>
    IReadOnlyList<Device> GetDiscoveredDevices();

    /// <summary>
    /// Forces a refresh of device discovery.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the refresh operation.</returns>
    Task RefreshAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Event arguments for device discovery.
/// </summary>
public sealed class DeviceDiscoveredEventArgs : EventArgs
{
    /// <summary>
    /// The discovered device.
    /// </summary>
    public Device Device { get; }

    /// <summary>
    /// The provider that discovered the device.
    /// </summary>
    public string ProviderId { get; }

    public DeviceDiscoveredEventArgs(Device device, string providerId)
    {
        Device = device;
        ProviderId = providerId;
    }
}

/// <summary>
/// Event arguments for device loss.
/// </summary>
public sealed class DeviceLostEventArgs : EventArgs
{
    /// <summary>
    /// The ID of the lost device.
    /// </summary>
    public string DeviceId { get; }

    /// <summary>
    /// The provider that lost the device.
    /// </summary>
    public string ProviderId { get; }

    /// <summary>
    /// Reason for device loss.
    /// </summary>
    public string? Reason { get; }

    public DeviceLostEventArgs(string deviceId, string providerId, string? reason = null)
    {
        DeviceId = deviceId;
        ProviderId = providerId;
        Reason = reason;
    }
}

/// <summary>
/// Event arguments for device updates.
/// </summary>
public sealed class DeviceUpdatedEventArgs : EventArgs
{
    /// <summary>
    /// The updated device.
    /// </summary>
    public Device Device { get; }

    /// <summary>
    /// The provider that updated the device.
    /// </summary>
    public string ProviderId { get; }

    /// <summary>
    /// The properties that were updated.
    /// </summary>
    public IReadOnlyList<string> UpdatedProperties { get; }

    public DeviceUpdatedEventArgs(Device device, string providerId, IEnumerable<string> updatedProperties)
    {
        Device = device;
        ProviderId = providerId;
        UpdatedProperties = updatedProperties.ToList().AsReadOnly();
    }
}