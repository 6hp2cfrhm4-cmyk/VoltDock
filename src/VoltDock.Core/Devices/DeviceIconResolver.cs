namespace VoltDock.Core.Devices;

using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

/// <summary>
/// Resolves device icons - core implementation returning generic icon info.
/// WPF-specific implementation is in VoltDock.UI.
/// </summary>
public sealed class DeviceIconResolver : IDeviceIconResolver
{
    private readonly ILogger<DeviceIconResolver> _logger;
    private readonly IDeviceMetadataProvider _metadataProvider;
    private readonly ConcurrentDictionary<string, IconInfo> _iconCache = new();

    public DeviceIconResolver(ILogger<DeviceIconResolver> logger, IDeviceMetadataProvider metadataProvider)
    {
        _logger = logger;
        _metadataProvider = metadataProvider;
    }

    public Task<object?> GetIconAsync(Device device, int? size = null)
    {
        var iconName = device.EffectiveIconName;
        return Task.FromResult<object?>(GetIconInfo(iconName, size));
    }

    public Task<object?> GetIconAsync(DeviceType type, int? size = null)
    {
        var iconName = type.GetDefaultIconName();
        return Task.FromResult<object?>(GetIconInfo(iconName, size));
    }

    public Task<object?> GetChargingIconAsync(Device device)
    {
        var iconName = $"{device.EffectiveIconName}_Charging";
        return Task.FromResult<object?>(GetIconInfo(iconName, null));
    }

    public Task PreloadIconsAsync(Device device)
    {
        var iconName = device.EffectiveIconName;
        GetIconInfo(iconName, 16);
        GetIconInfo(iconName, 24);
        GetIconInfo(iconName, 32);
        GetIconInfo(iconName, 48);
        GetIconInfo(iconName, 64);

        var chargingName = $"{iconName}_Charging";
        GetIconInfo(chargingName, 32);
        
        return Task.CompletedTask;
    }

    public void ClearCache()
    {
        _iconCache.Clear();
    }

    private IconInfo GetIconInfo(string iconName, int? size)
    {
        var cacheKey = $"{iconName}:{size ?? 0}";
        return _iconCache.GetOrAdd(cacheKey, _ => new IconInfo
        {
            ResourcePath = $"pack://application:,,,/VoltDock.UI;component/Resources/Icons/{iconName}.png",
            Size = size,
            IsFallback = false
        });
    }

    /// <summary>
    /// Icon information for UI layer to create actual images.
    /// </summary>
    public sealed class IconInfo
    {
        public string ResourcePath { get; init; } = string.Empty;
        public int? Size { get; init; }
        public bool IsFallback { get; init; }
    }
}