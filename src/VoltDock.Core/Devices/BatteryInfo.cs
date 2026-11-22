namespace VoltDock.Core.Devices;

/// <summary>
/// Represents the charging state of a battery.
/// </summary>
public enum BatteryChargeState
{
    /// <summary>
    /// Charge state is unknown.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Battery is discharging (not charging).
    /// </summary>
    Discharging = 1,

    /// <summary>
    /// Battery is currently charging.
    /// </summary>
    Charging = 2,

    /// <summary>
    /// Battery is fully charged.
    /// </summary>
    Full = 3,

    /// <summary>
    /// Battery is not present.
    /// </summary>
    NotPresent = 4,
}

/// <summary>
/// Represents the health/condition of a battery.
/// </summary>
public enum BatteryHealth
{
    /// <summary>
    /// Health status is unknown.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Battery is in good condition.
    /// </summary>
    Good = 1,

    /// <summary>
    /// Battery is degraded but functional.
    /// </summary>
    Degraded = 2,

    /// <summary>
    /// Battery is in poor condition and may need replacement.
    /// </summary>
    Poor = 3,

    /// <summary>
    /// Battery is dead or critically failed.
    /// </summary>
    Dead = 4,
}

/// <summary>
/// Represents the source of battery information.
/// </summary>
public enum BatterySource
{
    /// <summary>
    /// Unknown source.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Windows Bluetooth GATT Battery Service (0x180F).
    /// </summary>
    BluetoothGatt = 1,

    /// <summary>
    /// Windows Bluetooth Classic device properties.
    /// </summary>
    BluetoothClassic = 2,

    /// <summary>
    /// HID Battery Report (standard HID usage page 0x000C).
    /// </summary>
    HidReport = 3,

    /// <summary>
    /// XInput battery information (game controllers).
    /// </summary>
    XInput = 4,

    /// <summary>
    /// Windows Power API (internal laptop battery).
    /// </summary>
    WindowsPower = 5,

    /// <summary>
    /// Windows Device Enumeration properties (System.Devices.BatteryLevel).
    /// </summary>
    DeviceProperties = 6,

    /// <summary>
    /// Manufacturer-specific protocol (Logitech, Razer, etc.).
    /// </summary>
    ManufacturerProtocol = 7,

    /// <summary>
    /// USB device-specific battery reporting.
    /// </summary>
    UsbDevice = 8,

    /// <summary>
    /// UWP Battery API (Windows.Devices.Power).
    /// </summary>
    UwpBattery = 9,

    /// <summary>
    /// Demo/test data (not real hardware).
    /// </summary>
    Demo = 10,
}

/// <summary>
/// Represents a single battery reading with metadata.
/// </summary>
public readonly record struct BatteryReading
{
    /// <summary>
    /// Battery level as percentage (0-100), or null if unknown.
    /// </summary>
    public readonly int? LevelPercent;

    /// <summary>
    /// Current charging state.
    /// </summary>
    public readonly BatteryChargeState ChargeState;

    /// <summary>
    /// Battery health condition.
    /// </summary>
    public readonly BatteryHealth Health;

    /// <summary>
    /// Source of this reading.
    /// </summary>
    public readonly BatterySource Source;

    /// <summary>
    /// Timestamp when the reading was obtained.
    /// </summary>
    public readonly DateTimeOffset Timestamp;

    /// <summary>
    /// Unique identifier of the battery component (for multi-battery devices).
    /// </summary>
    public readonly string? ComponentId;

    /// <summary>
    /// Human-readable name of the battery component.
    /// </summary>
    public readonly string? ComponentName;

    /// <summary>
    /// Raw value as reported by the source (if applicable).
    /// </summary>
    public readonly object? RawValue;

    /// <summary>
    /// Creates a new battery reading.
    /// </summary>
    public BatteryReading(
        int? levelPercent,
        BatteryChargeState chargeState = BatteryChargeState.Unknown,
        BatteryHealth health = BatteryHealth.Unknown,
        BatterySource source = BatterySource.Unknown,
        string? componentId = null,
        string? componentName = null,
        object? rawValue = null)
    {
        LevelPercent = levelPercent;
        ChargeState = chargeState;
        Health = health;
        Source = source;
        Timestamp = DateTimeOffset.UtcNow;
        ComponentId = componentId;
        ComponentName = componentName;
        RawValue = rawValue;
    }

    /// <summary>
    /// Creates a reading with unknown level.
    /// </summary>
    public static BatteryReading Unknown(BatterySource source, string? componentId = null, string? componentName = null) =>
        new(null, BatteryChargeState.Unknown, BatteryHealth.Unknown, source, componentId, componentName);

    /// <summary>
    /// Creates a reading for a charging battery with unknown level.
    /// </summary>
    public static BatteryReading Charging(BatterySource source, string? componentId = null, string? componentName = null) =>
        new(null, BatteryChargeState.Charging, BatteryHealth.Unknown, source, componentId, componentName);

    /// <summary>
    /// Gets whether this reading has a valid level percentage.
    /// </summary>
    public bool HasLevel => LevelPercent.HasValue;

    /// <summary>
    /// Gets whether the reading is considered stale (older than 5 minutes).
    /// </summary>
    public bool IsStale(TimeSpan? maxAge = null) =>
        DateTimeOffset.UtcNow - Timestamp > (maxAge ?? TimeSpan.FromMinutes(5));

    /// <summary>
    /// Gets the level clamped to 0-100 range, or null if unknown.
    /// </summary>
    public int? ClampedLevel => LevelPercent.HasValue ? Math.Clamp(LevelPercent.Value, 0, 100) : null;
}

/// <summary>
/// Represents battery information for a device that may have multiple batteries.
/// </summary>
public sealed class DeviceBatteryInfo
{
    /// <summary>
    /// Gets or sets the primary battery reading.
    /// </summary>
    public BatteryReading PrimaryReading { get; set; } = BatteryReading.Unknown(BatterySource.Unknown);

    /// <summary>
    /// Gets additional battery readings (for TWS earbuds: left, right, case).
    /// </summary>
    public IReadOnlyList<BatteryReading> AdditionalReadings { get; set; } = [];

    /// <summary>
    /// Gets the aggregated battery level for display purposes.
    /// For single-battery devices, returns the primary reading.
    /// For multi-battery devices, returns an appropriate aggregate (e.g., minimum for TWS).
    /// </summary>
    public int? AggregatedLevel => CalculateAggregatedLevel();

    /// <summary>
    /// Gets the overall charging state.
    /// </summary>
    public BatteryChargeState OverallChargeState => CalculateOverallChargeState();

    /// <summary>
    /// Gets the timestamp of the most recent reading.
    /// </summary>
    public DateTimeOffset LastUpdated => GetLatestTimestamp();

    /// <summary>
    /// Gets whether any reading has a valid level.
    /// </summary>
    public bool HasAnyLevel => PrimaryReading.HasLevel || AdditionalReadings.Any(r => r.HasLevel);

    /// <summary>
    /// Gets whether all readings are stale.
    /// </summary>
    public bool AllStale => !PrimaryReading.HasLevel || PrimaryReading.IsStale() &&
                                AdditionalReadings.All(r => !r.HasLevel || r.IsStale());

    private int? CalculateAggregatedLevel()
    {
        var validReadings = new List<int?>();

        if (PrimaryReading.HasLevel)
            validReadings.Add(PrimaryReading.ClampedLevel);

        validReadings.AddRange(AdditionalReadings.Where(r => r.HasLevel).Select(r => r.ClampedLevel));

        if (validReadings.Count == 0)
            return null;

        // For TWS earbuds, typically show the minimum (worst case)
        // For other multi-battery devices, could use average or minimum
        return validReadings.Min();
    }

    private BatteryChargeState CalculateOverallChargeState()
    {
        var states = new List<BatteryChargeState> { PrimaryReading.ChargeState };
        states.AddRange(AdditionalReadings.Select(r => r.ChargeState));

        // If any is charging, overall is charging
        if (states.Any(s => s == BatteryChargeState.Charging))
            return BatteryChargeState.Charging;

        // If all are full, overall is full
        if (states.All(s => s == BatteryChargeState.Full))
            return BatteryChargeState.Full;

        // If any is discharging, overall is discharging
        if (states.Any(s => s == BatteryChargeState.Discharging))
            return BatteryChargeState.Discharging;

        return BatteryChargeState.Unknown;
    }

    private DateTimeOffset GetLatestTimestamp()
    {
        var timestamps = new List<DateTimeOffset> { PrimaryReading.Timestamp };
        timestamps.AddRange(AdditionalReadings.Select(r => r.Timestamp));
        return timestamps.Max();
    }
}