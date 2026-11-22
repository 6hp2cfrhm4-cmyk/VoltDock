namespace VoltDock.Core.Battery;

using VoltDock.Core.Devices;

/// <summary>
/// Aggregates battery readings from multiple providers for a single device.
/// </summary>
public interface IBatteryReadingAggregator
{
    /// <summary>
    /// Aggregates readings from multiple providers into a single DeviceBatteryInfo.
    /// </summary>
    /// <param name="device">The device to aggregate for.</param>
    /// <param name="readings">Readings from different providers.</param>
    /// <returns>Aggregated battery info.</returns>
    DeviceBatteryInfo Aggregate(Device device, IReadOnlyList<ProviderBatteryReading> readings);

    /// <summary>
    /// Updates the device's battery info with a new reading from a provider.
    /// </summary>
    /// <param name="device">The device to update.</param>
    /// <param name="reading">The new reading.</param>
    /// <returns>True if the battery info was updated.</returns>
    bool UpdateReading(Device device, ProviderBatteryReading reading);

    /// <summary>
    /// Removes a provider's readings for a device (e.g., when provider becomes unavailable).
    /// </summary>
    /// <param name="device">The device.</param>
    /// <param name="providerId">The provider ID to remove.</param>
    /// <returns>True if any readings were removed.</returns>
    bool RemoveProviderReadings(Device device, string providerId);

    /// <summary>
    /// Gets the current aggregated battery info for a device.
    /// </summary>
    /// <param name="device">The device.</param>
    /// <returns>The aggregated battery info, or null if no readings available.</returns>
    DeviceBatteryInfo? GetAggregatedInfo(Device device);
}

/// <summary>
/// A battery reading tagged with its provider.
/// </summary>
public readonly record struct ProviderBatteryReading
{
    /// <summary>
    /// The provider ID that produced this reading.
    /// </summary>
    public string ProviderId { get; init; }

    /// <summary>
    /// The battery reading.
    /// </summary>
    public BatteryReading Reading { get; init; }

    /// <summary>
    /// The timestamp when this reading was received by the aggregator.
    /// </summary>
    public DateTimeOffset ReceivedAt { get; init; }

    public ProviderBatteryReading(string providerId, BatteryReading reading, DateTimeOffset? receivedAt = null)
    {
        ProviderId = providerId;
        Reading = reading;
        ReceivedAt = receivedAt ?? DateTimeOffset.UtcNow;
    }
}