using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using Microsoft.Extensions.Logging;
using VoltDock.Core;
using VoltDock.Core.Devices;
using VoltDock.Core.Settings;

namespace VoltDock.App.ViewModels;

/// <summary>
/// View model for the floating dock window.
/// </summary>
public sealed class DockViewModel : ViewModelBase
{
    private readonly VoltDockService _voltDockService;
    private readonly ISettingsService _settingsService;
    private ObservableCollection<DockDeviceViewModel> _devices = new();

    public DockViewModel(
        VoltDockService voltDockService,
        ISettingsService settingsService,
        ILogger<DockViewModel> logger) : base(logger)
    {
        _voltDockService = voltDockService;
        _settingsService = settingsService;

        ToggleVisibilityCommand = new RelayCommand(ToggleVisibility);
        
        // Subscribe to device changes
        _voltDockService.GetDevices().ForEach(AddDevice);
        
        // TODO: Subscribe to device events from VoltDockService
        RefreshDevices();
    }

    public ObservableCollection<DockDeviceViewModel> Devices
    {
        get => _devices;
        private set => SetProperty(ref _devices, value);
    }

    public AppSettings Settings => _settingsService.Settings;
    public DockSettings Dock => _settingsService.Settings.Dock;

    public ICommand ToggleVisibilityCommand { get; }

    public int CornerRadius => Dock.CornerRadius;
    public double DockOpacity => Dock.Opacity;
    public bool EnableAnimations => Dock.EnableAnimations;

    public void ToggleVisibility()
    {
        // This would be called from the main window to show/hide the dock
        // The actual window visibility is managed by the DockWindow
    }

    public void RefreshDevices()
    {
        var devices = _voltDockService.GetDevices()
            .Where(d => ShouldShowInDock(d))
            .OrderBy(d => GetSortOrder(d))
            .ToList();

        // Update existing or add new
        var currentIds = Devices.Select(d => d.Device.Id).ToHashSet();
        var newIds = devices.Select(d => d.Id).ToHashSet();

        // Remove devices no longer present
        for (int i = Devices.Count - 1; i >= 0; i--)
        {
            if (!newIds.Contains(Devices[i].Device.Id))
            {
                Devices.RemoveAt(i);
            }
        }

        // Add or update devices
        foreach (var device in devices)
        {
            var existing = Devices.FirstOrDefault(d => d.Device.Id == device.Id);
            if (existing != null)
            {
                existing.UpdateFromDevice(device);
            }
            else
            {
                Devices.Add(new DockDeviceViewModel(device, Dock));
            }
        }
    }

    private bool ShouldShowInDock(Device device)
    {
        if (!device.HasBatteryData && Dock.HideUnknownBattery)
            return false;

        if (device.ConnectionState != DeviceConnectionState.Connected)
        {
            if (device.HideWhenDisconnected || Dock.HideOnFullscreen)
                return false;
        }

        return true;
    }

    private int GetSortOrder(Device device)
    {
        var deviceSettings = Settings.Devices.FirstOrDefault(d => d.DeviceId == device.Id);
        if (deviceSettings != null && deviceSettings.DisplayOrder > 0)
            return deviceSettings.DisplayOrder;

        return Dock.SortOrder switch
        {
            DeviceSortOrder.ByType => (int)device.Type * 1000,
            DeviceSortOrder.ByName => device.DisplayName.GetHashCode(),
            DeviceSortOrder.ByBatteryLevel => device.BatteryInfo.AggregatedLevel ?? 101,
            DeviceSortOrder.ByLastSeen => (int)(DateTimeOffset.UtcNow - device.LastSeen).TotalSeconds,
            _ => 0
        };
    }

    private void AddDevice(Device device)
    {
        if (ShouldShowInDock(device))
        {
            Devices.Add(new DockDeviceViewModel(device, Dock));
        }
    }
}

/// <summary>
/// View model for a single device in the dock.
/// </summary>
public sealed class DockDeviceViewModel : INotifyPropertyChanged
{
    private readonly Device _device;
    private readonly DockSettings _dockSettings;
    private int _iconSize = 32;
    private bool _showPercentage = true;
    private bool _showBatteryIndicator = true;

    public event PropertyChangedEventHandler? PropertyChanged;

    public Device Device => _device;

    public DockDeviceViewModel(Device device, DockSettings dockSettings)
    {
        _device = device;
        _dockSettings = dockSettings;
        _iconSize = dockSettings.IconSize;
        _showPercentage = dockSettings.DisplayMode == DockDisplayMode.IconsWithPercent || 
                          dockSettings.DisplayMode == DockDisplayMode.ExpandedCards;
        _showBatteryIndicator = dockSettings.DisplayMode != DockDisplayMode.IconsOnly;

        _device.PropertyChanged += Device_PropertyChanged;
    }

    public int IconSize
    {
        get => _iconSize;
        set => SetField(ref _iconSize, value);
    }

    public bool ShowPercentage
    {
        get => _showPercentage;
        set => SetField(ref _showPercentage, value);
    }

    public bool ShowBatteryIndicator
    {
        get => _showBatteryIndicator;
        set => SetField(ref _showBatteryIndicator, value);
    }

    public object? IconSource => _device.IconPath != null 
        ? new System.Windows.Media.Imaging.BitmapImage(new Uri(_device.IconPath, UriKind.RelativeOrAbsolute))
        : null;

    public int? BatteryLevel => _device.BatteryInfo.AggregatedLevel;

    public string BatteryPercentageText => BatteryLevel.HasValue ? $"{BatteryLevel}%" : "—";

    public BatteryChargeState ChargeState => _device.BatteryInfo.OverallChargeState;

    public string ToolTipText => $"{_device.DisplayName}\n" +
                                 $"Заряд: {BatteryPercentageText}\n" +
                                 $"Статус: {GetChargeStateText(ChargeState)}\n" +
                                 $"Обновлено: {_device.BatteryInfo.LastUpdated:HH:mm:ss}";

    private void Device_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Device.BatteryInfo) ||
            e.PropertyName == nameof(Device.ConnectionState) ||
            e.PropertyName == nameof(Device.DisplayName))
        {
            OnPropertyChanged(nameof(BatteryLevel));
            OnPropertyChanged(nameof(BatteryPercentageText));
            OnPropertyChanged(nameof(ChargeState));
            OnPropertyChanged(nameof(ToolTipText));
        }
    }

    public void UpdateFromDevice(Device device)
    {
        // Device reference is the same, just trigger property updates
        OnPropertyChanged(nameof(BatteryLevel));
        OnPropertyChanged(nameof(BatteryPercentageText));
        OnPropertyChanged(nameof(ChargeState));
        OnPropertyChanged(nameof(ToolTipText));
    }

    private string GetChargeStateText(BatteryChargeState state) => state switch
    {
        BatteryChargeState.Charging => "Заряжается",
        BatteryChargeState.Full => "Заряжен",
        BatteryChargeState.Discharging => "Разряжается",
        BatteryChargeState.NotPresent => "Нет батареи",
        _ => "Неизвестно"
    }

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private bool SetField<T>(ref T field, T value, [System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}