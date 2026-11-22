# Installation Guide

## System Requirements

### Minimum
- **OS:** Windows 10 19041 (20H1, May 2020) or Windows 11
- **Architecture:** x64 (ARM64 supported via separate build)
- **Runtime:** .NET 9 Runtime (included in self-contained builds)
- **RAM:** 50 MB available
- **Disk:** 20 MB for application + settings

### Recommended
- Windows 11 23H2+
- 4 GB+ RAM
- SSD for faster startup

---

## Installation Methods

### Method 1: Portable (Recommended)

**Best for:** Most users, no admin rights needed, easy to update/remove.

1. **Download**
   - Go to [Releases](https://github.com/yourusername/VoltDock/releases)
   - Download `VoltDock-x64.zip` (or `VoltDock-ARM64.zip` for ARM devices)

2. **Extract**
   - Right-click ZIP → "Extract All..."
   - Choose destination (e.g., `C:\Tools\VoltDock`, `%USERPROFILE%\Apps\VoltDock`)
   - **Do not** extract to `Program Files` (requires admin for updates)

3. **Run**
   - Open extracted folder
   - Double-click `VoltDock.exe`
   - Allow Windows SmartScreen if prompted (first run only)

4. **Auto-start (Optional)**
   - Press `Win+R`, type `shell:startup`, press Enter
   - Create shortcut to `VoltDock.exe` in the Startup folder
   - Or enable "Start with Windows" in VoltDock Settings → Behavior

### Method 2: Installer (MSI)

**Best for:** Enterprise deployment, managed environments, Start Menu integration.

1. **Download**
   - Download `VoltDock-Setup.exe` from Releases

2. **Run Installer**
   - Double-click `VoltDock-Setup.exe`
   - Follow wizard:
     - Choose install location (default: `%LOCALAPPDATA%\Programs\VoltDock`)
     - Choose Start Menu folder
     - Choose desktop shortcut
   - Click Install

3. **Launch**
   - Start Menu → VoltDock
   - Or desktop shortcut

4. **Uninstall**
   - Settings → Apps → Installed apps → VoltDock → Uninstall
   - Or run installer again → "Remove"

### Method 3: Microsoft Store (Future)

*Planned for v1.1+*

### Method 4: Winget (Future)

*Planned for v1.1+*
```bash
winget install VoltDock
```

---

## Post-Installation

### First Run
1. VoltDock starts minimized to system tray (bottom-right)
2. Click tray icon → "Show Window" to open settings
3. Dock appears automatically based on settings

### Initial Configuration
1. **Settings → Dock**
   - Position: Bottom/Top/Left/Right
   - Display Mode: Icons Only / Icons + Percent / Expanded Cards
   - Auto-hide: Never / On Mouse Leave / Timeout / Edge Hover

2. **Settings → Notifications**
   - Enable notifications
   - Set warning/critical thresholds (default: 20%/10%)
   - Per-device overrides in Devices tab

3. **Settings → Appearance**
   - Theme: Dark Glass / Light Glass / Midnight / Minimal / System Auto
   - Scale, font, accent color

4. **Settings → Devices**
   - View detected devices
   - Pin favorites to dock
   - Hide when disconnected
   - Custom names

### Verify Device Detection
1. Open Settings → Dashboard
2. Check "Connected" count matches your devices
3. If device shows "Unknown" battery:
   - Ensure device is paired in Windows Settings → Bluetooth
   - Try reconnecting device
   - Check Advanced → Diagnostics for provider errors

---

## Updating

### Portable
1. Download new `VoltDock-x64.zip`
2. Extract to **same folder** (overwrite)
3. Restart VoltDock (tray → Exit → relaunch)

### Installer
1. Download new `VoltDock-Setup.exe`
2. Run installer → "Update" or "Repair"
3. Or uninstall old, install new

### Auto-Update (Planned)
*Future version will check GitHub Releases on startup*

---

## Uninstalling

### Portable
1. Right-click tray icon → Exit
2. Delete the VoltDock folder
3. Remove Startup shortcut if created

### Installer
1. Settings → Apps → VoltDock → Uninstall
2. Or: Run `VoltDock-Setup.exe` → Remove
3. **Note:** Settings in `%LOCALAPPDATA%\VoltDock\` are **preserved** for re-install

### Complete Removal (Including Settings)
```cmd
rmdir /s /q "%LOCALAPPDATA%\VoltDock"
```

---

## Troubleshooting

### "Windows protected your PC" (SmartScreen)
- Click "More info" → "Run anyway"
- This happens on first run of unsigned apps
- Future versions may be code-signed

### "VoltDock.exe is not a valid Win32 application"
- Wrong architecture: Download `x64` for 64-bit Windows, `ARM64` for ARM devices
- Corrupted download: Re-download ZIP

### "The application was unable to start correctly (0xc000007b)"
- Missing .NET Runtime: Install .NET 9 Desktop Runtime
- Or use self-contained build (larger download)

### Dock Not Appearing
1. Check tray icon → "Show Dock" is checked
2. Settings → Dock → "Auto-hide" not set to "Never" with dock off-screen
3. Multi-monitor: Check "Monitor" setting in Dock tab
4. Fullscreen app: Check "Hide on fullscreen" setting

### Device Shows "Unknown" Battery
1. Device must report battery via Windows APIs
2. Bluetooth: Must support GATT Battery Service (0x180F)
3. USB/2.4GHz: Must implement HID Battery Report (Usage Page 0x000C)
4. Manufacturer apps (Logitech G HUB, Razer Synapse) **not required**
5. Check Advanced → Diagnostics for which providers tried

### High CPU/Memory
1. Settings → Advanced → Enable "Adaptive Polling"
2. Increase "Polling Interval" to 120s+
3. Disable unused providers in Advanced
4. Restart VoltDock

### Notifications Not Working
1. Windows Settings → System → Notifications → VoltDock → On
2. Focus Assist (Win+A) → Off or "Priority only"
3. VoltDock Settings → Notifications → Enabled
4. Per-device: Devices tab → Notification toggle

### Settings Not Saving
1. Check `%LOCALAPPDATA%\VoltDock\settings.json` exists
2. Folder permissions: Your user must have write access
3. Antivirus not blocking write to AppData

---

## Command Line Arguments

```bash
# Show main window
VoltDock.exe --show

# Start minimized (default)
VoltDock.exe --minimized

# Show dock only
VoltDock.exe --dock

# Run in demo mode (fake devices)
VoltDock.exe --demo

# Export diagnostics
VoltDock.exe --export-diagnostics "C:\path\diag.json"

# Help
VoltDock.exe --help
```

---

## File Locations

| Type | Location |
|------|----------|
| Executable (Portable) | Where you extracted ZIP |
| Executable (Installed) | `%LOCALAPPDATA%\Programs\VoltDock\VoltDock.exe` |
| Settings | `%LOCALAPPDATA%\VoltDock\settings.json` |
| Logs (if enabled) | `%LOCALAPPDATA%\VoltDock\logs\` |
| Diagnostic Export | User-chosen location |
| Startup Shortcut | `%APPDATA%\Microsoft\Windows\Start Menu\Programs\Startup\VoltDock.lnk` |

---

## Enterprise Deployment

### Group Policy / Intune
- Deploy MSI via Group Policy Software Installation
- Or Intune Win32 App (`.intunewin` wrapper)
- Pre-configure settings via `settings.json` deployment

### Pre-configured Settings
Deploy `%LOCALAPPDATA%\VoltDock\settings.json` with:
```json
{
  "Behavior": { "StartWithWindows": true, "StartMinimized": true },
  "Notifications": { "Enabled": true, "GlobalWarningPercent": 15 }
}
```

### Silent Install (MSI)
```cmd
msiexec /i VoltDock.msi /quiet /norestart
```

### Silent Uninstall
```cmd
msiexec /x {PRODUCT-GUID} /quiet /norestart
```

---

## Support

- **FAQ:** This document
- **Issues:** [GitHub Issues](https://github.com/yourusername/VoltDock/issues)
- **Discussions:** [GitHub Discussions](https://github.com/yourusername/VoltDock/discussions)
- **Logs:** Enable in Settings → Advanced → Enable Diagnostics, then check `%LOCALAPPDATA%\VoltDock\logs\`

---

*Last updated: 2024*