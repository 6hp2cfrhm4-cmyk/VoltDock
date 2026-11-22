# Security Policy

## Supported Versions

| Version | Supported          |
| ------- | ------------------ |
| 1.x.x   | ✅ Yes             |
| < 1.0   | ❌ No              |

## Reporting a Vulnerability

We take security seriously. If you discover a security vulnerability, please report it responsibly:

### Private Disclosure
**Do not** create a public issue for security vulnerabilities.

Instead, email: **security@voltdock.example.com** (or use GitHub Security Advisories)

Include:
- Description of the vulnerability
- Steps to reproduce
- Potential impact
- Suggested fix (if any)

### Response Timeline
- **Acknowledgment**: Within 48 hours
- **Initial Assessment**: Within 7 days
- **Fix Development**: Depends on severity
- **Public Disclosure**: After fix is released

## Security Features

### No Network Access
VoltDock makes **zero network connections**:
- No telemetry
- No update checks (manual only)
- No crash reporting
- No license validation
- No cloud sync

### Local-Only Data
All data stays on your machine:
- Settings in `%LOCALAPPDATA%\VoltDock\settings.json`
- No cloud storage
- No accounts
- No analytics

### Minimal Permissions
- No administrator required
- No kernel drivers
- No system service installation
- Standard user privileges only

### Secure Defaults
- Auto-start disabled by default
- Notifications respect Focus Assist
- Diagnostic export requires explicit action
- No hidden background processes

## Dependency Security

### Regular Audits
- Automated dependency scanning in CI
- `dotnet list package --vulnerable` on every build
- Dependabot alerts monitored

### Minimal Dependencies
- Only essential NuGet packages
- Microsoft-maintained packages preferred
- No obscure or unmaintained libraries
- Native Windows APIs used where possible

## Threat Model

| Threat | Mitigation |
|--------|------------|
| Malicious device firmware | Read-only HID/GATT access, no write commands |
| DLL hijacking | Self-contained publish, no external DLLs |
| Settings tampering | User-writable AppData only, no SYSTEM access |
| UI spoofing | Native WPF, no web views |
| Resource exhaustion | Adaptive polling, rate limiting, timeouts |

## Cryptography

VoltDock does not implement custom cryptography. No encryption is needed as:
- No sensitive data stored
- No network communication
- No authentication/authorization

## Secure Development Practices

- Code reviews required for all changes
- Static analysis in CI (Roslyn analyzers)
- No `unsafe` code without justification
- No P/Invoke without validation
- Input validation on all external data

## Vulnerability Disclosure History

| Date | Version | CVE | Description |
|------|---------|-----|-------------|
| — | — | — | No known vulnerabilities |

## Contact

For security concerns: **security@voltdock.example.com**

For general questions: [GitHub Discussions](https://github.com/yourusername/VoltDock/discussions)