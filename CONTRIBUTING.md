# Contributing to VoltDock

Thank you for your interest in contributing to VoltDock! This document provides guidelines for contributing to the project.

## Code of Conduct

By participating in this project, you agree to abide by our Code of Conduct:
- Be respectful and inclusive
- Welcome newcomers and help them learn
- Focus on constructive criticism
- No harassment, discrimination, or offensive behavior

## How to Contribute

### Reporting Bugs
1. Check [existing issues](https://github.com/yourusername/VoltDock/issues) first
2. Create a new issue with:
   - Clear, descriptive title
   - Steps to reproduce
   - Expected vs actual behavior
   - Windows version, VoltDock version
   - Device(s) involved
   - Relevant logs (from Advanced → Diagnostics)

### Suggesting Features
1. Check existing issues/discussions
2. Create a feature request with:
   - Clear description of the feature
   - Use case / motivation
   - Possible implementation approach
   - Mockups (if UI-related)

### Code Contributions
1. Fork the repository
2. Create a feature branch: `git checkout -b feature/amazing-feature`
3. Make your changes
4. Ensure tests pass: `dotnet test`
5. Check formatting: `dotnet format --verify-no-changes`
6. Commit with clear messages
7. Push to your fork
8. Open a Pull Request

## Development Setup

### Prerequisites
- Visual Studio 2022 17.9+ or .NET 9 SDK
- Windows 10/11 SDK (10.0.19041+)
- Git

### Getting Started
```bash
git clone https://github.com/yourusername/VoltDock.git
cd VoltDock
dotnet restore VoltDock.sln
dotnet build VoltDock.sln
```

### Running Tests
```bash
# All tests
dotnet test VoltDock.sln

# Specific project
dotnet test tests/VoltDock.Core.Tests/VoltDock.Core.Tests.csproj

# With coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Code Style
- Follow existing code style in the project
- Use `dotnet format` before committing
- Meaningful variable/method names
- Comment complex logic (why, not what)
- Keep methods small and focused

### Commit Messages
Use conventional commits:
```
feat: add support for Logitech G Pro X Superlight
fix: resolve dock positioning on multi-monitor setups
docs: update README with new installation steps
refactor: simplify battery reading aggregation
test: add unit tests for device deduplication
```

## Architecture Guidelines

### Adding New Device Providers
1. Implement `IDeviceDiscoveryProvider` for discovery
2. Implement `IBatteryProvider` for battery reading
3. Add manufacturer-specific provider if needed (`IManufacturerBatteryProvider`)
4. Register in `App.xaml.cs` DI container
5. Add tests

### Adding New Themes
1. Create new `.xaml` in `src/VoltDock.UI/Themes/`
2. Override colors/brushes from `Generic.xaml`
3. Add to `App.xaml` merged dictionaries
4. Add to `AppTheme` enum in `AppSettings.cs`

### UI Changes
- Use MVVM pattern
- Keep view logic in ViewModels
- Use data binding, avoid code-behind
- Follow Fluent Design principles
- Test with different DPI scaling (100%, 125%, 150%, 200%)

## Testing Guidelines

### Unit Tests (VoltDock.Core.Tests)
- Test pure logic: deduplication, aggregation, settings
- Mock external dependencies (Windows APIs)
- Aim for >80% coverage on core logic

### UI Tests (VoltDock.UI.Tests)
- Test view model behavior
- Test converters
- Test theme resource loading

### Hardware Testing
- Test on real devices when possible
- Document tested devices in compatibility matrix
- Never fake battery readings in tests

## Pull Request Process

1. Ensure CI passes (build, tests, format, security)
2. Update documentation if needed
3. Add changelog entry in `CHANGELOG.md`
4. Request review from maintainers
5. Address review comments
6. Squash commits if requested
7. Merge after approval

## Release Process

Releases are created by maintainers:
1. Version bump in `VoltDock.App.csproj`
2. Update `CHANGELOG.md`
3. Create git tag: `git tag v1.2.3`
4. Push tag: `git push origin v1.2.3`
5. GitHub Actions builds and publishes release

## Questions?

- Open a [Discussion](https://github.com/yourusername/VoltDock/discussions)
- Check existing [Issues](https://github.com/yourusername/VoltDock/issues)

Thank you for contributing! 🎉