namespace VoltDock.Core.Battery;

using VoltDock.Core.Devices;

/// <summary>
/// Aggregates battery readings from multiple providers.
/// </summary>
public sealed class BatteryReadingAggregator : IBatteryReadingAggregator
{
    private readonly Dictionary<string, Dictionary<string, ProviderBatteryReading>> _deviceReadings = new();
    private readonly object _lock = new();

    public DeviceBatteryInfo Aggregate(Device device, IReadOnlyList<ProviderBatteryReading> readings)
    {
        lock (_lock)
        {
            if (!_deviceReadings.TryGetValue(device.Id, out var providerReadings))
            {
                providerReadings = new Dictionary<string, ProviderBatteryReading>(StringComparer.OrdinalIgnoreCase);
                _deviceReadings[device.Id] = providerReadings;
            }

            foreach (var reading in readings)
            {
                providerReadings[reading.ProviderId] = reading;
            }

            return BuildAggregatedInfo(device, providerReadings.Values);
        }
    }

    public bool UpdateReading(Device device, ProviderBatteryReading reading)
    {
        lock (_lock)
        {
            if (!_deviceReadings.TryGetValue(device.Id, out var providerReadings))
            {
                providerReadings = new Dictionary<string, ProviderBatteryReading>(StringComparer.OrdinalIgnoreCase);
                _deviceReadings[device.Id] = providerReadings;
            }

            var changed = !providerReadings.TryGetValue(reading.ProviderId, out var existing) ||
                          existing.Reading.LevelPercent != reading.Reading.LevelPercent ||
                          existing.Reading.ChargeState != reading.Reading.ChargeState;

            providerReadings[reading.ProviderId] = reading;

            if (changed)
            {
                var aggregated = BuildAggregatedInfo(device, providerReadings.Values);
                device.BatteryInfo.PrimaryReading = aggregated.PrimaryReading;
                device.BatteryInfo.AdditionalReadings = aggregated.AdditionalReadings;
            }

            return changed;
        }
    }

    public bool RemoveProviderReadings(Device device, string providerId)
    {
        lock (_lock)
        {
            if (!_deviceReadings.TryGetValue(device.Id, out var providerReadings))
                return false;

            var removed = providerReadings.Remove(providerId);
            if (removed && providerReadings.Count > 0)
            {
                var aggregated = BuildAggregatedInfo(device, providerReadings.Values);
                device.BatteryInfo.PrimaryReading = aggregated.PrimaryReading;
                device.BatteryInfo.AdditionalReadings = aggregated.AdditionalReadings;
            }
            else if (removed)
            {
                device.BatteryInfo.PrimaryReading = BatteryReading.Unknown(BatterySource.Unknown);
                device.BatteryInfo.AdditionalReadings = [];
            }

            return removed;
        }
    }

    public DeviceBatteryInfo? GetAggregatedInfo(Device device)
    {
        lock (_lock)
        {
            if (!_deviceReadings.TryGetValue(device.Id, out var providerReadings))
                return null;

            return BuildAggregatedInfo(device, providerReadings.Values);
        }
    }

    private DeviceBatteryInfo BuildAggregatedInfo(Device device, IEnumerable<ProviderBatteryReading> readings)
    {
        var info = new DeviceBatteryInfo();
        var readingList = readings.ToList();

        if (readingList.Count == 0)
            return info;

        // Sort by provider priority (higher priority first)
        // For now, just use the first valid reading as primary
        var primaryReading = readingList
            .Where(r => r.Reading.HasLevel)
            .OrderByDescending(r => GetProviderPriority(r.ProviderId))
            .FirstOrDefault();

        if (primaryReading.Reading.HasLevel)
        {
            info.PrimaryReading = primaryReading.Reading;
        }
        else
        {
            // No valid level, use the most recent reading
            var latest = readingList.MaxBy(r => r.ReceivedAt);
            info.PrimaryReading = latest.Reading;
        }

        // Additional readings (for multi-battery devices)
        var additional = readingList
            .Where(r => r != primaryReading && r.Reading.HasLevel)
            .Select(r => r.Reading)
            .ToList();

        info.AdditionalReadings = additional;

        return info;
    }

    private int GetProviderPriority(string providerId)
    {
        return providerId switch
        {
            "bluetooth-gatt" => 100,
            "hid-battery" => 90,
            "xinput-battery" => 100,
            "windows-power-battery" => 100,
            "device-properties" => 50,
            _ => 0
        };
    }
}