namespace VoltDock.Core.Devices;

using Microsoft.Extensions.Logging;
using System.Windows.Media.Imaging;

/// <summary>
/// Resolves device icons from resources and system.
/// </summary>
public sealed class DeviceIconResolver : IDeviceIconResolver
{
    private readonly ILogger<DeviceIconResolver> _logger;
    private readonly IDeviceMetadataProvider _metadataProvider;
    private readonly Dictionary<string, BitmapImage> _iconCache = new();
    private readonly SemaphoreSlim _cacheLock = new(1, 1);

    public DeviceIconResolver(ILogger<DeviceIconResolver> logger, IDeviceMetadataProvider metadataProvider)
    {
        _logger = logger;
        _metadataProvider = metadataProvider;
    }

    public async Task<object?> GetIconAsync(Device device, int? size = null)
    {
        var iconName = device.EffectiveIconName;
        return await GetOrLoadIconAsync(iconName, size);
    }

    public async Task<object?> GetIconAsync(DeviceType type, int? size = null)
    {
        var iconName = type.GetDefaultIconName();
        return await GetOrLoadIconAsync(iconName, size);
    }

    public async Task<object?> GetChargingIconAsync(Device device)
    {
        var iconName = $"{device.EffectiveIconName}_Charging";
        return await GetOrLoadIconAsync(iconName, null);
    }

    public async Task PreloadIconsAsync(Device device)
    {
        var iconName = device.EffectiveIconName;
        await GetOrLoadIconAsync(iconName, 16);
        await GetOrLoadIconAsync(iconName, 24);
        await GetOrLoadIconAsync(iconName, 32);
        await GetOrLoadIconAsync(iconName, 48);
        await GetOrLoadIconAsync(iconName, 64);

        var chargingName = $"{iconName}_Charging";
        await GetOrLoadIconAsync(chargingName, 32);
    }

    public void ClearCache()
    {
        _iconCache.Clear();
    }

    private async Task<BitmapImage?> GetOrLoadIconAsync(string iconName, int? size)
    {
        var cacheKey = $"{iconName}:{size ?? 0}";

        await _cacheLock.WaitAsync();
        try
        {
            if (_iconCache.TryGetValue(cacheKey, out var cached))
                return cached;
        }
        finally
        {
            _cacheLock.Release();
        }

        var icon = await LoadIconAsync(iconName, size);

        await _cacheLock.WaitAsync();
        try
        {
            if (icon != null)
                _iconCache[cacheKey] = icon;
        }
        finally
        {
            _cacheLock.Release();
        }

        return icon;
    }

    private async Task<BitmapImage?> LoadIconAsync(string iconName, int? size)
    {
        try
        {
            // Try to load from application resources
            var resourcePath = $"pack://application:,,,/VoltDock.UI;component/Resources/Icons/{iconName}.png";
            var uri = new Uri(resourcePath, UriKind.Absolute);

            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = uri;
            if (size.HasValue)
            {
                bitmap.DecodePixelWidth = size.Value;
                bitmap.DecodePixelHeight = size.Value;
            }
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            bitmap.Freeze();

            return bitmap;
        }
        catch
        {
            // Fallback to generic icon
            return await LoadFallbackIconAsync(size);
        }
    }

    private async Task<BitmapImage?> LoadFallbackIconAsync(int? size)
    {
        try
        {
            var resourcePath = "pack://application:,,,/VoltDock.UI;component/Resources/Icons/UnknownDevice.png";
            var uri = new Uri(resourcePath, UriKind.Absolute);

            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = uri;
            if (size.HasValue)
            {
                bitmap.DecodePixelWidth = size.Value;
                bitmap.DecodePixelHeight = size.Value;
            }
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            bitmap.Freeze();

            return bitmap;
        }
        catch
        {
            return null;
        }
    }
}