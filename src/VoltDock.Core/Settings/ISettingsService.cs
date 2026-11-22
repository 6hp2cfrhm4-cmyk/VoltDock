namespace VoltDock.Core.Settings;

/// <summary>
/// Service for managing application settings.
/// </summary>
public interface ISettingsService
{
    /// <summary>
    /// Current settings.
    /// </summary>
    AppSettings Settings { get; }

    /// <summary>
    /// Event raised when settings change.
    /// </summary>
    event EventHandler<SettingsChangedEventArgs>? SettingsChanged;

    /// <summary>
    /// Loads settings from storage.
    /// </summary>
    Task LoadAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves settings to storage.
    /// </summary>
    Task SaveAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Resets settings to defaults.
    /// </summary>
    Task ResetAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a specific setting.
    /// </summary>
    Task UpdateSettingAsync<T>(string key, T value, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a setting value.
    /// </summary>
    T? GetSetting<T>(string key);

    /// <summary>
    /// Exports settings to a file.
    /// </summary>
    Task ExportAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Imports settings from a file.
    /// </summary>
    Task ImportAsync(string filePath, CancellationToken cancellationToken = default);
}

/// <summary>
/// Event arguments for settings changes.
/// </summary>
public sealed class SettingsChangedEventArgs : EventArgs
{
    public string Key { get; }
    public object? OldValue { get; }
    public object? NewValue { get; }

    public SettingsChangedEventArgs(string key, object? oldValue, object? newValue)
    {
        Key = key;
        OldValue = oldValue;
        NewValue = newValue;
    }
}