# Privacy Policy

**Effective Date:** 2024-01-01  
**Last Updated:** 2024-01-01

## Summary

**VoltDock collects zero personal data. Zero. None.**

This is not marketing speak — it's a technical reality. The application has no network capabilities, no analytics SDKs, no crash reporters, and no way to send data anywhere.

---

## What We Collect

### Nothing

| Data Type | Collected? | Details |
|-----------|------------|---------|
| Personal Information | ❌ No | Name, email, IP, location, etc. |
| Device Identifiers | ❌ No | MAC addresses, serial numbers, hardware IDs |
| Usage Statistics | ❌ No | Feature usage, session length, errors |
| Battery Data | ❌ No | Levels, charging states, history |
| Crash Reports | ❌ No | Stack traces, memory dumps |
| Settings/Preferences | ❌ No | Synced or uploaded anywhere |

---

## What Stays on Your Machine

### Local Settings
- **Location:** `%LOCALAPPDATA%\VoltDock\settings.json`
- **Content:** Dock position, theme, notification thresholds, device preferences
- **Access:** Only readable by your user account
- **Encryption:** Plain text (no sensitive data to protect)

### Logs (Optional)
- **Location:** `%LOCALAPPDATA%\VoltDock\logs\` (if diagnostics enabled)
- **Content:** Debug information for troubleshooting
- **Retention:** Never uploaded, deleted on uninstall
- **Control:** Disabled by default, user-enabled only

### Diagnostic Export (Manual Only)
- **Trigger:** User explicitly clicks "Export Diagnostics" in Advanced settings
- **Content:** Anonymized device list, provider status, settings (no MACs, serials, names)
- **Destination:** User-chosen file location
- **Purpose:** Troubleshooting, sharing with developers

---

## Network Activity

### Zero Network Connections

VoltDock does not:
- Check for updates automatically
- Send heartbeat/ping requests
- Contact license servers
- Download assets/configs
- Use CDN or cloud services
- Report crashes/errors
- Load remote resources

You can verify this with:
- Windows Firewall (no outbound rules needed)
- Process Monitor (no network I/O)
- Wireshark (no packets)

---

## Third-Party Services

**None.** VoltDock does not integrate with:
- Google Analytics / Firebase
- Microsoft App Center / HockeyApp
- Sentry / Rollbar / Bugsnag
- Mixpanel / Amplitude / Segment
- Any analytics or crash reporting service

---

## Device Data Handling

### Battery Information
- Read from Windows APIs only
- Never stored persistently (only in-memory for display)
- Never transmitted
- Deleted when device disconnects

### Device Identifiers
- MAC addresses, USB VID/PID, serial numbers used **only** for:
  - Device deduplication (matching same device across interfaces)
  - Display in UI (user-visible only)
- Never logged, exported, or sent anywhere

---

## Your Rights

Since we collect nothing, there's nothing to:
- Access
- Rectify
- Delete
- Port
- Restrict processing of
- Object to

You own your data because it never leaves your computer.

---

## Children's Privacy

VoltDock does not knowingly collect data from anyone, including children under 13. Since we collect no data at all, COPPA/GDPR-K compliance is inherent.

---

## Changes to This Policy

Any changes will be:
1. Published in this file
2. Noted in CHANGELOG.md
3. Not silently applied

We have no mechanism to notify you of changes (no email, no in-app notifications, no network).

---

## Contact

Questions about privacy? Since we have no data about you, there's nothing to request. But you can:
- Open a [GitHub Discussion](https://github.com/yourusername/VoltDock/discussions)
- Check the source code yourself (it's open source)

---

## Legal Basis (GDPR)

| Processing Activity | Legal Basis |
|---------------------|-------------|
| None | N/A — no processing occurs |

---

## Compliance

- ✅ GDPR (no personal data processed)
- ✅ CCPA (no personal information collected)
- ✅ LGPD (no dados pessoais coletados)
- ✅ PIPEDA (no personal information collected)
- ✅ All other privacy laws (by virtue of collecting nothing)

---

**TL;DR:** VoltDock is a local-only tool. What happens in VoltDock stays in VoltDock (on your disk).