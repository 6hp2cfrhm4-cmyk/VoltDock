# VoltDock

<div align="center">
  <img src="docs/images/logo.png" alt="VoltDock Logo" width="128" height="128">
  
  **Beautiful, lightweight battery dock for Windows**
  
  [![Build Status](https://github.com/yourusername/VoltDock/workflows/Build%20and%20Test/badge.svg)](https://github.com/yourusername/VoltDock/actions)
  [![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
  [![Windows 10/11](https://img.shields.io/badge/Windows-10%2F11-blue.svg)](https://www.microsoft.com/windows)
  [![.NET 9](https://img.shields.io/badge/.NET-9.0-purple.svg)](https://dotnet.microsoft.com)
</div>

---

## Overview

VoltDock is a modern, native Windows application that displays battery levels of your connected devices in a beautiful, customizable floating dock. No manufacturer bloatware required — it works with standard Windows APIs and supports a wide range of devices out of the box.

### Key Features

- **🎯 Universal Device Support** — Bluetooth mice, keyboards, headphones, TWS earbuds, game controllers, 2.4 GHz USB receivers, laptop battery
- **🎨 Beautiful Dock** — Floating panel with glassmorphism, blur, smooth animations, and multiple themes
- **⚡ Real-time Updates** — Event-driven + adaptive polling, minimal CPU/memory usage
- **🔔 Smart Notifications** — Configurable thresholds, respects Focus Assist, no spam
- **🔧 Fully Customizable** — Position, size, opacity, animations, per-device settings
- **🛡️ Privacy First** — 100% local, no telemetry, no network requests, no accounts
- **📦 Portable & Lightweight** — ~5 MB, no installer required, runs from anywhere

---

## Supported Devices

| Category | Examples | Battery Source |
|----------|----------|----------------|
| **Bluetooth Mice** | Logitech MX Master, Microsoft Surface Mouse, Keychron M3 | GATT Battery Service (0x180F) |
| **Bluetooth Keyboards** | Keychron K8, Logitech MX Keys, NuPhy Air75 | GATT Battery Service |
| **Headphones/Headsets** | Sony WH-1000XM5, Bose QC45, JBL Tour One | GATT / Device Properties |
| **TWS Earbuds** | AirPods Pro, Galaxy Buds, MCHOSE V9 Pro | GATT (multi-battery: L/R/Case) |
| **Game Controllers** | Xbox Wireless, DualSense, 8BitDo | XInput / GATT |
| **2.4 GHz Receivers** | Logitech Lightspeed, Razer HyperSpeed, Attack Shark R2 | HID Battery Report / Manufacturer Protocol |
| **Laptop Battery** | Internal battery | Windows Power API |

> **Note:** Battery reading depends on what the device/firmware exposes to Windows. If a device doesn't report battery via standard protocols, it will show as "Unknown" rather than fake data.

---

## Screenshots

<div align="center">
  <img src="docs/images/dock-darkglass.png" alt="Dark Glass Dock" width="600">
  <p><em>Dark Glass theme on Windows 11</em></p>
  
  <img src="docs/images/settings.png" alt="Settings Window" width="600">
  <p><em>Settings window with live dock preview</em></p>
</div>

---

## Installation

### Portable (Recommended)
1. Download the latest `VoltDock-x64.zip` from [Releases](https://github.com/yourusername/VoltDock/releases)
2. Extract to any folder (e.g., `C:\Tools\VoltDock`)
3. Run `VoltDock.exe`
4. (Optional) Create a shortcut in `shell:startup` for auto-start

### Installer
1. Download `VoltDock-Setup.exe` from Releases
2. Run the installer
3. Follow the wizard

### Requirements
- Windows 10 19041+ (20H1) or Windows 11
- .NET 9 Runtime (included in self-contained builds)
- x64 or ARM64 processor

---

## Building from Source

### Prerequisites
- Visual Studio 2022 17.9+ or .NET 9 SDK
- Windows 10/11 SDK (10.0.19041+)
- Git

### Build Steps
```bash
# Clone repository
git clone https://github.com/yourusername/VoltDock.git
cd VoltDock

# Restore dependencies
dotnet restore VoltDock.sln

# Build
dotnet build VoltDock.sln --configuration Release --platform x64

# Run tests
dotnet test VoltDock.sln --configuration Release --platform x64

# Publish
dotnet publish src/VoltDock.App/VoltDock.App.csproj --configuration Release --platform x64 --self-contained false -o ./publish
```

---

## Architecture

```
VoltDock/
├── src/
│   ├── VoltDock.Core/          # Core logic, device discovery, battery providers
│   ├── VoltDock.UI/            # WPF controls, themes, converters
│   └── VoltDock.App/           # Application entry point, windows, viewmodels
├── tests/
│   ├── VoltDock.Core.Tests/    # Unit tests for core logic
│   └── VoltDock.UI.Tests/      # UI tests
├── docs/                       # Documentation
├── installer/                  # Installer projects (Wix/MSIX)
└── .github/workflows/          # CI/CD pipelines
```

### Core Components

| Component | Responsibility |
|-----------|----------------|
| `IDeviceDiscoveryProvider` | Discovers devices (Bluetooth LE, HID, XInput, Windows Power) |
| `IBatteryProvider` | Reads battery from sources (GATT, HID, XInput, Manufacturer) |
| `IDeviceIdentityResolver` | Deduplicates devices across multiple interfaces |
| `IBatteryReadingAggregator` | Combines readings from multiple providers |
| `VoltDockService` | Orchestrates all components, manages lifecycle |

### Battery Providers (Priority Order)
1. **Manufacturer-specific** (MCHOSE V9 Pro, Attack Shark R2, Logitech, Razer) — Highest
2. **Bluetooth GATT** (0x180F Battery Service) — Standard Bluetooth LE
3. **HID Battery Report** (Usage Page 0x000C) — USB/2.4 GHz devices
4. **XInput** — Game controllers
5. **Windows Power API** — Laptop internal battery
6. **Device Properties** — Fallback via `System.Devices.BatteryLevel`

---

## Configuration

Settings are stored in `%LOCALAPPDATA%\VoltDock\settings.json`.

### Dock Settings
```json
{
  "Dock": {
    "Position": "Bottom",           // Bottom, Top, Left, Right
    "DisplayMode": "IconsWithPercent",
    "AutoHide": "Never",
    "EdgeMargin": 20,
    "IconSize": 32,
    "CornerRadius": 16,
    "Opacity": 0.85,
    "EnableBlur": true,
    "EnableAnimations": true,
    "HideOnFullscreen": true
  }
}
```

### Notification Thresholds
```json
{
  "Notifications": {
    "Enabled": true,
    "GlobalWarningPercent": 20,
    "GlobalCriticalPercent": 10,
    "Thresholds": [
      { "DeviceId": "mouse-xyz", "WarningPercent": 15, "CriticalPercent": 5 }
    ],
    "RespectFocusAssist": true,
    "MinNotificationInterval": 30
  }
}
```

---

## Privacy

**VoltDock collects zero data.**

- No telemetry
- No analytics
- No crash reporting (unless you manually export diagnostics)
- No network connections whatsoever
- All data stays on your machine
- Open source — audit it yourself

See [PRIVACY.md](PRIVACY.md) for details.

---

## Compatibility Matrix

| Device | Bluetooth | 2.4 GHz USB | USB Wired | Notes |
|--------|-----------|-------------|-----------|-------|
| MCHOSE V9 Pro | ✅ GATT | 🔄 Research | ❓ | Multi-battery (L/R/Case) |
| Attack Shark R2 | ✅ GATT | 🔄 Research | ❓ | Gaming mouse |
| Logitech G Pro Wireless | ✅ | ✅ HID/Logi | ❓ | Via Lightspeed receiver |
| Razer Viper Ultimate | ✅ | ✅ HID/Razer | ❓ | Via HyperSpeed receiver |
| Keychron K8/K3 | ✅ GATT | ❌ | ✅ HID | Wired shows battery |
| Xbox Series Controller | ✅ GATT | ❌ | ✅ XInput | Bluetooth + USB |
| DualSense (PS5) | ✅ GATT | ❌ | ✅ HID | |
| AirPods Pro | ✅ GATT | ❌ | ❌ | Multi-battery |
| Generic HID Mouse | ❌ | ✅ HID | ✅ HID | If firmware supports |

**Legend:** ✅ Tested working | 🔄 In research | ❓ Unknown | ❌ Not supported

---

## Troubleshooting

### Device shows "Unknown" battery
1. Ensure device is paired/connected in Windows Settings
2. Check if device firmware supports battery reporting
3. Try reconnecting the device
4. Check `Advanced → Diagnostics` for provider errors

### Dock not appearing
1. Check if dock is enabled in Settings → Dock
2. Verify monitor configuration (multi-monitor setups)
3. Check `Hide on fullscreen` setting if gaming

### High CPU usage
1. Enable `Adaptive Polling` in Advanced settings
2. Increase `Polling Interval`
3. Disable unused providers in Advanced settings

### Notifications not showing
1. Check Windows Focus Assist / Do Not Disturb
2. Verify `Notifications → Enabled` in settings
3. Check per-device notification settings

---

## Contributing

We welcome contributions! Please see [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines.

### Development Setup
1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Run tests: `dotnet test`
5. Check formatting: `dotnet format --verify-no-changes`
6. Submit a PR

---

## License

MIT License — see [LICENSE](LICENSE) for details.

---

## Acknowledgments

- Windows SDK team for excellent APIs
- [Hardcodet.NotifyIcon.Wpf](https://github.com/hardcodet/wpf-notifyicon) for system tray
- [Microsoft.Xaml.Behaviors.Wpf](https://github.com/microsoft/XamlBehaviorsWpf) for behaviors
- All open-source reverse-engineering efforts for manufacturer protocols

---

## Links

- [Releases](https://github.com/yourusername/VoltDock/releases)
- [Issues](https://github.com/yourusername/VoltDock/issues)
- [Discussions](https://github.com/yourusername/VoltDock/discussions)
- [Changelog](CHANGELOG.md)
- [Security Policy](SECURITY.md)

---

<div align="center">
  <sub>Built with ❤️ for Windows users who love clean, functional software.</sub>
</div>