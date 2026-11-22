# Changelog

All notable changes to VoltDock will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- Initial project structure and architecture
- Core device discovery providers (Bluetooth LE, HID, XInput, Windows Power)
- Battery providers (GATT, HID, XInput, Windows Power, Device Properties)
- Manufacturer-specific providers (MCHOSE V9 Pro, Attack Shark R2)
- Device identity resolution and deduplication
- Battery reading aggregation
- Settings system with JSON storage
- Notification system with Windows Toast
- Modern WPF UI with multiple themes (Dark Glass, Light Glass, Midnight, Minimal)
- Floating dock window with animations
- Main settings window with navigation
- System tray integration
- GitHub Actions CI/CD pipeline
- Comprehensive documentation

### Changed
- N/A

### Fixed
- N/A

### Security
- No network connections implemented
- No telemetry or analytics
- Local-only data storage

## [1.0.0] - 2024-XX-XX

### Added
- First stable release
- Core device detection and battery monitoring
- Beautiful floating dock with customizable appearance
- Per-device settings and notification thresholds
- Multi-monitor support
- DPI-aware UI
- Portable and installer distributions

---

## Release Types

- **Major** (X.0.0): Breaking changes, major architecture changes
- **Minor** (0.X.0): New features, backwards compatible
- **Patch** (0.0.X): Bug fixes, small improvements

## Versioning

VoltDock uses Semantic Versioning. Version numbers follow `MAJOR.MINOR.PATCH` format.

## Links

- [Releases](https://github.com/yourusername/VoltDock/releases)
- [Compare v1.0.0...HEAD](https://github.com/yourusername/VoltDock/compare/v1.0.0...HEAD)