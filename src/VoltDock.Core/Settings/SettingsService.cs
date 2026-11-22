namespace VoltDock.Core.Settings;

using Microsoft.Extensions.Logging;
using System.IO;
using System.Text.Json;

/// <summary>
/// Settings service implementation using JSON file storage.
/// </summary>
public sealed class SettingsService : ISettingsService
{
    private readonly ILogger<SettingsService> _logger;
    private readonly string _settingsPath;
    private AppSettings _settings = new();
    private readonly SemaphoreSlim _saveLock = new(1, 1);
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    public AppSettings Settings => _settings;

    public event EventHandler<SettingsChangedEventArgs>? SettingsChanged;

    public SettingsService(ILogger<SettingsService> logger)
    {
        _logger = logger;

        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var appFolder = Path.Combine(appDataPath, "VoltDock");
        Directory.CreateDirectory(appFolder);
        _settingsPath = Path.Combine(appFolder, "settings.json");
    }

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (File.Exists(_settingsPath))
            {
                var json = await File.ReadAllTextAsync(_settingsPath, cancellationToken);
                var loaded = JsonSerializer.Deserialize<AppSettings>(json, _jsonOptions);
                if (loaded != null)
                {
                    _settings = MigrateSettings(loaded);
                    _logger.LogInformation("Settings loaded from {Path}", _settingsPath);
                }
            }
            else
            {
                _settings = new AppSettings();
                _logger.LogInformation("No settings file found, using defaults");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load settings, using defaults");
            _settings = new AppSettings();
        }

        // Ensure version is current
        _settings.Version = 1;
    }

    public async Task SaveAsync(CancellationToken cancellationToken = default)
    {
        await _saveLock.WaitAsync(cancellationToken);
        try
        {
            var json = JsonSerializer.Serialize(_settings, _jsonOptions);
            var tempPath = _settingsPath + ".tmp";
            await File.WriteAllTextAsync(tempPath, json, cancellationToken);
            File.Move(tempPath, _settingsPath, true);
            _logger.LogDebug("Settings saved to {Path}", _settingsPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save settings");
        }
        finally
        {
            _saveLock.Release();
        }
    }

    public async Task ResetAsync(CancellationToken cancellationToken = default)
    {
        _settings = new AppSettings();
        await SaveAsync(cancellationToken);
        _logger.LogInformation("Settings reset to defaults");
    }

    public async Task UpdateSettingAsync<T>(string key, T value, CancellationToken cancellationToken = default)
    {
        var oldValue = GetSetting<T>(key);
        SetSetting(key, value);
        await SaveAsync(cancellationToken);
        SettingsChanged?.Invoke(this, new SettingsChangedEventArgs(key, oldValue, value));
    }

    public T? GetSetting<T>(string key)
    {
        // Simple key-based access for common settings
        return key switch
        {
            "Dock.Position" => (T)(object)_settings.Dock.Position,
            "Dock.DisplayMode" => (T)(object)_settings.Dock.DisplayMode,
            "Dock.AutoHide" => (T)(object)_settings.Dock.AutoHide,
            "Dock.IconSize" => (T)(object)_settings.Dock.IconSize,
            "Dock.Opacity" => (T)(object)_settings.Dock.Opacity,
            "Dock.EnableBlur" => (T)(object)_settings.Dock.EnableBlur,
            "Dock.EnableAnimations" => (T)(object)_settings.Dock.EnableAnimations,
            "Appearance.Theme" => (T)(object)_settings.Appearance.Theme,
            "Appearance.ScaleFactor" => (T)(object)_settings.Appearance.ScaleFactor,
            "Notifications.Enabled" => (T)(object)_settings.Notifications.Enabled,
            "Notifications.GlobalWarningPercent" => (T)(object)_settings.Notifications.GlobalWarningPercent,
            "Notifications.GlobalCriticalPercent" => (T)(object)_settings.Notifications.GlobalCriticalPercent,
            "Behavior.StartWithWindows" => (T)(object)_settings.Behavior.StartWithWindows,
            "Behavior.StartMinimized" => (T)(object)_settings.Behavior.StartMinimized,
            "Behavior.PollingInterval" => (T)(object)_settings.Behavior.PollingInterval,
            "Advanced.EnableDiagnostics" => (T)(object)_settings.Advanced.EnableDiagnostics,
            "Advanced.DemoMode" => (T)(object)_settings.Advanced.DemoMode,
            _ => default
        };
    }

    private void SetSetting<T>(string key, T value)
    {
        switch (key)
        {
            case "Dock.Position": _settings.Dock.Position = (DockPosition)(object)value!; break;
            case "Dock.DisplayMode": _settings.Dock.DisplayMode = (DockDisplayMode)(object)value!; break;
            case "Dock.AutoHide": _settings.Dock.AutoHide = (AutoHideMode)(object)value!; break;
            case "Dock.IconSize": _settings.Dock.IconSize = (int)(object)value!; break;
            case "Dock.Opacity": _settings.Dock.Opacity = (double)(object)value!; break;
            case "Dock.EnableBlur": _settings.Dock.EnableBlur = (bool)(object)value!; break;
            case "Dock.EnableAnimations": _settings.Dock.EnableAnimations = (bool)(object)value!; break;
            case "Appearance.Theme": _settings.Appearance.Theme = (AppTheme)(object)value!; break;
            case "Appearance.ScaleFactor": _settings.Appearance.ScaleFactor = (double)(object)value!; break;
            case "Notifications.Enabled": _settings.Notifications.Enabled = (bool)(object)value!; break;
            case "Notifications.GlobalWarningPercent": _settings.Notifications.GlobalWarningPercent = (int)(object)value!; break;
            case "Notifications.GlobalCriticalPercent": _settings.Notifications.GlobalCriticalPercent = (int)(object)value!; break;
            case "Behavior.StartWithWindows": _settings.Behavior.StartWithWindows = (bool)(object)value!; break;
            case "Behavior.StartMinimized": _settings.Behavior.StartMinimized = (bool)(object)value!; break;
            case "Behavior.PollingInterval": _settings.Behavior.PollingInterval = (int)(object)value!; break;
            case "Advanced.EnableDiagnostics": _settings.Advanced.EnableDiagnostics = (bool)(object)value!; break;
            case "Advanced.DemoMode": _settings.Advanced.DemoMode = (bool)(object)value!; break;
        }
    }

    public async Task ExportAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(_settings, _jsonOptions);
        await File.WriteAllTextAsync(filePath, json, cancellationToken);
    }

    public async Task ImportAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var json = await File.ReadAllTextAsync(filePath, cancellationToken);
        var imported = JsonSerializer.Deserialize<AppSettings>(json, _jsonOptions);
        if (imported != null)
        {
            _settings = MigrateSettings(imported);
            await SaveAsync(cancellationToken);
        }
    }

    private AppSettings MigrateSettings(AppSettings settings)
    {
        // Handle version migrations
        if (settings.Version < 1)
        {
            // Migration from pre-versioned settings
            settings.Version = 1;
        }
        return settings;
    }
}