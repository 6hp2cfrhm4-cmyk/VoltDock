# Build Instructions

## Prerequisites

### Required Software
- **Visual Studio 2022 17.9+** (Community/Professional/Enterprise)
  - Workload: ".NET desktop development"
  - Workload: "Universal Windows Platform development" (for Windows App SDK)
- **.NET 9 SDK** (9.0.100+)
- **Windows 10 SDK 10.0.19041.0+** (20H1) or **Windows 11 SDK**
- **Git** for version control

### Optional
- **Wix Toolset v4** — for MSI installer
- **MSIX Packaging Tool** — for MSIX package
- **Dotnet-format** — `dotnet tool install -g dotnet-format`

---

## Quick Build

```bash
# Clone
git clone https://github.com/yourusername/VoltDock.git
cd VoltDock

# Restore & Build
dotnet restore VoltDock.sln
dotnet build VoltDock.sln --configuration Release --platform x64

# Run Tests
dotnet test VoltDock.sln --configuration Release --platform x64

# Publish (framework-dependent)
dotnet publish src/VoltDock.App/VoltDock.App.csproj \
  --configuration Release --platform x64 \
  --self-contained false \
  -o ./publish/x64

# Publish (self-contained, single file)
dotnet publish src/VoltDock.App/VoltDock.App.csproj \
  --configuration Release --platform x64 \
  --self-contained true \
  -p:PublishSingleFile=true \
  -p:IncludeNativeLibrariesForSelfExtract=true \
  -p:EnableCompressionInSingleFile=true \
  -o ./publish/x64-single
```

---

## Detailed Build Steps

### 1. Restore Dependencies
```bash
dotnet restore VoltDock.sln
```
Downloads all NuGet packages defined in `.csproj` files.

### 2. Build Solution
```bash
# Debug build
dotnet build VoltDock.sln --configuration Debug --platform x64

# Release build
dotnet build VoltDock.sln --configuration Release --platform x64

# ARM64 build (requires ARM64 machine or cross-compilation setup)
dotnet build VoltDock.sln --configuration Release --platform ARM64
```

### 3. Run Tests
```bash
# All tests
dotnet test VoltDock.sln --configuration Release --platform x64

# With code coverage
dotnet test VoltDock.sln --configuration Release --platform x64 \
  --collect:"XPlat Code Coverage" \
  --results-directory ./TestResults

# Specific test project
dotnet test tests/VoltDock.Core.Tests/VoltDock.Core.Tests.csproj \
  --configuration Release --platform x64
```

### 4. Format Check
```bash
# Check formatting (CI)
dotnet format VoltDock.sln --verify-no-changes --verbosity detailed

# Fix formatting
dotnet format VoltDock.sln --verbosity detailed
```

### 5. Publish Application

#### Framework-Dependent (Recommended for Distribution)
```bash
dotnet publish src/VoltDock.App/VoltDock.App.csproj \
  --configuration Release \
  --platform x64 \
  --self-contained false \
  --runtime win-x64 \
  -o ./publish/win-x64
```
**Output:** ~5 MB, requires .NET 9 Runtime installed.

#### Self-Contained (No Runtime Required)
```bash
dotnet publish src/VoltDock.App/VoltDock.App.csproj \
  --configuration Release \
  --platform x64 \
  --self-contained true \
  --runtime win-x64 \
  -p:PublishSingleFile=false \
  -o ./publish/win-x64-selfcontained
```
**Output:** ~60-80 MB, runs without .NET installed.

#### Self-Contained Single File
```bash
dotnet publish src/VoltDock.App/VoltDock.App.csproj \
  --configuration Release \
  --platform x64 \
  --self-contained true \
  --runtime win-x64 \
  -p:PublishSingleFile=true \
  -p:IncludeNativeLibrariesForSelfExtract=true \
  -p:EnableCompressionInSingleFile=true \
  -o ./publish/win-x64-single
```
**Output:** Single `.exe` ~40-50 MB, extracts to temp on first run.

### 6. Create Release Archive
```bash
# For x64
cd ./publish/win-x64
Compress-Archive -Path * -DestinationPath ../../VoltDock-x64.zip

# For ARM64
cd ./publish/win-arm64
Compress-Archive -Path * -DestinationPath ../../VoltDock-ARM64.zip
```

---

## Visual Studio Build

1. Open `VoltDock.sln` in Visual Studio 2022
2. Select configuration: **Release** | **x64** (or **ARM64**)
3. Build → Build Solution (Ctrl+Shift+B)
4. Right-click `VoltDock.App` → Publish
   - Target: `Folder`
   - Configuration: `Release`
   - Target Framework: `net9.0-windows10.0.19041.0`
   - Deployment Mode: `Framework-Dependent` or `Self-Contained`
   - Target Runtime: `win-x64` (or `win-arm64`)

---

## CI/CD Pipeline (GitHub Actions)

The repository includes `.github/workflows/build.yml` which runs on:
- Push to `main`/`develop`
- Pull requests to `main`/`develop`
- Manual trigger
- Tag push (for releases)

### Pipeline Stages
1. **Build** — Matrix: Debug/Release × x64/ARM64
2. **Format** — `dotnet format --verify-no-changes`
3. **Security** — Dependency audit + Gitleaks secrets scan
4. **Package** — On tag push: publish x64/ARM64, create release

### Required Secrets (for Release)
- `GITHUB_TOKEN` — Auto-provided by GitHub Actions

---

## Troubleshooting

### Build Errors

**"Windows App SDK not found"**
```bash
# Install Windows App SDK runtime
# Or ensure Visual Studio has "Universal Windows Platform development" workload
```

**"xinput1_4.dll not found"**
- XInput is part of Windows, no separate install needed
- Ensure `xinput1_4.dll` is in System32 (always present on Windows 7+)

**"HID/Bluetooth APIs not available"**
- Requires Windows 10 19041+ (20H1)
- Target framework: `net9.0-windows10.0.19041.0`

**"NU1605: Detected package downgrade"**
```bash
# Clear NuGet cache
dotnet nuget locals all --clear
dotnet restore
```

### Test Failures

**"Access denied to Bluetooth/HID"**
- Tests run in CI without hardware access
- Mock providers used in unit tests
- Integration tests require physical devices

**"DPI awareness test fails"**
- Requires per-monitor DPI awareness context
- Run on Windows 10 1703+ with multiple monitors

### Publish Issues

**"Single file extraction fails"**
- Ensure `IncludeNativeLibrariesForSelfExtract=true`
- Some antivirus may block temp extraction
- Try framework-dependent instead

**"ARM64 build fails on x64 machine"**
```bash
# Cross-compilation requires ARM64 SDK
# Or build on ARM64 machine (Windows on ARM device)
```

---

## Development Build

For active development with hot reload:

```bash
# Watch mode (requires dotnet-watch)
dotnet watch run --project src/VoltDock.App/VoltDock.App.csproj

# Or run from Visual Studio with "Hot Reload" enabled
```

---

## Dependencies

### Core (VoltDock.Core)
| Package | Version | Purpose |
|---------|---------|---------|
| Microsoft.Windows.SDK.BuildTools | 10.0.26100.1 | Windows SDK |
| Microsoft.WindowsAppSDK | 1.6.250117000 | WinUI 3, Windows App SDK |
| System.Text.Json | 8.0.5 | JSON serialization |
| Microsoft.Extensions.Logging.Abstractions | 8.0.1 | Logging abstraction |
| Microsoft.Extensions.DependencyInjection.Abstractions | 8.0.1 | DI abstraction |

### UI (VoltDock.UI)
| Package | Version | Purpose |
|---------|---------|---------|
| Microsoft.WindowsAppSDK | 1.6.250117000 | WinUI 3 |
| Microsoft.Xaml.Behaviors.Wpf | 1.1.77 | WPF Behaviors |
| System.Text.Json | 8.0.5 | JSON |

### App (VoltDock.App)
| Package | Version | Purpose |
|---------|---------|---------|
| Microsoft.WindowsAppSDK | 1.6.250117000 | WinUI 3 |
| Microsoft.Extensions.Hosting | 8.0.1 | Generic host |
| Microsoft.Extensions.DependencyInjection | 8.0.1 | DI container |
| Microsoft.Extensions.Logging | 8.0.1 | Logging |
| Microsoft.Extensions.Configuration.Json | 8.0.1 | JSON config |
| Hardcodet.NotifyIcon.Wpf | 1.1.0 | System tray icon |

### Tests
| Package | Version | Purpose |
|---------|---------|---------|
| xunit | 2.9.0 | Test framework |
| xunit.runner.visualstudio | 2.8.2 | VS runner |
| Microsoft.NET.Test.Sdk | 17.11.1 | Test SDK |
| FluentAssertions | 6.12.1 | Assertions |
| Moq | 4.20.70 | Mocking |

---

## Version Management

Version defined in `src/VoltDock.App/VoltDock.App.csproj`:
```xml
<Version>1.0.0</Version>
<AssemblyVersion>1.0.0.0</AssemblyVersion>
<FileVersion>1.0.0.0</FileVersion>
<InformationalVersion>1.0.0</InformationalVersion>
```

Update before release:
1. Bump version in `.csproj`
2. Update `CHANGELOG.md`
3. Commit: `chore: release v1.0.0`
4. Tag: `git tag v1.0.0`
5. Push: `git push origin v1.0.0`

---

## Support

- **Build Issues:** [GitHub Issues](https://github.com/yourusername/VoltDock/issues)
- **Discussions:** [GitHub Discussions](https://github.com/yourusername/VoltDock/discussions)
- **Documentation:** `docs/ARCHITECTURE.md`, `docs/INSTALL.md`