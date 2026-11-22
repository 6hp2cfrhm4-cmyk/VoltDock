namespace VoltDock.Core.Battery;

using VoltDock.Core.Devices;
using Microsoft.Extensions.Logging;

/// <summary>
/// Base class for battery providers.
/// </summary>
public abstract class BatteryProviderBase : IBatteryProvider
{
    protected readonly ILogger _logger;

    protected BatteryProviderBase(ILogger logger)
    {
        _logger = logger;
    }

    public abstract string ProviderId { get; }
    public abstract string ProviderName { get; }
    public abstract BatterySource SourceType { get; }
    public abstract int Priority { get; }
    public abstract IReadOnlyList<DeviceType> SupportedDeviceTypes { get; }

    public abstract bool CanReadBattery(Device device);

    public abstract Task<BatteryReading?> ReadBatteryAsync(Device device, CancellationToken cancellationToken = default);

    public virtual async Task<IReadOnlyList<BatteryReading>> ReadAllBatteriesAsync(Device device, CancellationToken cancellationToken = default)
    {
        var reading = await ReadBatteryAsync(device, cancellationToken);
        return reading.HasValue ? [reading.Value] : [];
    }

    public virtual async Task<IDisposable?> SubscribeToChangesAsync(Device device, Action<BatteryReading> callback, CancellationToken cancellationToken = default)
    {
        // Default implementation: no subscription support
        return null;
    }

    public virtual bool SupportsNotifications(Device device) => false;

    public virtual async ValueTask DisposeAsync()
    {
        // Default: nothing to dispose
    }
}