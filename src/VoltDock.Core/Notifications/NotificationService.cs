namespace VoltDock.Core.Notifications;

using VoltDock.Core.Devices;
using Microsoft.Extensions.Logging;
using Windows.UI.Notifications;
using Windows.Data.Xml.Dom;

/// <summary>
/// Notification service using Windows Toast Notifications.
/// </summary>
public sealed class NotificationService : INotificationService
{
    private readonly ILogger<NotificationService> _logger;
    private readonly ToastNotifier _toastNotifier;
    private readonly List<BatteryNotification> _history = new();
    private readonly object _historyLock = new();
    private const int MaxHistorySize = 100;
    private bool _focusAssistSupported = true;

    public bool AreNotificationsSuppressed
    {
        get
        {
            try
            {
                // Check Focus Assist / Quiet Hours status
                return UserNotificationListener.Current.GetAccessStatus() != UserNotificationListenerAccessStatus.Allowed;
            }
            catch
            {
                // API not available or error - assume not suppressed
                return false;
            }
        }
    }

    public event EventHandler<bool>? SuppressionChanged;

    public NotificationService(ILogger<NotificationService> logger)
    {
        _logger = logger;
        _toastNotifier = ToastNotificationManager.CreateToastNotifier("VoltDock");

        // Monitor focus assist changes
        _ = MonitorSuppressionAsync();
    }

    public async Task<bool> ShowNotificationAsync(BatteryNotification notification, CancellationToken cancellationToken = default)
    {
        if (AreNotificationsSuppressed)
        {
            _logger.LogDebug("Notifications suppressed, skipping: {Message}", notification.Message);
            return false;
        }

        try
        {
            var toastXml = CreateToastXml(notification);
            var toast = new ToastNotification(toastXml)
            {
                Tag = notification.DeviceId,
                Group = "VoltDock.Battery",
                ExpirationTime = DateTimeOffset.Now.AddMinutes(5),
                SuppressPopup = false
            };

            _toastNotifier.Show(toast);

            lock (_historyLock)
            {
                _history.Insert(0, notification);
                if (_history.Count > MaxHistorySize)
                    _history.RemoveAt(_history.Count - 1);
            }

            _logger.LogInformation("Showed notification: {Message}", notification.Message);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to show notification");
            return false;
        }
    }

    private XmlDocument CreateToastXml(BatteryNotification notification)
    {
        var template = $@"
<toast>
    <visual>
        <binding template='ToastGeneric'>
            <text>{GetNotificationTitle(notification)}</text>
            <text>{notification.Message}</text>
        </binding>
    </visual>
    <audio src='ms-winsoundevent:Notification.Default' loop='false'/>
</toast>";

        var xml = new XmlDocument();
        xml.LoadXml(template);
        return xml;
    }

    private string GetNotificationTitle(BatteryNotification notification)
    {
        return notification.Type switch
        {
            BatteryNotificationType.Warning => $"⚠ Низкий заряд: {notification.DeviceName}",
            BatteryNotificationType.Critical => $"🔴 Критический заряд: {notification.DeviceName}",
            BatteryNotificationType.Charging => $"⚡ Зарядка: {notification.DeviceName}",
            BatteryNotificationType.Full => $"✅ Заряжено: {notification.DeviceName}",
            BatteryNotificationType.Connected => $"🔗 Подключено: {notification.DeviceName}",
            BatteryNotificationType.Disconnected => $"🔌 Отключено: {notification.DeviceName}",
            _ => $"VoltDock: {notification.DeviceName}"
        };
    }

    public IReadOnlyList<BatteryNotification> GetHistory(int maxCount = 100)
    {
        lock (_historyLock)
        {
            return _history.Take(maxCount).ToList().AsReadOnly();
        }
    }

    public void ClearHistory()
    {
        lock (_historyLock)
        {
            _history.Clear();
        }
    }

    private async Task MonitorSuppressionAsync()
    {
        var lastState = AreNotificationsSuppressed;
        while (true)
        {
            await Task.Delay(TimeSpan.FromSeconds(30));
            try
            {
                var currentState = AreNotificationsSuppressed;
                if (currentState != lastState)
                {
                    lastState = currentState;
                    SuppressionChanged?.Invoke(this, currentState);
                }
            }
            catch
            {
                // Ignore errors in monitoring
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        // Nothing to dispose for toast notifier
    }
}