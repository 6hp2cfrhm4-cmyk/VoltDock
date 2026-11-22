namespace VoltDock.Core.Devices;

using System.Collections.Concurrent;

/// <summary>
/// Resolves and merges device identities from multiple discovery providers.
/// </summary>
public sealed class DeviceIdentityResolver : IDeviceIdentityResolver
{
    private readonly ConcurrentDictionary<string, Device> _devices = new();
    private readonly ConcurrentDictionary<string, HashSet<string>> _deviceProviders = new();
    private readonly object _lock = new();

    public event EventHandler<DeviceEventArgs>? DeviceCreated;
    public event EventHandler<DeviceEventArgs>? DeviceRemoved;
    public event EventHandler<DeviceUpdatedEventArgs>? DeviceUpdated;

    public Device RegisterDevice(Device device, string providerId)
    {
        lock (_lock)
        {
            // Check if this device matches an existing one
            var existingDevice = FindMatchingDevice(device);
            if (existingDevice != null)
            {
                // Merge with existing device
                MergeDevice(existingDevice, device, providerId);
                _deviceProviders[existingDevice.Id].Add(providerId);
                RaiseDeviceUpdated(existingDevice, ["Merged", "Providers"]);
                return existingDevice;
            }

            // New device
            var canonicalId = GenerateCanonicalId(device, providerId);
            device.Id = canonicalId;
            _devices[canonicalId] = device;
            _deviceProviders[canonicalId] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { providerId };

            RaiseDeviceCreated(device);
            return device;
        }
    }

    public bool UnregisterDevice(string deviceId, string providerId)
    {
        lock (_lock)
        {
            if (!_deviceProviders.TryGetValue(deviceId, out var providers))
                return false;

            providers.Remove(providerId);

            if (providers.Count == 0)
            {
                _deviceProviders.TryRemove(deviceId, out _);
                if (_devices.TryRemove(deviceId, out var device))
                {
                    RaiseDeviceRemoved(device);
                    return true;
                }
            }

            return false;
        }
    }

    public bool UpdateDevice(string deviceId, string providerId, Action<Device> updateAction)
    {
        lock (_lock)
        {
            if (!_devices.TryGetValue(deviceId, out var device))
                return false;

            var updatedProperties = new List<string>();
            // We can't easily track which properties changed without more complex logic
            // For now, just apply the update
            updateAction(device);

            RaiseDeviceUpdated(device, updatedProperties);
            return true;
        }
    }

    public Device? GetDevice(string deviceId)
    {
        _devices.TryGetValue(deviceId, out var device);
        return device;
    }

    public IReadOnlyList<Device> GetAllDevices()
    {
        return _devices.Values.OrderBy(d => d.DisplayName).ToList().AsReadOnly();
    }

    public IReadOnlyList<string> GetProviderIdsForDevice(string deviceId)
    {
        if (_deviceProviders.TryGetValue(deviceId, out var providers))
            return providers.ToList().AsReadOnly();
        return [];
    }

    public IReadOnlyList<Device> FindDevices(Func<Device, bool> predicate)
    {
        return _devices.Values.Where(predicate).ToList().AsReadOnly();
    }

    private Device? FindMatchingDevice(Device newDevice)
    {
        foreach (var existing in _devices.Values)
        {
            if (existing.IsSamePhysicalDevice(newDevice))
                return existing;
        }
        return null;
    }

    private void MergeDevice(Device existing, Device newDevice, string providerId)
    {
        // Update properties from new device if they're more specific
        if (!string.IsNullOrEmpty(newDevice.Name) && string.IsNullOrEmpty(existing.Name))
            existing.Name = newDevice.Name;

        if (existing.Type == DeviceType.Unknown && newDevice.Type != DeviceType.Unknown)
            existing.Type = newDevice.Type;

        if (!string.IsNullOrEmpty(newDevice.Manufacturer) && string.IsNullOrEmpty(existing.Manufacturer))
            existing.Manufacturer = newDevice.Manufacturer;

        if (!string.IsNullOrEmpty(newDevice.Model) && string.IsNullOrEmpty(existing.Model))
            existing.Model = newDevice.Model;

        if (!string.IsNullOrEmpty(newDevice.BluetoothAddress) && string.IsNullOrEmpty(existing.BluetoothAddress))
            existing.BluetoothAddress = newDevice.BluetoothAddress;

        if (newDevice.UsbVendorId.HasValue && !existing.UsbVendorId.HasValue)
            existing.UsbVendorId = newDevice.UsbVendorId;

        if (newDevice.UsbProductId.HasValue && !existing.UsbProductId.HasValue)
            existing.UsbProductId = newDevice.UsbProductId;

        if (!string.IsNullOrEmpty(newDevice.DeviceInstanceId) && string.IsNullOrEmpty(existing.DeviceInstanceId))
            existing.DeviceInstanceId = newDevice.DeviceInstanceId;

        if (!string.IsNullOrEmpty(newDevice.DeviceContainerId) && string.IsNullOrEmpty(existing.DeviceContainerId))
            existing.DeviceContainerId = newDevice.DeviceContainerId;

        // Merge interface paths
        foreach (var path in newDevice.InterfacePaths)
            existing.AddInterfacePath(path);

        // Merge battery sources
        foreach (var source in newDevice.BatterySources)
            existing.AddBatterySource(source);

        // Update connection state if new device is connected
        if (newDevice.ConnectionState == DeviceConnectionState.Connected)
            existing.ConnectionState = DeviceConnectionState.Connected;

        existing.Touch();
    }

    private string GenerateCanonicalId(Device device, string providerId)
    {
        // Prefer stable identifiers
        if (!string.IsNullOrEmpty(device.DeviceContainerId))
            return $"container:{device.DeviceContainerId}";

        if (!string.IsNullOrEmpty(device.BluetoothAddress))
            return $"bt:{device.BluetoothAddress}";

        if (device.UsbVendorId.HasValue && device.UsbProductId.HasValue && !string.IsNullOrEmpty(device.SerialNumber))
            return $"usb:{device.UsbVendorId:X4}:{device.UsbProductId:X4}:{device.SerialNumber}";

        if (device.UsbVendorId.HasValue && device.UsbProductId.HasValue)
            return $"usb:{device.UsbVendorId:X4}:{device.UsbProductId:X4}:{providerId}";

        // Fallback to provider-specific ID
        return $"{providerId}:{device.Id}";
    }

    private void RaiseDeviceCreated(Device device)
    {
        DeviceCreated?.Invoke(this, new DeviceEventArgs(device));
    }

    private void RaiseDeviceRemoved(Device device)
    {
        DeviceRemoved?.Invoke(this, new DeviceEventArgs(device));
    }

    private void RaiseDeviceUpdated(Device device, IEnumerable<string> updatedProperties)
    {
        DeviceUpdated?.Invoke(this, new DeviceUpdatedEventArgs(device, updatedProperties));
    }
}