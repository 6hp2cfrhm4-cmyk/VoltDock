using System.Windows.Input;
using Microsoft.Extensions.Logging;
using VoltDock.Core.Settings;

namespace VoltDock.App.ViewModels;

/// <summary>
/// View model for the settings window/page.
/// </summary>
public sealed class SettingsViewModel : ViewModelBase
{
    private readonly ISettingsService _settingsService;

    public SettingsViewModel(
        ISettingsService settingsService,
        ILogger<SettingsViewModel> logger) : base(logger)
    {
        _settingsService = settingsService;

        SaveCommand = new RelayCommand(async () => await SaveAsync());
        ResetCommand = new RelayCommand(async () => await ResetAsync());
        ExportCommand = new RelayCommand(ExportSettings);
        ImportCommand = new RelayCommand(ImportSettings);
    }

    public AppSettings Settings => _settingsService.Settings;

    public ICommand SaveCommand { get; }
    public ICommand ResetCommand { get; }
    public ICommand ExportCommand { get; }
    public ICommand ImportCommand { get; }

    public async Task SaveAsync()
    {
        await _settingsService.SaveAsync();
        LogInformation("Settings saved");
    }

    public async Task ResetAsync()
    {
        await _settingsService.ResetAsync();
        OnPropertyChanged(nameof(Settings));
        LogInformation("Settings reset to defaults");
    }

    private void ExportSettings()
    {
        // TODO: Show file dialog and export
    }

    private void ImportSettings()
    {
        // TODO: Show file dialog and import
    }
}

/// <summary>
/// Dashboard view model.
/// </summary>
public sealed class DashboardViewModel : ViewModelBase
{
    private readonly VoltDockService _voltDockService;

    public DashboardViewModel(
        VoltDockService voltDockService,
        ILogger<DashboardViewModel> logger) : base(logger)
    {
        _voltDockService = voltDockService;
        RefreshCommand = new RelayCommand(async () => await RefreshAsync());
    }

    public ICommand RefreshCommand { get; }

    public IReadOnlyList<Device> Devices => _voltDockService.GetDevices();

    public int ConnectedCount => Devices.Count(d => d.ConnectionState == DeviceConnectionState.Connected);
    public int TotalCount => Devices.Count;
    public int LowBatteryCount => Devices.Count(d => d.BatteryInfo.AggregatedLevel.HasValue && d.BatteryInfo.AggregatedLevel.Value <= 20);

    public async Task RefreshAsync()
    {
        await _voltDockService.RefreshAsync();
        OnPropertyChanged(nameof(Devices));
        OnPropertyChanged(nameof(ConnectedCount));
        OnPropertyChanged(nameof(TotalCount));
        OnPropertyChanged(nameof(LowBatteryCount));
    }
}

/// <summary>
/// Devices view model.
/// </summary>
public sealed class DeviceViewModel : ViewModelBase
{
    private readonly VoltDockService _voltDockService;
    private Device? _selectedDevice;

    public DeviceViewModel(
        VoltDockService voltDockService,
        ILogger<DeviceViewModel> logger) : base(logger)
    {
        _voltDockService = voltDockService;
        RefreshCommand = new RelayCommand(async () => await RefreshAsync());
    }

    public ICommand RefreshCommand { get; }

    public IReadOnlyList<Device> Devices => _voltDockService.GetDevices();

    public Device? SelectedDevice
    {
        get => _selectedDevice;
        set => SetProperty(ref _selectedDevice, value);
    }

    public async Task RefreshAsync()
    {
        await _voltDockService.RefreshAsync();
        OnPropertyChanged(nameof(Devices));
    }
}

/// <summary>
/// Notifications view model.
/// </summary>
public sealed class NotificationsViewModel : ViewModelBase
{
    private readonly ISettingsService _settingsService;

    public NotificationsViewModel(
        ISettingsService settingsService,
        ILogger<NotificationsViewModel> logger) : base(logger)
    {
        _settingsService = settingsService;
        AddThresholdCommand = new RelayCommand(AddThreshold);
        RemoveThresholdCommand = new RelayCommand<BatteryThreshold>(RemoveThreshold);
    }

    public NotificationSettings Notifications => _settingsService.Settings.Notifications;

    public ICommand AddThresholdCommand { get; }
    public ICommand RemoveThresholdCommand { get; }

    private void AddThreshold()
    {
        var threshold = new BatteryThreshold
        {
            WarningPercent = 20,
            CriticalPercent = 10,
            Enabled = true
        };
        Notifications.Thresholds.Add(threshold);
        OnPropertyChanged(nameof(Notifications));
    }

    private void RemoveThreshold(BatteryThreshold? threshold)
    {
        if (threshold != null)
        {
            Notifications.Thresholds.Remove(threshold);
            OnPropertyChanged(nameof(Notifications));
        }
    }
}

/// <summary>
/// Appearance view model.
/// </summary>
public sealed class AppearanceViewModel : ViewModelBase
{
    private readonly ISettingsService _settingsService;

    public AppearanceViewModel(
        ISettingsService settingsService,
        ILogger<AppearanceViewModel> logger) : base(logger)
    {
        _settingsService = settingsService;
    }

    public AppearanceSettings Appearance => _settingsService.Settings.Appearance;
    public DockSettings Dock => _settingsService.Settings.Dock;

    public AppTheme[] Themes => Enum.GetValues<AppTheme>();
    public DockPosition[] Positions => Enum.GetValues<DockPosition>();
    public DockDisplayMode[] DisplayModes => Enum.GetValues<DockDisplayMode>();
    public AutoHideMode[] AutoHideModes => Enum.GetValues<AutoHideMode>();
}

/// <summary>
/// Advanced view model.
/// </summary>
public sealed class AdvancedViewModel : ViewModelBase
{
    private readonly ISettingsService _settingsService;
    private readonly VoltDockService _voltDockService;

    public AdvancedViewModel(
        ISettingsService settingsService,
        VoltDockService voltDockService,
        ILogger<AdvancedViewModel> logger) : base(logger)
    {
        _settingsService = settingsService;
        _voltDockService = voltDockService;

        ExportDiagnosticsCommand = new RelayCommand(ExportDiagnostics);
        RefreshDevicesCommand = new RelayCommand(async () => await RefreshDevicesAsync());
    }

    public AdvancedSettings Advanced => _settingsService.Settings.Advanced;
    public BehaviorSettings Behavior => _settingsService.Settings.Behavior;

    public ICommand ExportDiagnosticsCommand { get; }
    public ICommand RefreshDevicesCommand { get; }

    private void ExportDiagnostics()
    {
        // TODO: Implement diagnostic export
    }

    private async Task RefreshDevicesAsync()
    {
        await _voltDockService.RefreshAsync();
        LogInformation("Devices refreshed from Advanced page");
    }
}

/// <summary>
/// About view model.
/// </summary>
public sealed class AboutViewModel : ViewModelBase
{
    public AboutViewModel(ILogger<AboutViewModel> logger) : base(logger)
    {
        OpenGitHubCommand = new RelayCommand(OpenGitHub);
        OpenLicenseCommand = new RelayCommand(OpenLicense);
    }

    public string Version => GetVersion();
    public string Description => "VoltDock — красивый док для отображения заряда подключённых устройств.";
    public string Author => "VoltDock Contributors";
    public string License => "MIT License";
    public string RepositoryUrl => "https://github.com/yourusername/VoltDock";

    public ICommand OpenGitHubCommand { get; }
    public ICommand OpenLicenseCommand { get; }

    private void OpenGitHub()
    {
        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = RepositoryUrl,
                UseShellExecute = true
            });
        }
        catch { }
    }

    private void OpenLicense()
    {
        // TODO: Show license dialog
    }

    private string GetVersion()
    {
        var assembly = System.Reflection.Assembly.GetExecutingAssembly();
        var version = assembly.GetName().Version;
        return version?.ToString(3) ?? "1.0.0";
    }
}