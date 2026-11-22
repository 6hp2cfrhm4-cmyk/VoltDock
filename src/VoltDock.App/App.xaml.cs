using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VoltDock.Core;
using VoltDock.Core.Devices;
using VoltDock.Core.Devices.Providers;
using VoltDock.Core.Battery;
using VoltDock.Core.Settings;
using VoltDock.Core.Notifications;
using VoltDock.App.ViewModels;
using VoltDock.App.Views;

namespace VoltDock.App;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private IHost? _host;
    private VoltDockService? _voltDockService;
    private MainWindow? _mainWindow;
    private DockWindow? _dockWindow;
    private readonly Mutex _singleInstanceMutex;

    public App()
    {
        // Ensure single instance
        _singleInstanceMutex = new Mutex(true, "Global\\VoltDock_SingleInstance", out var createdNew);
        if (!createdNew)
        {
            // Another instance is running
            Current.Shutdown();
            return;
        }

        InitializeHost();
    }

    private void InitializeHost()
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                // Core services
                services.AddSingleton<ISettingsService, SettingsService>();
                services.AddSingleton<IDeviceIdentityResolver, DeviceIdentityResolver>();
                services.AddSingleton<IBatteryReadingAggregator, BatteryReadingAggregator>();
                services.AddSingleton<INotificationService, NotificationService>();
                services.AddSingleton<IDeviceMetadataProvider, DeviceMetadataProvider>();
                services.AddSingleton<IDeviceIconResolver, DeviceIconResolver>();

                // Discovery providers
                services.AddSingleton<IDeviceDiscoveryProvider, BluetoothLeDiscoveryProvider>();
                services.AddSingleton<IDeviceDiscoveryProvider, HidDiscoveryProvider>();
                services.AddSingleton<IDeviceDiscoveryProvider, XInputDiscoveryProvider>();
                services.AddSingleton<IDeviceDiscoveryProvider, WindowsPowerDiscoveryProvider>();

                // Battery providers
                services.AddSingleton<IBatteryProvider, BluetoothGattBatteryProvider>();
                services.AddSingleton<IBatteryProvider, HidBatteryProvider>();
                services.AddSingleton<IBatteryProvider, XInputBatteryProvider>();
                services.AddSingleton<IBatteryProvider, WindowsPowerBatteryProvider>();
                services.AddSingleton<IBatteryProvider, DevicePropertiesBatteryProvider>();

                // Manufacturer-specific providers
                services.AddSingleton<IBatteryProvider, MchoseV9ProBatteryProvider>();
                services.AddSingleton<IBatteryProvider, AttackSharkR2BatteryProvider>();

                // Main service
                services.AddSingleton<VoltDockService>();

                // ViewModels
                services.AddTransient<MainViewModel>();
                services.AddTransient<DockViewModel>();
                services.AddTransient<SettingsViewModel>();
                services.AddTransient<DeviceViewModel>();
                services.AddTransient<DashboardViewModel>();
                services.AddTransient<NotificationsViewModel>();
                services.AddTransient<AppearanceViewModel>();
                services.AddTransient<AdvancedViewModel>();
                services.AddTransient<AboutViewModel>();

                // Views
                services.AddTransient<MainWindow>();
                services.AddTransient<DockWindow>();
                services.AddTransient<SettingsWindow>();
            })
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddDebug();
                logging.AddConsole();
            })
            .Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        if (_host == null)
        {
            Shutdown();
            return;
        }

        await _host.StartAsync();

        _voltDockService = _host.Services.GetRequiredService<VoltDockService>();
        await _voltDockService.StartAsync();

        var settingsService = _host.Services.GetRequiredService<ISettingsService>();
        await settingsService.LoadAsync();

        // Create dock window (always running)
        _dockWindow = _host.Services.GetRequiredService<DockWindow>();
        _dockWindow.Hide(); // Start hidden, show based on settings

        // Create main window (hidden by default)
        _mainWindow = _host.Services.GetRequiredService<MainWindow>();
        if (!settingsService.Settings.Behavior.StartMinimized)
        {
            _mainWindow.Show();
        }

        // Setup system tray
        SetupTrayIcon();
    }

    private void SetupTrayIcon()
    {
        // System tray icon will be implemented in MainWindow
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (_voltDockService != null)
        {
            await _voltDockService.StopAsync();
        }

        if (_host != null)
        {
            await _host.StopAsync();
            _host.Dispose();
        }

        _singleInstanceMutex?.ReleaseMutex();
        _singleInstanceMutex?.Dispose();

        base.OnExit(e);
    }

    public static new App Current => (App)Application.Current;

    public T GetService<T>() where T : notnull => _host!.Services.GetRequiredService<T>();

    public void ShowMainWindow()
    {
        _mainWindow?.Show();
        _mainWindow?.Activate();
        _mainWindow?.WindowState = WindowState.Normal;
    }

    public void ShowDockWindow()
    {
        _dockWindow?.Show();
    }

    public void HideDockWindow()
    {
        _dockWindow?.Hide();
    }
}