namespace VoltDock.Core.Devices;

using Microsoft.Extensions.Logging;
using System.Text.Json;

/// <summary>
/// Provides device metadata from embedded database and manufacturer-specific overrides.
/// </summary>
public sealed class DeviceMetadataProvider : IDeviceMetadataProvider
{
    private readonly ILogger<DeviceMetadataProvider> _logger;
    private readonly Dictionary<string, DeviceMetadata> _metadataCache = new();
    private readonly Dictionary<(ushort, ushort), DeviceMetadata> _usbMetadata = new();
    private readonly Dictionary<string, DeviceMetadata> _bluetoothMetadata = new();

    public string ProviderId => "device-metadata";
    public int Priority => 100;

    public DeviceMetadataProvider(ILogger<DeviceMetadataProvider> logger)
    {
        _logger = logger;
        InitializeBuiltinMetadata();
    }

    public bool TryGetMetadata(Device device, out DeviceMetadata metadata)
    {
        // Try USB VID/PID first (most specific)
        if (device.UsbVendorId.HasValue && device.UsbProductId.HasValue)
        {
            var usbKey = (device.UsbVendorId.Value, device.UsbProductId.Value);
            if (_usbMetadata.TryGetValue(usbKey, out var usbMeta))
            {
                metadata = usbMeta;
                return true;
            }
        }

        // Try Bluetooth name pattern
        if (!string.IsNullOrEmpty(device.BluetoothAddress) || !string.IsNullOrEmpty(device.Name))
        {
            foreach (var kvp in _bluetoothMetadata)
            {
                if (MatchesBluetoothPattern(device, kvp.Key))
                {
                    metadata = kvp.Value;
                    return true;
                }
            }
        }

        // Try cache
        var cacheKey = GetCacheKey(device);
        if (_metadataCache.TryGetValue(cacheKey, out var cached))
        {
            metadata = cached;
            return true;
        }

        // Return default metadata for device type
        metadata = GetDefaultMetadata(device.Type);
        return false;
    }

    public string? GetDefaultIcon(DeviceType type)
    {
        return type.GetDefaultIconName();
    }

    private void InitializeBuiltinMetadata()
    {
        // MCHOSE V9 Pro
        _usbMetadata[(0x2D1F, 0x0001)] = new DeviceMetadata // VID/PID placeholder - need real values
        {
            DisplayName = "MCHOSE V9 Pro",
            Manufacturer = "MCHOSE",
            Model = "V9 Pro",
            Type = DeviceType.Headphones,
            IconPath = "MchoseV9Pro",
            SupportsMultiBattery = true,
            BatteryComponentNames = ["Левый", "Правый", "Кейс"],
            PreferredSources = [BatterySource.HidReport, BatterySource.BluetoothGatt, BatterySource.ManufacturerProtocol]
        };

        // Attack Shark R2
        _usbMetadata[(0x2D1F, 0x0002)] = new DeviceMetadata // VID/PID placeholder - need real values
        {
            DisplayName = "Attack Shark R2",
            Manufacturer = "Attack Shark",
            Model = "R2",
            Type = DeviceType.Mouse,
            IconPath = "AttackSharkR2",
            PreferredSources = [BatterySource.HidReport, BatterySource.BluetoothGatt, BatterySource.ManufacturerProtocol]
        };

        // Common Logitech devices
        _usbMetadata[(0x046D, 0xB01D)] = new DeviceMetadata // G Pro Wireless
        {
            DisplayName = "Logitech G Pro Wireless",
            Manufacturer = "Logitech",
            Model = "G Pro Wireless",
            Type = DeviceType.Mouse,
            IconPath = "LogitechGProWireless",
            PreferredSources = [BatterySource.HidReport, BatterySource.ManufacturerProtocol]
        };

        _usbMetadata[(0x046D, 0xB01E)] = new DeviceMetadata // G Pro X Superlight
        {
            DisplayName = "Logitech G Pro X Superlight",
            Manufacturer = "Logitech",
            Model = "G Pro X Superlight",
            Type = DeviceType.Mouse,
            IconPath = "LogitechGProXSuperlight",
            PreferredSources = [BatterySource.HidReport, BatterySource.ManufacturerProtocol]
        };

        // Common Razer devices
        _usbMetadata[(0x1532, 0x0084)] = new DeviceMetadata // Razer Viper Ultimate
        {
            DisplayName = "Razer Viper Ultimate",
            Manufacturer = "Razer",
            Model = "Viper Ultimate",
            Type = DeviceType.Mouse,
            IconPath = "RazerViperUltimate",
            PreferredSources = [BatterySource.HidReport, BatterySource.ManufacturerProtocol]
        };

        // Add more as needed
    }

    private bool MatchesBluetoothPattern(Device device, string pattern)
    {
        if (!string.IsNullOrEmpty(device.Name) &&
            System.Text.RegularExpressions.Regex.IsMatch(device.Name, pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase))
            return true;

        return false;
    }

    private string GetCacheKey(Device device)
    {
        if (!string.IsNullOrEmpty(device.BluetoothAddress))
            return $"bt:{device.BluetoothAddress}";

        if (device.UsbVendorId.HasValue && device.UsbProductId.HasValue)
            return $"usb:{device.UsbVendorId:X4}:{device.UsbProductId:X4}";

        return $"type:{device.Type}";
    }

    private DeviceMetadata GetDefaultMetadata(DeviceType type)
    {
        return new DeviceMetadata
        {
            Type = type,
            IconPath = type.GetDefaultIconName(),
            SupportsMultiBattery = type.SupportsMultiBattery(),
            BatteryComponentNames = type.SupportsMultiBattery() ? ["Левый", "Правый", "Кейс"] : [],
            PreferredSources = [BatterySource.BluetoothGatt, BatterySource.HidReport, BatterySource.DeviceProperties]
        };
    }
}