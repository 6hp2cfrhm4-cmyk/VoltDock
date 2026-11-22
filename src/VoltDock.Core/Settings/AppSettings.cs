namespace VoltDock.Core.Settings;

using System.Text.Json.Serialization;
using VoltDock.Core.Devices;
using Microsoft.Extensions.Logging;

/// <summary>
/// Dock position on screen.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DockPosition
{
    Bottom = 0,
    Top = 1,
    Left = 2,
    Right = 3,
}

/// <summary>
/// Dock display mode.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DockDisplayMode
{
    /// <summary>
    /// Icons only.
    /// </summary>
    IconsOnly = 0,

    /// <summary>
    /// Icons with percentage.
    /// </summary>
    IconsWithPercent = 1,

    /// <summary>
    /// Expanded cards with details.
    /// </summary>
    ExpandedCards = 2,
}

/// <summary>
/// Auto-hide behavior.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AutoHideMode
{
    /// <summary>
    /// Never auto-hide.
    /// </summary>
    Never = 0,

    /// <summary>
    /// Hide when mouse leaves dock area.
    /// </summary>
    OnMouseLeave = 1,

    /// <summary>
    /// Hide after timeout.
    /// </summary>
    Timeout = 2,

    /// <summary>
    /// Show only on hover of screen edge.
    /// </summary>
    EdgeHover = 3,
}

/// <summary>
/// Application theme.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AppTheme
{
    /// <summary>
    /// Dark glass with blur.
    /// </summary>
    DarkGlass = 0,

    /// <summary>
    /// Light glass with blur.
    /// </summary>
    LightGlass = 1,

    /// <summary>
    /// Midnight dark theme.
    /// </summary>
    Midnight = 2,

    /// <summary>
    /// Minimal flat theme.
    /// </summary>
    Minimal = 3,

    /// <summary>
    /// Follow system theme.
    /// </summary>
    SystemAuto = 4,
}

/// <summary>
/// Battery level threshold for notifications.
/// </summary>
public sealed class BatteryThreshold
{
    /// <summary>
    /// Device ID this threshold applies to (null = global).
    /// </summary>
    public string? DeviceId { get; set; }

    /// <summary>
    /// Device type this threshold applies to (null = all types).
    /// </summary>
    public DeviceType? DeviceType { get; set; }

    /// <summary>
    /// Warning threshold percentage (e.g., 20).
    /// </summary>
    public int WarningPercent { get; set; } = 20;

    /// <summary>
    /// Critical threshold percentage (e.g., 10).
    /// </summary>
    public int CriticalPercent { get; set; } = 10;

    /// <summary>
    /// Whether notifications are enabled for this threshold.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Whether to notify only once per discharge cycle.
    /// </summary>
    public bool NotifyOncePerCycle { get; set; } = true;
}

/// <summary>
/// Main application settings.
/// </summary>
public sealed class AppSettings
{
    /// <summary>
    /// Settings version for migration.
    /// </summary>
    public int Version { get; set; } = 1;

    // Dock settings
    public DockSettings Dock { get; set; } = new();

    // Appearance settings
    public AppearanceSettings Appearance { get; set; } = new();

    // Notification settings
    public NotificationSettings Notifications { get; set; } = new();

    // Behavior settings
    public BehaviorSettings Behavior { get; set; } = new();

    // Advanced settings
    public AdvancedSettings Advanced { get; set; } = new();

    // Per-device settings
    public List<DeviceSettings> Devices { get; set; } = [];
}

/// <summary>
/// Dock-specific settings.
/// </summary>
public sealed class DockSettings
{
    /// <summary>
    /// Dock position on screen.
    /// </summary>
    public DockPosition Position { get; set; } = DockPosition.Bottom;

    /// <summary>
    /// Display mode.
    /// </summary>
    public DockDisplayMode DisplayMode { get; set; } = DockDisplayMode.IconsWithPercent;

    /// <summary>
    /// Auto-hide mode.
    /// </summary>
    public AutoHideMode AutoHide { get; set; } = AutoHideMode.Never;

    /// <summary>
    /// Margin from screen edge in pixels.
    /// </summary>
    public int EdgeMargin { get; set; } = 20;

    /// <summary>
    /// Offset from center/edge in pixels.
    /// </summary>
    public int Offset { get; set; } = 0;

    /// <summary>
    /// Icon size in pixels.
    /// </summary>
    public int IconSize { get; set; } = 32;

    /// <summary>
    /// Dock width (0 = auto).
    /// </summary>
    public int Width { get; set; } = 0;

    /// <summary>
    /// Dock height (0 = auto).
    /// </summary>
    public int Height { get; set; } = 0;

    /// <summary>
    /// Corner radius in pixels.
    /// </summary>
    public int CornerRadius { get; set; } = 16;

    /// <summary>
    /// Background opacity (0-1).
    /// </summary>
    public double Opacity { get; set; } = 0.85;

    /// <summary>
    /// Enable background blur.
    /// </summary>
    public bool EnableBlur { get; set; } = true;

    /// <summary>
    /// Blur intensity.
    /// </summary>
    public double BlurIntensity { get; set; } = 0.7;

    /// <summary>
    /// Enable animations.
    /// </summary>
    public bool EnableAnimations { get; set; } = true;

    /// <summary>
    /// Animation duration in milliseconds.
    /// </summary>
    public int AnimationDuration { get; set; } = 200;

    /// <summary>
    /// Show on specific monitor (null = primary).
    /// </summary>
    public string? MonitorId { get; set; }

    /// <summary>
    /// Hide dock when fullscreen app is active.
    /// </summary>
    public bool HideOnFullscreen { get; set; } = true;

    /// <summary>
    /// Auto-hide delay in milliseconds.
    /// </summary>
    public int AutoHideDelay { get; set; } = 500;

    /// <summary>
    /// Show device names on hover.
    /// </summary>
    public bool ShowNamesOnHover { get; set; } = true;

    /// <summary>
    /// Show charging animation.
    /// </summary>
    public bool ShowChargingAnimation { get; set; } = true;

    /// <summary>
    /// Sort order for devices.
    /// </summary>
    public DeviceSortOrder SortOrder { get; set; } = DeviceSortOrder.ByType;
}

/// <summary>
/// Device sort order in dock.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DeviceSortOrder
{
    ByType = 0,
    ByName = 1,
    ByBatteryLevel = 2,
    ByLastSeen = 3,
    Manual = 4,
}

/// <summary>
/// Appearance settings.
/// </summary>
public sealed class AppearanceSettings
{
    /// <summary>
    /// Current theme.
    /// </summary>
    public AppTheme Theme { get; set; } = AppTheme.SystemAuto;

    /// <summary>
    /// UI scale factor.
    /// </summary>
    public double ScaleFactor { get; set; } = 1.0;

    /// <summary>
    /// Font family.
    /// </summary>
    public string FontFamily { get; set; } = "Segoe UI Variable";

    /// <summary>
    /// Font size.
    /// </summary>
    public double FontSize { get; set; } = 12;

    /// <summary>
    /// Accent color (hex).
    /// </summary>
    public string? AccentColor { get; set; }

    /// <summary>
    /// Use system accent color.
    /// </summary>
    public bool UseSystemAccent { get; set; } = true;

    /// <summary>
    /// High contrast mode.
    /// </summary>
    public bool HighContrast { get; set; } = false;

    /// <summary>
    /// Reduce motion.
    /// </summary>
    public bool ReduceMotion { get; set; } = false;
}

/// <summary>
/// Notification settings.
/// </summary>
public sealed class NotificationSettings
{
    /// <summary>
    /// Enable notifications globally.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Respect system Focus Assist / Do Not Disturb.
    /// </summary>
    public bool RespectFocusAssist { get; set; } = true;

    /// <summary>
    /// Global warning threshold.
    /// </summary>
    public int GlobalWarningPercent { get; set; } = 20;

    /// <summary>
    /// Global critical threshold.
    /// </summary>
    public int GlobalCriticalPercent { get; set; } = 10;

    /// <summary>
    /// Per-device thresholds.
    /// </summary>
    public List<BatteryThreshold> Thresholds { get; set; } = [];

    /// <summary>
    /// Notify when device starts charging.
    /// </summary>
    public bool NotifyOnCharging { get; set; } = false;

    /// <summary>
    /// Notify when device reaches full charge.
    /// </summary>
    public bool NotifyOnFull { get; set; } = false;

    /// <summary>
    /// Minimum interval between notifications for same device (minutes).
    /// </summary>
    public int MinNotificationInterval { get; set; } = 30;

    /// <summary>
    /// Show notification in action center.
    /// </summary>
    public bool ShowInActionCenter { get; set; } = true;

    /// <summary>
    /// Play notification sound.
    /// </summary>
    public bool PlaySound { get; set; } = false;
}

/// <summary>
/// Behavior settings.
/// </summary>
public sealed class BehaviorSettings
{
    /// <summary>
    /// Start with Windows.
    /// </summary>
    public bool StartWithWindows { get; set; } = false;

    /// <summary>
    /// Start minimized to tray.
    /// </summary>
    public bool StartMinimized { get; set; } = true;

    /// <summary>
    /// Check for updates on startup.
    /// </summary>
    public bool CheckUpdates { get; set; } = true;

    /// <summary>
    /// Battery polling interval in seconds (0 = event-driven only).
    /// </summary>
    public int PollingInterval { get; set; } = 60;

    /// <summary>
    /// Maximum polling interval for idle devices (seconds).
    /// </summary>
    public int MaxPollingInterval { get; set; } = 300;

    /// <summary>
    /// Enable adaptive polling (slower when stable).
    /// </summary>
    public bool AdaptivePolling { get; set; } = true;

    /// <summary>
    /// Timeout for battery reads (milliseconds).
    /// </summary>
    public int ReadTimeout { get; set; } = 5000;

    /// <summary>
    /// Maximum concurrent battery reads.
    /// </summary>
    public int MaxConcurrentReads { get; set; } = 3;

    /// <summary>
    /// Cache battery readings for this many seconds.
    /// </summary>
    public int CacheDuration { get; set; } = 30;

    /// <summary>
    /// Consider readings stale after this many minutes.
    /// </summary>
    public int StaleThresholdMinutes { get; set; } = 5;

    /// <summary>
    /// Enable manufacturer-specific providers.
    /// </summary>
    public bool EnableManufacturerProviders { get; set; } = true;

    /// <summary>
    /// Enable HID battery reports.
    /// </summary>
    public bool EnableHidReports { get; set; } = true;

    /// <summary>
    /// Enable Bluetooth GATT battery service.
    /// </summary>
    public bool EnableBluetoothGatt { get; set; } = true;

    /// <summary>
    /// Enable XInput for controllers.
    /// </summary>
    public bool EnableXInput { get; set; } = true;

    /// <summary>
    /// Enable Windows Power API for laptop battery.
    /// </summary>
    public bool EnableWindowsPower { get; set; } = true;

    /// <summary>
    /// Show disconnected devices.
    /// </summary>
    public bool ShowDisconnected { get; set; } = false;

    /// <summary>
    /// Hide devices with unknown battery.
    /// </summary>
    public bool HideUnknownBattery { get; set; } = false;
}

/// <summary>
/// Advanced settings.
/// </summary>
public sealed class AdvancedSettings
{
    /// <summary>
    /// Enable diagnostic logging.
    /// </summary>
    public bool EnableDiagnostics { get; set; } = false;

    /// <summary>
    /// Log level.
    /// </summary>
    public LogLevel LogLevel { get; set; } = LogLevel.Information;

    /// <summary>
    /// Export diagnostic data on exit.
    /// </summary>
    public bool ExportDiagnosticsOnExit { get; set; } = false;

    /// <summary>
    /// Diagnostic data directory.
    /// </summary>
    public string? DiagnosticsPath { get; set; }

    /// <summary>
    /// Enable demo mode (fake devices).
    /// </summary>
    public bool DemoMode { get; set; } = false;

    /// <summary>
    /// Demo devices configuration.
    /// </summary>
    public List<DemoDeviceConfig> DemoDevices { get; set; } = [];

    /// <summary>
    /// Disable specific providers.
    /// </summary>
    public List<string> DisabledProviders { get; set; } = [];

    /// <summary>
    /// Force specific provider for a device.
    /// </summary>
    public Dictionary<string, string> ForcedProviders { get; set; } = [];

    /// <summary>
    /// Hardware acceleration for UI.
    /// </summary>
    public bool HardwareAcceleration { get; set; } = true;

    /// <summary>
    /// Enable DPI awareness.
    /// </summary>
    public bool PerMonitorDpiAware { get; set; } = true;
}

/// <summary>
/// Demo device configuration.
/// </summary>
public sealed class DemoDeviceConfig
{
    public string Name { get; set; } = "Demo Device";
    public DeviceType Type { get; set; } = DeviceType.Mouse;
    public int BatteryLevel { get; set; } = 75;
    public BatteryChargeState ChargeState { get; set; } = BatteryChargeState.Discharging;
    public string? Manufacturer { get; set; }
    public string? Model { get; set; }
}

/// <summary>
/// Per-device settings.
/// </summary>
public sealed class DeviceSettings
{
    /// <summary>
    /// Device ID.
    /// </summary>
    public string DeviceId { get; set; } = string.Empty;

    /// <summary>
    /// Custom display name.
    /// </summary>
    public string? CustomName { get; set; }

    /// <summary>
    /// Pin to dock.
    /// </summary>
    public bool IsPinned { get; set; }

    /// <summary>
    /// Hide when disconnected.
    /// </summary>
    public bool HideWhenDisconnected { get; set; }

    /// <summary>
    /// Custom warning threshold.
    /// </summary>
    public int? WarningPercent { get; set; }

    /// <summary>
    /// Custom critical threshold.
    /// </summary>
    public int? CriticalPercent { get; set; }

    /// <summary>
    /// Enable notifications for this device.
    /// </summary>
    public bool NotificationsEnabled { get; set; } = true;

    /// <summary>
    /// Preferred battery source.
    /// </summary>
    public BatterySource? PreferredSource { get; set; }

    /// <summary>
    /// Display order in dock (lower = first).
    /// </summary>
    public int DisplayOrder { get; set; }
}