namespace VoltDock.Core.Devices.Providers;

using VoltDock.Core.Devices;
using Microsoft.Extensions.Logging;

/// <summary>
/// Base class for device discovery providers.
/// </summary>
public abstract class DiscoveryProviderBase : IDeviceDiscoveryProvider
{
    protected readonly ILogger _logger;
    protected bool _isActive;

    protected DiscoveryProviderBase(ILogger logger)
    {
        _logger = logger;
    }

    public abstract string ProviderId { get; }
    public abstract string ProviderName { get; }
    public abstract int Priority { get; }

    public bool IsActive => _isActive;

    public event EventHandler<DeviceDiscoveredEventArgs>? DeviceDiscovered;
    public event EventHandler<DeviceLostEventArgs>? DeviceLost;
    public event EventHandler<DeviceUpdatedEventArgs>? DeviceUpdated;

    public abstract Task StartAsync(CancellationToken cancellationToken = default);

    public abstract Task StopAsync(CancellationToken cancellationToken = default);

    public abstract IReadOnlyList<Device> GetDiscoveredDevices();

    public abstract Task RefreshAsync(CancellationToken cancellationToken = default);

    protected void RaiseDeviceDiscovered(Device device)
    {
        DeviceDiscovered?.Invoke(this, new DeviceDiscoveredEventArgs(device, ProviderId));
    }

    protected void RaiseDeviceLost(string deviceId, string? reason = null)
    {
        DeviceLost?.Invoke(this, new DeviceLostEventArgs(deviceId, ProviderId, reason));
    }

    protected void RaiseDeviceUpdated(Device device, IEnumerable<string> updatedProperties)
    {
        DeviceUpdated?.Invoke(this, new DeviceUpdatedEventArgs(device, ProviderId, updatedProperties));
    }

    protected virtual void SetActive(bool active)
    {
        _isActive = active;
    }

    public virtual async ValueTask DisposeAsync()
    {
        if (_isActive)
        {
            await StopAsync(CancellationToken.None);
        }
    }
}