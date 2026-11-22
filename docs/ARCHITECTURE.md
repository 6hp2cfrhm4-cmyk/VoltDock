# VoltDock Architecture Documentation

## Overview

VoltDock follows a modular, provider-based architecture designed for extensibility, testability, and maintainability. The core principle is **separation of concerns**: device discovery, battery reading, identity resolution, and UI are completely decoupled.

## High-Level Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                      VoltDockService                            │
│  (Orchestrator: lifecycle, coordination, settings integration) │
└─────────────────────────────────────────────────────────────────┘
                              │
        ┌─────────────────────┼─────────────────────┐
        ▼                     ▼                     ▼
┌───────────────┐    ┌───────────────┐    ┌───────────────┐
│  Discovery    │    │   Battery     │    │  Identity     │
│  Providers    │    │  Providers    │    │  Resolver     │
└───────────────┘    └───────────────┘    └───────────────┘
        │                     │                     │
        ▼                     ▼                     ▼
┌─────────────────────────────────────────────────────────────────┐
│                    DeviceIdentityResolver                       │
│         (Canonical device registry, deduplication)              │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                   BatteryReadingAggregator                      │
│         (Priority-based merging of multi-provider readings)     │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                      NotificationService                        │
│         (Threshold evaluation, Windows Toast notifications)     │
└─────────────────────────────────────────────────────────────────┘
```

## Core Interfaces

### IDeviceDiscoveryProvider
Discovers devices from a specific source.

```csharp
public interface IDeviceDiscoveryProvider : IAsyncDisposable
{
    string ProviderId { get; }
    string ProviderName { get; }
    int Priority { get; }
    bool IsActive { get; }
    
    event EventHandler<DeviceDiscoveredEventArgs> DeviceDiscovered;
    event EventHandler<DeviceLostEventArgs> DeviceLost;
    event EventHandler<DeviceUpdatedEventArgs> DeviceUpdated;
    
    Task StartAsync(CancellationToken ct);
    Task StopAsync(CancellationToken ct);
    IReadOnlyList<Device> GetDiscoveredDevices();
    Task RefreshAsync(CancellationToken ct);
}
```

**Built-in Providers:**
- `BluetoothLeDiscoveryProvider` — Windows.Devices.Bluetooth + DeviceEnumeration
- `HidDiscoveryProvider` — Windows.Devices.HumanInterfaceDevice
- `XInputDiscoveryProvider` — XInput API (xinput1_4.dll)
- `WindowsPowerDiscoveryProvider` — Windows.System.Power.BatteryStatus

### IBatteryProvider
Reads battery level from a specific source.

```csharp
public interface IBatteryProvider : IAsyncDisposable
{
    string ProviderId { get; }
    string ProviderName { get; }
    BatterySource SourceType { get; }
    int Priority { get; }
    IReadOnlyList<DeviceType> SupportedDeviceTypes { get; }
    
    bool CanReadBattery(Device device);
    Task<BatteryReading?> ReadBatteryAsync(Device device, CancellationToken ct);
    Task<IReadOnlyList<BatteryReading>> ReadAllBatteriesAsync(Device device, CancellationToken ct);
    Task<IDisposable?> SubscribeToChangesAsync(Device device, Action<BatteryReading> callback, CancellationToken ct);
    bool SupportsNotifications(Device device);
}
```

**Built-in Providers:**
- `BluetoothGattBatteryProvider` — GATT Battery Service (0x180F/0x2A19)
- `HidBatteryProvider` — HID Battery Report (Usage Page 0x000C)
- `XInputBatteryProvider` — XInputGetBatteryInformation
- `WindowsPowerBatteryProvider` — Windows.System.Power
- `DevicePropertiesBatteryProvider` — System.Devices.BatteryLevel

**Manufacturer Providers (implement IManufacturerBatteryProvider):**
- `MchoseV9ProBatteryProvider` — MCHOSE V9 Pro headphones
- `AttackSharkR2BatteryProvider` — Attack Shark R2 mouse

### IDeviceIdentityResolver
Manages canonical device identities across providers.

```csharp
public interface IDeviceIdentityResolver
{
    Device RegisterDevice(Device device, string providerId);
    bool UnregisterDevice(string deviceId, string providerId);
    bool UpdateDevice(string deviceId, string providerId, Action<Device> updateAction);
    Device? GetDevice(string deviceId);
    IReadOnlyList<Device> GetAllDevices();
    IReadOnlyList<string> GetProviderIdsForDevice(string deviceId);
    IReadOnlyList<Device> FindDevices(Func<Device, bool> predicate);
    
    event EventHandler<DeviceEventArgs> DeviceCreated;
    event EventHandler<DeviceEventArgs> DeviceRemoved;
    event EventHandler<DeviceUpdatedEventArgs> DeviceUpdated;
}
```

**Deduplication Strategy:**
1. **Device Container ID** (strongest) — Windows groups interfaces of same physical device
2. **Bluetooth MAC Address** — For Bluetooth devices
3. **USB VID/PID + Serial** — For USB devices
4. **Interface Path Overlap** — Shared device interface paths

### IBatteryReadingAggregator
Combines readings from multiple providers with priority.

```csharp
public interface IBatteryReadingAggregator
{
    DeviceBatteryInfo Aggregate(Device device, IReadOnlyList<ProviderBatteryReading> readings);
    bool UpdateReading(Device device, ProviderBatteryReading reading);
    bool RemoveProviderReadings(Device device, string providerId);
    DeviceBatteryInfo? GetAggregatedInfo(Device device);
}
```

**Priority Order (higher = preferred):**
1. Manufacturer-specific (110)
2. Bluetooth GATT (90)
3. HID Report (90)
4. XInput (100)
5. Windows Power (100)
6. Device Properties (50) — fallback

## Data Models

### Device
Represents a physical device with merged identity.

```csharp
public sealed class Device : IEquatable<Device>
{
    string Id;                    // Canonical ID
    string Name;                  // Display name
    DeviceType Type;              // Classification
    string? Manufacturer;
    string? Model;
    string? BluetoothAddress;
    ushort? UsbVendorId;
    ushort? UsbProductId;
    string? DeviceInstanceId;     // PnP instance ID
    string? DeviceContainerId;    // PnP container ID
    HashSet<string> InterfacePaths;
    HashSet<BatterySource> BatterySources;
    DeviceBatteryInfo BatteryInfo;
    DeviceConnectionState ConnectionState;
    Dictionary<string, object> Metadata;
    DateTimeOffset FirstSeen, LastSeen;
    bool IsPinned, HideWhenDisconnected;
    string? CustomName;
}
```

### BatteryReading
Single battery measurement with metadata.

```csharp
public readonly record struct BatteryReading
{
    int? LevelPercent;              // 0-100, null = unknown
    BatteryChargeState ChargeState; // Unknown, Discharging, Charging, Full, NotPresent
    BatteryHealth Health;           // Unknown, Good, Degraded, Poor, Dead
    BatterySource Source;           // Where reading came from
    DateTimeOffset Timestamp;       // When read
    string? ComponentId;            // For multi-battery (L/R/Case)
    string? ComponentName;
    object? RawValue;               // Raw data for debugging
}
```

### DeviceBatteryInfo
Aggregated battery info for a device.

```csharp
public sealed class DeviceBatteryInfo
{
    BatteryReading PrimaryReading;
    IReadOnlyList<BatteryReading> AdditionalReadings; // TWS: Left, Right, Case
    int? AggregatedLevel;              // Min for TWS, primary for others
    BatteryChargeState OverallChargeState;
    DateTimeOffset LastUpdated;
    bool HasAnyLevel;
    bool AllStale;
}
```

## Battery Sources

| Source | Enum Value | Description |
|--------|------------|-------------|
| Bluetooth GATT | `BluetoothGatt` | Standard BT LE Battery Service |
| Bluetooth Classic | `BluetoothClassic` | Legacy BT profiles |
| HID Report | `HidReport` | USB/2.4GHz HID Battery Report |
| XInput | `XInput` | Game controllers |
| Windows Power | `WindowsPower` | Laptop internal battery |
| Device Properties | `DeviceProperties` | System.Devices.BatteryLevel |
| Manufacturer Protocol | `ManufacturerProtocol` | Brand-specific (Logitech, Razer, etc.) |
| USB Device | `UsbDevice` | USB-specific battery reporting |
| UWP Battery | `UwpBattery` | Windows.Devices.Power.Battery |
| Demo | `Demo` | Test data only |

## Event Flow

### Device Discovery
```
1. Provider.StartAsync()
2. Provider detects device → DeviceDiscovered event
3. VoltDockService.OnDeviceDiscovered()
4. IdentityResolver.RegisterDevice() → returns canonical Device
5. If new: DeviceCreated event → VoltDockService.OnDeviceCreated()
6. VoltDockService.ReadDeviceBatteryAsync() for initial reading
```

### Battery Reading
```
1. Polling timer fires (adaptive interval)
2. VoltDockService.ReadDeviceBatteryAsync(device)
3. For each applicable provider: ReadBatteryAsync()
4. Results → BatteryReadingAggregator.Aggregate()
5. Device.BatteryInfo updated
6. CheckNotifications() → NotificationService if thresholds met
```

### Real-time Notifications (where supported)
```
1. Provider.SubscribeToChangesAsync(device, callback)
2. Provider fires callback on battery change
3. VoltDockService updates aggregator
4. Immediate notification check
```

## Settings System

### ISettingsService
```csharp
public interface ISettingsService
{
    AppSettings Settings { get; }
    event EventHandler<SettingsChangedEventArgs> SettingsChanged;
    Task LoadAsync(CancellationToken ct);
    Task SaveAsync(CancellationToken ct);
    Task ResetAsync(CancellationToken ct);
    Task UpdateSettingAsync<T>(string key, T value, CancellationToken ct);
    T? GetSetting<T>(string key);
    Task ExportAsync(string filePath, CancellationToken ct);
    Task ImportAsync(string filePath, CancellationToken ct);
}
```

**Storage:** `%LOCALAPPDATA%\VoltDock\settings.json`

**Key Settings Categories:**
- `Dock` — Position, appearance, behavior
- `Appearance` — Theme, fonts, scaling
- `Notifications` — Thresholds, Focus Assist respect
- `Behavior` — Polling, providers, startup
- `Advanced` — Diagnostics, demo mode, developer options
- `Devices` — Per-device overrides

## UI Architecture (MVVM)

### ViewModels
- `MainViewModel` — Main window, navigation
- `DockViewModel` — Floating dock, device list
- `SettingsViewModel` — Settings pages
- `DashboardViewModel` — Overview
- `DeviceViewModel` — Device list/details
- `NotificationsViewModel` — Threshold management
- `AppearanceViewModel` — Theme, dock preview
- `AdvancedViewModel` — Diagnostics, providers
- `AboutViewModel` — Version, links, license

### Views
- `MainWindow.xaml` — Navigation sidebar + content area
- `DockWindow.xaml` — Floating borderless window
- `SettingsWindow.xaml` — Modal settings dialog

### Themes
Located in `VoltDock.UI/Themes/`:
- `Generic.xaml` — Base theme (Dark)
- `DarkGlass.xaml` — Glassmorphism dark
- `LightGlass.xaml` — Glassmorphism light
- `Midnight.xaml` — Deep blue/purple
- `Minimal.xaml` — Flat, no shadows

Theme switching via `AppTheme` enum and merged `ResourceDictionary`.

## Performance Considerations

### Adaptive Polling
- Base interval: 60 seconds (configurable)
- Doubles when readings stable (max 300s)
- Resets on battery change or device connect/disconnect

### Concurrency Control
- `MaxConcurrentReads` (default 3) via `SemaphoreSlim`
- Per-device read timeout (default 5s)
- Cancellation token propagation

### Memory Management
- Weak event patterns where possible
- Explicit subscription disposal on device removal
- Icon caching with size variants
- No long-lived object references

### UI Performance
- Virtualized lists for device collections
- Frozen `BitmapImage` for icons
- GPU-accelerated rendering (WPF default)
- Minimal layout passes

## Extensibility Points

### Adding a Discovery Provider
1. Implement `IDeviceDiscoveryProvider`
2. Register in `App.xaml.cs` DI container
3. Handle device-specific properties in `CreateDeviceFromInfoAsync`

### Adding a Battery Provider
1. Implement `IBatteryProvider`
2. Implement `CanReadBattery()` for device matching
3. Handle protocol-specific reading in `ReadBatteryAsync()`
4. Optional: Implement `SubscribeToChangesAsync()` for real-time
5. Register in DI container

### Adding Manufacturer Support
1. Implement `IManufacturerBatteryProvider`
2. Define `SupportedModels` with VID/PID/name patterns
3. Implement `CreateDeviceReader()` for device-specific logic
4. Implement `IDeviceBatteryReader` for reading
5. Higher priority (110) than generic providers

### Adding a Theme
1. Create `ThemeName.xaml` in `VoltDock.UI/Themes/`
2. Override colors/brushes from `Generic.xaml`
3. Add to `App.xaml` merged dictionaries
4. Add to `AppTheme` enum

## Testing Strategy

### Unit Tests (`VoltDock.Core.Tests`)
- `DeviceIdentityResolver` deduplication logic
- `BatteryReadingAggregator` priority merging
- Settings serialization/migration
- Provider `CanReadBattery` logic
- Threshold evaluation

### UI Tests (`VoltDock.UI.Tests`)
- Converter behavior
- ViewModel command execution
- Theme resource loading

### Integration Tests (Manual)
- Real device detection
- Battery reading accuracy
- Multi-monitor dock positioning
- DPI scaling (100%, 125%, 150%, 200%)
- Sleep/wake cycles
- Fullscreen game detection

## Security Considerations

- No network code in core
- No P/Invoke without validation
- Read-only device access (no write commands)
- Settings in user AppData (no SYSTEM access)
- No sensitive data in logs
- Diagnostic export anonymizes identifiers

## Deployment

### Build Configurations
- `Debug|Any CPU` — Development
- `Release|x64` — Production x64
- `Release|ARM64` — Production ARM64

### Publish Options
- Framework-dependent (smaller, requires .NET runtime)
- Self-contained (larger, no runtime needed)
- Single-file optional
- Trimmed (experimental)

### Installer
- Wix Toolset for MSI
- MSIX for Microsoft Store
- Portable ZIP for GitHub Releases

---

*Document version: 1.0*  
*Last updated: 2024*