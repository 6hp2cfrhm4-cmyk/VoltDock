using System.Windows.Input;
using Microsoft.Extensions.Logging;
using VoltDock.Core;
using VoltDock.Core.Settings;
using VoltDock.App.Views;

namespace VoltDock.App.ViewModels;

/// <summary>
/// Main window view model.
/// </summary>
public sealed class MainViewModel : ViewModelBase
{
    private readonly VoltDockService _voltDockService;
    private readonly ISettingsService _settingsService;
    private ViewModelBase? _currentView;
    private string _currentPage = "Dashboard";

    public MainViewModel(
        VoltDockService voltDockService,
        ISettingsService settingsService,
        ILogger<MainViewModel> logger,
        DashboardViewModel dashboardViewModel,
        DeviceViewModel deviceViewModel,
        DockViewModel dockViewModel,
        SettingsViewModel settingsViewModel,
        NotificationsViewModel notificationsViewModel,
        AppearanceViewModel appearanceViewModel,
        AdvancedViewModel advancedViewModel,
        AboutViewModel aboutViewModel) : base(logger)
    {
        _voltDockService = voltDockService;
        _settingsService = settingsService;

        // Initialize sub-viewmodels
        DashboardVM = dashboardViewModel;
        DevicesVM = deviceViewModel;
        DockVM = dockViewModel;
        SettingsVM = settingsViewModel;
        NotificationsVM = notificationsViewModel;
        AppearanceVM = appearanceViewModel;
        AdvancedVM = advancedViewModel;
        AboutVM = aboutViewModel;

        // Commands
        NavigateCommand = new RelayCommand<string>(Navigate);
        ShowWindowCommand = new RelayCommand(ShowWindow);
        ToggleDockCommand = new RelayCommand(ToggleDock);
        RefreshDevicesCommand = new RelayCommand(async () => await RefreshDevicesAsync());
        OpenSettingsCommand = new RelayCommand(() => Navigate("Settings"));
        ExitCommand = new RelayCommand(ExitApplication);

        // Default view
        CurrentView = DashboardVM;
        CurrentPage = "Dashboard";

        VersionText = $"VoltDock v{GetVersion()}";
    }

    public DashboardViewModel DashboardVM { get; }
    public DeviceViewModel DevicesVM { get; }
    public DockViewModel DockVM { get; }
    public SettingsViewModel SettingsVM { get; }
    public NotificationsViewModel NotificationsVM { get; }
    public AppearanceViewModel AppearanceVM { get; }
    public AdvancedViewModel AdvancedVM { get; }
    public AboutViewModel AboutVM { get; }

    public ViewModelBase? CurrentView
    {
        get => _currentView;
        set => SetProperty(ref _currentView, value);
    }

    public string CurrentPage
    {
        get => _currentPage;
        set
        {
            if (SetProperty(ref _currentPage, value))
            {
                UpdateSelectionStates();
            }
        }
    }

    public string VersionText { get; }

    public AppSettings Settings => _settingsService.Settings;

    public bool IsDashboardSelected => CurrentPage == "Dashboard";
    public bool IsDevicesSelected => CurrentPage == "Devices";
    public bool IsDockSelected => CurrentPage == "Dock";
    public bool IsNotificationsSelected => CurrentPage == "Notifications";
    public bool IsAppearanceSelected => CurrentPage == "Appearance";
    public bool IsAdvancedSelected => CurrentPage == "Advanced";
    public bool IsAboutSelected => CurrentPage == "About";

    public ICommand NavigateCommand { get; }
    public ICommand ShowWindowCommand { get; }
    public ICommand ToggleDockCommand { get; }
    public ICommand RefreshDevicesCommand { get; }
    public ICommand OpenSettingsCommand { get; }
    public ICommand ExitCommand { get; }

    private void Navigate(string? page)
    {
        CurrentPage = page ?? "Dashboard";
        CurrentView = page switch
        {
            "Dashboard" => DashboardVM,
            "Devices" => DevicesVM,
            "Dock" => DockVM,
            "Notifications" => NotificationsVM,
            "Appearance" => AppearanceVM,
            "Advanced" => AdvancedVM,
            "About" => AboutVM,
            _ => DashboardVM
        };
    }

    private void UpdateSelectionStates()
    {
        OnPropertyChanged(nameof(IsDashboardSelected));
        OnPropertyChanged(nameof(IsDevicesSelected));
        OnPropertyChanged(nameof(IsDockSelected));
        OnPropertyChanged(nameof(IsNotificationsSelected));
        OnPropertyChanged(nameof(IsAppearanceSelected));
        OnPropertyChanged(nameof(IsAdvancedSelected));
        OnPropertyChanged(nameof(IsAboutSelected));
    }

    private void ShowWindow()
    {
        if (Application.Current.MainWindow is Window window)
        {
            window.Show();
            window.WindowState = WindowState.Normal;
            window.Activate();
        }
    }

    private void ToggleDock()
    {
        DockVM.ToggleVisibility();
    }

    private async Task RefreshDevicesAsync()
    {
        await _voltDockService.RefreshAsync();
        LogInformation("Devices refreshed manually");
    }

    private void ExitApplication()
    {
        Application.Current.Shutdown();
    }

    private string GetVersion()
    {
        var assembly = System.Reflection.Assembly.GetExecutingAssembly();
        var version = assembly.GetName().Version;
        return version?.ToString(3) ?? "1.0.0";
    }
}

/// <summary>
/// Simple relay command implementation.
/// </summary>
public class RelayCommand : ICommand
{
    private readonly Action _execute;
    private readonly Func<bool>? _canExecute;

    public RelayCommand(Action execute, Func<bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;

    public void Execute(object? parameter) => _execute();
}

/// <summary>
/// Generic relay command implementation.
/// </summary>
public class RelayCommand<T> : ICommand
{
    private readonly Action<T?> _execute;
    private readonly Func<T?, bool>? _canExecute;

    public RelayCommand(Action<T?> execute, Func<T?, bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public bool CanExecute(object? parameter) => _canExecute?.Invoke((T?)parameter) ?? true;

    public void Execute(object? parameter) => _execute((T?)parameter);
}