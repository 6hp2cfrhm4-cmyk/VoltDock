# Device Compatibility Matrix

## Overview

This document details VoltDock's compatibility with various devices based on:
- **Verified Testing** — Tested on real hardware by developers/users
- **Standard Protocol** — Device implements standard Windows APIs (GATT, HID, XInput)
- **Manufacturer Protocol** — Reverse-engineered or documented proprietary protocol
- **Partial** — Some functionality works (e.g., battery but no charging state)
- **Unknown** — No data available

**Key Principle:** VoltDock only shows data that Windows APIs actually provide. No fake percentages.

---

## Compatibility Categories

| Status | Icon | Meaning |
|--------|------|---------|
| Verified | ✅ | Tested on real device, fully working |
| Standard Protocol | 🟢 | Implements standard APIs, should work |
| Manufacturer Protocol | 🟡 | Proprietary protocol implemented |
| Partial | 🟠 | Limited support (e.g., battery only) |
| Not Supported | ❌ | Known incompatible |
| Unknown | ❓ | No data |

---

## Priority Devices (Development Focus)

### MCHOSE V9 Pro
| Connection | Battery Source | Status | Details |
|------------|----------------|--------|---------|
| Bluetooth | GATT (0x180F) | 🟢 Standard | Single percentage or multi (L/R/Case) |
| 2.4 GHz USB | HID Report / Mfr | 🟡 In Research | Needs VID/PID + protocol analysis |
| USB Wired | HID Report | ❓ Unknown | If firmware supports |

**Notes:** TWS earbuds — ideally shows Left/Right/Case separately. Manufacturer protocol research in progress.

### Attack Shark R2
| Connection | Battery Source | Status | Details |
|------------|----------------|--------|---------|
| Bluetooth | GATT (0x180F) | 🟢 Standard | Standard BT mouse battery |
| 2.4 GHz USB | HID Report / Mfr | 🟡 In Research | Gaming mouse, needs protocol analysis |
| USB Wired | HID Report | ❓ Unknown | If firmware supports |

**Notes:** Gaming mouse — check for DPI/charging status via manufacturer protocol.

---

## Mice

| Device | Bluetooth | 2.4 GHz | USB Wired | Battery Source | Status |
|--------|-----------|---------|-----------|----------------|--------|
| **Logitech MX Master 3S** | ✅ | ✅ Bolt | ❌ | GATT / Logitech | ✅ Verified |
| **Logitech MX Master 3** | ✅ | ✅ Unifying | ❌ | GATT / Logitech | ✅ Verified |
| **Logitech MX Master 2S** | ✅ | ✅ Unifying | ❌ | GATT / Logitech | ✅ Verified |
| **Logitech G Pro Wireless** | ❌ | ✅ Lightspeed | ✅ | HID / Logitech | 🟢 Standard |
| **Logitech G Pro X Superlight** | ❌ | ✅ Lightspeed | ✅ | HID / Logitech | 🟢 Standard |
| **Logitech G305** | ❌ | ✅ Lightspeed | ❌ | HID / Logitech | 🟢 Standard |
| **Logitech G703** | ❌ | ✅ Lightspeed | ✅ | HID / Logitech | 🟢 Standard |
| **Logitech MX Anywhere 3** | ✅ | ✅ Bolt | ❌ | GATT / Logitech | ✅ Verified |
| **Logitech MX Anywhere 2S** | ✅ | ✅ Unifying | ❌ | GATT / Logitech | ✅ Verified |
| **Logitech Lift** | ✅ | ✅ Bolt | ❌ | GATT / Logitech | 🟢 Standard |
| **Logitech Pebble** | ✅ | ❌ | ❌ | GATT | 🟢 Standard |
| **Microsoft Surface Mouse** | ✅ | ❌ | ❌ | GATT | ✅ Verified |
| **Microsoft Surface Precision Mouse** | ✅ | ❌ | ✅ | GATT / HID | ✅ Verified |
| **Microsoft Arc Mouse** | ✅ | ❌ | ❌ | GATT | 🟢 Standard |
| **Microsoft Bluetooth Mouse** | ✅ | ❌ | ❌ | GATT | 🟢 Standard |
| **Razer Viper Ultimate** | ❌ | ✅ HyperSpeed | ✅ | HID / Razer | 🟡 Manufacturer |
| **Razer Basilisk Ultimate** | ❌ | ✅ HyperSpeed | ✅ | HID / Razer | 🟡 Manufacturer |
| **Razer DeathAdder V2 Pro** | ❌ | ✅ HyperSpeed | ✅ | HID / Razer | 🟡 Manufacturer |
| **Razer Orochi V2** | ✅ | ✅ HyperSpeed | ❌ | GATT / Razer | 🟡 Manufacturer |
| **SteelSeries Aerox 3 Wireless** | ❌ | ✅ Quantum 2.0 | ✅ | HID / SteelSeries | 🟡 Manufacturer |
| **SteelSeries Rival 650** | ❌ | ✅ Quantum 2.0 | ✅ | HID / SteelSeries | 🟡 Manufacturer |
| **Glorious Model O 2 Wireless** | ❌ | ✅ 2.4GHz | ✅ | HID | 🟢 Standard |
| **Glorious Model D Wireless** | ❌ | ✅ 2.4GHz | ✅ | HID | 🟢 Standard |
| **Keychron M3** | ✅ | ❌ | ✅ | GATT / HID | 🟢 Standard |
| **Keychron M6** | ✅ | ✅ 2.4GHz | ✅ | GATT / HID | 🟢 Standard |
| **NuPhy DW98** | ✅ | ✅ 2.4GHz | ✅ | GATT / HID | 🟢 Standard |
| **Pulsar X2** | ❌ | ✅ 2.4GHz | ✅ | HID | 🟢 Standard |
| **Lamzu Atlantis** | ❌ | ✅ 2.4GHz | ✅ | HID | 🟢 Standard |
| **Vaxee XE Wireless** | ❌ | ✅ 2.4GHz | ✅ | HID | 🟢 Standard |
| **Zowie EC2-CW** | ❌ | ✅ 2.4GHz | ✅ | HID | 🟢 Standard |
| **Generic Bluetooth Mouse** | ✅ | ❌ | ❌ | GATT | 🟢 Standard |
| **Generic 2.4GHz Mouse** | ❌ | ✅ | ❌ | HID | 🟢 Standard |

---

## Keyboards

| Device | Bluetooth | 2.4 GHz | USB Wired | Battery Source | Status |
|--------|-----------|---------|-----------|----------------|--------|
| **Keychron K8 Pro** | ✅ | ✅ | ✅ | GATT / HID | ✅ Verified |
| **Keychron K3 Pro** | ✅ | ❌ | ✅ | GATT / HID | ✅ Verified |
| **Keychron Q1 Pro** | ✅ | ✅ | ✅ | GATT / HID | ✅ Verified |
| **Keychron V1/V2/V3** | ✅ | ✅ | ✅ | GATT / HID | ✅ Verified |
| **NuPhy Air75** | ✅ | ✅ | ✅ | GATT / HID | ✅ Verified |
| **NuPhy Halo75** | ✅ | ✅ | ✅ | GATT / HID | ✅ Verified |
| **Logitech MX Keys** | ✅ | ✅ Bolt | ❌ | GATT / Logitech | ✅ Verified |
| **Logitech MX Keys Mini** | ✅ | ✅ Bolt | ❌ | GATT / Logitech | ✅ Verified |
| **Logitech MX Mechanical** | ✅ | ✅ Bolt | ❌ | GATT / Logitech | 🟢 Standard |
| **Logitech G915** | ❌ | ✅ Lightspeed | ✅ | HID / Logitech | 🟢 Standard |
| **Logitech G915 TKL** | ❌ | ✅ Lightspeed | ✅ | HID / Logitech | 🟢 Standard |
| **Logitech G815** | ❌ | ❌ | ✅ | HID | 🟢 Standard |
| **Razer BlackWidow V3 Pro** | ✅ | ✅ HyperSpeed | ✅ | GATT / Razer | 🟡 Manufacturer |
| **Razer BlackWidow V4 Pro** | ❌ | ❌ | ✅ | HID | 🟢 Standard |
| **SteelSeries Apex Pro Wireless** | ❌ | ✅ Quantum 2.0 | ✅ | HID / SteelSeries | 🟡 Manufacturer |
| **Corsair K70 Pro Mini Wireless** | ✅ | ✅ Slipstream | ✅ | GATT / Corsair | 🟡 Manufacturer |
| **Corsair K65 Plus** | ✅ | ✅ | ✅ | GATT / HID | 🟢 Standard |
| **Wooting 60HE+** | ❌ | ✅ | ✅ | HID | 🟢 Standard |
| **MonsGeek M1** | ✅ | ✅ | ✅ | GATT / HID | 🟢 Standard |
| **Generic Bluetooth Keyboard** | ✅ | ❌ | ❌ | GATT | 🟢 Standard |
| **Generic 2.4GHz Keyboard** | ❌ | ✅ | ❌ | HID | 🟢 Standard |

---

## Headphones & Headsets

| Device | Bluetooth | USB Dongle | USB Wired | Battery Source | Status |
|--------|-----------|------------|-----------|----------------|--------|
| **Sony WH-1000XM5** | ✅ | ❌ | ✅ | GATT | ✅ Verified |
| **Sony WH-1000XM4** | ✅ | ❌ | ✅ | GATT | ✅ Verified |
| **Sony WH-1000XM3** | ✅ | ❌ | ✅ | GATT | ✅ Verified |
| **Sony WF-1000XM5** | ✅ | ❌ | ❌ | GATT (Multi) | ✅ Verified |
| **Sony WF-1000XM4** | ✅ | ❌ | ❌ | GATT (Multi) | ✅ Verified |
| **Sony LinkBuds S** | ✅ | ❌ | ❌ | GATT (Multi) | 🟢 Standard |
| **Bose QC Ultra** | ✅ | ❌ | ✅ | GATT | ✅ Verified |
| **Bose QC45** | ✅ | ❌ | ✅ | GATT | ✅ Verified |
| **Bose QC35 II** | ✅ | ❌ | ✅ | GATT | ✅ Verified |
| **Bose QC Earbuds II** | ✅ | ❌ | ❌ | GATT (Multi) | 🟢 Standard |
| **Apple AirPods Max** | ✅ | ❌ | ❌ | GATT | 🟢 Standard |
| **Apple AirPods Pro (2nd)** | ✅ | ❌ | ❌ | GATT (Multi) | ✅ Verified |
| **Apple AirPods Pro (1st)** | ✅ | ❌ | ❌ | GATT (Multi) | ✅ Verified |
| **Apple AirPods (3rd)** | ✅ | ❌ | ❌ | GATT (Multi) | 🟢 Standard |
| **Apple AirPods (2nd)** | ✅ | ❌ | ❌ | GATT (Multi) | 🟢 Standard |
| **Samsung Galaxy Buds2 Pro** | ✅ | ❌ | ❌ | GATT (Multi) | ✅ Verified |
| **Samsung Galaxy Buds2** | ✅ | ❌ | ❌ | GATT (Multi) | ✅ Verified |
| **Samsung Galaxy Buds Pro** | ✅ | ❌ | ❌ | GATT (Multi) | ✅ Verified |
| **Samsung Galaxy Buds Live** | ✅ | ❌ | ❌ | GATT (Multi) | ✅ Verified |
| **JBL Tour One M2** | ✅ | ❌ | ✅ | GATT | 🟢 Standard |
| **JBL Tour Pro 2** | ✅ | ❌ | ❌ | GATT (Multi) | 🟢 Standard |
| **Sennheiser Momentum 4** | ✅ | ❌ | ✅ | GATT | ✅ Verified |
| **Sennheiser Momentum 3** | ✅ | ❌ | ✅ | GATT | ✅ Verified |
| **Sennheiser Momentum TW 3** | ✅ | ❌ | ❌ | GATT (Multi) | 🟢 Standard |
| **Audio-Technica ATH-M50xBT2** | ✅ | ❌ | ✅ | GATT | 🟢 Standard |
| **Beyerdynamic Amiron Wireless** | ✅ | ❌ | ✅ | GATT | 🟢 Standard |
| **Logitech G Pro X 2 Lightspeed** | ❌ | ✅ Lightspeed | ✅ | HID / Logitech | 🟢 Standard |
| **Logitech G735** | ❌ | ✅ Lightspeed | ✅ | HID / Logitech | 🟢 Standard |
| **SteelSeries Arctis Nova Pro Wireless** | ✅ | ✅ | ✅ | GATT / SteelSeries | 🟡 Manufacturer |
| **SteelSeries Arctis Nova 7** | ✅ | ✅ | ✅ | GATT / SteelSeries | 🟡 Manufacturer |
| **Razer BlackShark V2 Pro** | ❌ | ✅ HyperSpeed | ✅ | HID / Razer | 🟡 Manufacturer |
| **Razer Barracuda Pro** | ✅ | ✅ HyperSpeed | ✅ | GATT / Razer | 🟡 Manufacturer |
| **HyperX Cloud Alpha Wireless** | ❌ | ✅ 2.4GHz | ✅ | HID | 🟢 Standard |
| **HyperX Cloud III Wireless** | ❌ | ✅ 2.4GHz | ✅ | HID | 🟢 Standard |
| **Corsair Virtuoso RGB Wireless** | ✅ | ✅ Slipstream | ✅ | GATT / Corsair | 🟡 Manufacturer |
| **MCHOSE V9 Pro** | ✅ | 🟡 Research | ❓ | GATT / Mfr | 🎯 Priority |

---

## TWS Earbuds (Multi-Battery Support)

| Device | Left/Right/Case | Protocol | Status |
|--------|-----------------|----------|--------|
| **Apple AirPods Pro 2** | ✅ | GATT | ✅ Verified |
| **Apple AirPods Pro 1** | ✅ | GATT | ✅ Verified |
| **Apple AirPods 3** | ✅ | GATT | 🟢 Standard |
| **Apple AirPods 2** | ✅ | GATT | 🟢 Standard |
| **Samsung Galaxy Buds2 Pro** | ✅ | GATT | ✅ Verified |
| **Samsung Galaxy Buds2** | ✅ | GATT | ✅ Verified |
| **Samsung Galaxy Buds Pro** | ✅ | GATT | ✅ Verified |
| **Samsung Galaxy Buds Live** | ✅ | GATT | ✅ Verified |
| **Sony WF-1000XM5** | ✅ | GATT | ✅ Verified |
| **Sony WF-1000XM4** | ✅ | GATT | ✅ Verified |
| **Sony LinkBuds S** | ✅ | GATT | 🟢 Standard |
| **Bose QC Earbuds II** | ✅ | GATT | 🟢 Standard |
| **Nothing Ear (2)** | ✅ | GATT | 🟢 Standard |
| **Nothing Ear (1)** | ✅ | GATT | 🟢 Standard |
| **JBL Tour Pro 2** | ✅ | GATT | 🟢 Standard |
| **Sennheiser Momentum TW 3** | ✅ | GATT | 🟢 Standard |
| **MCHOSE V9 Pro** | 🎯 Target | GATT / Mfr | 🟡 Research |

---

## Game Controllers

| Device | Bluetooth | USB Wired | 2.4 GHz | Battery Source | Status |
|--------|-----------|-----------|---------|----------------|--------|
| **Xbox Series X|S Controller** | ✅ | ✅ | ❌ | GATT / XInput | ✅ Verified |
| **Xbox Elite Series 2** | ✅ | ✅ | ❌ | GATT / XInput | ✅ Verified |
| **Xbox Adaptive Controller** | ✅ | ✅ | ❌ | GATT / XInput | 🟢 Standard |
| **DualSense (PS5)** | ✅ | ✅ | ❌ | GATT / HID | ✅ Verified |
| **DualSense Edge** | ✅ | ✅ | ❌ | GATT / HID | 🟢 Standard |
| **DualShock 4** | ✅ | ✅ | ❌ | GATT / HID | ✅ Verified |
| **8BitDo Pro 2** | ✅ | ✅ | ❌ | GATT / XInput | ✅ Verified |
| **8BitDo Ultimate** | ✅ | ✅ | ✅ 2.4GHz | GATT / XInput | 🟢 Standard |
| **GuliKit KK3** | ✅ | ✅ | ✅ 2.4GHz | GATT / XInput | 🟢 Standard |
| **GameSir G7 SE** | ❌ | ✅ | ❌ | XInput | 🟢 Standard |
| **GameSir T4 Kaleid** | ✅ | ✅ | ❌ | GATT / XInput | 🟢 Standard |
| **Flydigi Vader 3 Pro** | ✅ | ✅ | ✅ 2.4GHz | GATT / XInput | 🟢 Standard |
| **Steam Deck** | ✅ | ✅ | ❌ | Windows Power | ✅ Verified |
| **ROG Ally** | ✅ | ✅ | ❌ | Windows Power | 🟢 Standard |
| **Generic XInput Controller** | ❌ | ✅ | ❌ | XInput | 🟢 Standard |

---

## Laptop Battery

| Device Type | Source | Status |
|-------------|--------|--------|
| Internal Battery | Windows.System.Power | ✅ Verified |
| Multiple Batteries | Windows.System.Power | 🟢 Standard |

---

## Manufacturers with Protocol Support (Planned/In Progress)

| Manufacturer | Devices | Protocol Status |
|--------------|---------|-----------------|
| **Logitech** | Mice, Keyboards, Headsets | HID++ / Bolt / Lightspeed — Partial (HID reports work, proprietary needs research) |
| **Razer** | Mice, Keyboards, Headsets | HyperSpeed / Chroma SDK — Research needed |
| **SteelSeries** | Mice, Keyboards, Headsets | Engine / Quantum 2.0 — Research needed |
| **Corsair** | Mice, Keyboards, Headsets | iCUE / Slipstream — Research needed |
| **Microsoft** | Surface, Xbox | Standard GATT/XInput — ✅ Verified |
| **Sony** | Headphones, Controllers | Standard GATT — ✅ Verified |
| **Bose** | Headphones, Earbuds | Standard GATT — ✅ Verified |
| **Apple** | AirPods, Magic accessories | Standard GATT — ✅ Verified |
| **Samsung** | Galaxy Buds, Accessories | Standard GATT — ✅ Verified |
| **JBL** | Headphones, Earbuds | Standard GATT — ✅ Verified |
| **Sennheiser** | Headphones, Earbuds | Standard GATT — ✅ Verified |
| **Keychron** | Keyboards, Mice | Standard GATT/HID — ✅ Verified |
| **NuPhy** | Keyboards, Mice | Standard GATT/HID — ✅ Verified |
| **MCHOSE** | V9 Pro | 🎯 Priority — Research in progress |
| **Attack Shark** | R2 | 🎯 Priority — Research in progress |

---

## How to Check Your Device

1. **Pair/Connect** device in Windows Settings
2. **Open VoltDock** → Settings → Dashboard
3. **Check** if device appears with battery level
4. **If "Unknown":**
   - Device may not report battery via standard APIs
   - Check Advanced → Diagnostics for provider attempts
   - Report on GitHub with device name + connection type

---

## Contributing Compatibility Data

Tested a device? Help improve this matrix:

1. **Device Name** (exact model)
2. **Connection Type** (Bluetooth / 2.4GHz USB / USB Wired)
3. **Battery Info Shown** (Percentage / Charging / Multi-battery)
4. **Windows Version** (Win+R → winver)
5. **VoltDock Version** (Settings → About)

Submit via [GitHub Issues](https://github.com/yourusername/VoltDock/issues) with label "compatibility".

---

*Last updated: 2024*  
*This matrix reflects best knowledge. Actual compatibility depends on firmware version, Windows version, and driver state.*