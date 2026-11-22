namespace VoltDock.Core.Notifications;

using VoltDock.Core.Devices;

/// <summary>
/// Types of battery notifications.
/// </summary>
public enum BatteryNotificationType
{
    Warning = 0,
    Critical = 1,
    Charging = 2,
    Full = 3,
    Connected = 4,
    Disconnected = 5,
}

/// <summary>
/// Battery notification data.
/// </summary>
public sealed class BatteryNotification
{
    public string DeviceId { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
    public DeviceType DeviceType { get; set; }
    public BatteryNotificationType Type { get; set; }
    public int? BatteryLevel { get; set; }
    public BatteryChargeState ChargeState { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    public Dictionary<string, object> Metadata { get; } = [];
}

/// <summary>
/// Service for showing system notifications.
/// </summary>
public interface INotificationService : IAsyncDisposable
{
    /// <summary>
    /// Shows a battery notification.
    /// </summary>
    /// <param name="notification">The notification to show.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if notification was shown.</returns>
    Task<bool> ShowNotificationAsync(BatteryNotification notification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if notifications are currently suppressed (Focus Assist, etc.).
    /// </summary>
    bool AreNotificationsSuppressed { get; }

    /// <summary>
    /// Gets the history of shown notifications.
    /// </summary>
    IReadOnlyList<BatteryNotification> GetHistory(int maxCount = 100);

    /// <summary>
    /// Clears notification history.
    /// </summary>
    void ClearHistory();
}