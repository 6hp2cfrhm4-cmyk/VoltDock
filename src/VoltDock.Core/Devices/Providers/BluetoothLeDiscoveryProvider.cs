namespace VoltDock.Core.Devices.Providers;

using VoltDock.Core.Devices;
using Microsoft.Extensions.Logging;
using Windows.Devices.Bluetooth;
using Windows.Devices.Enumeration;

/// <summary>
/// Discovers Bluetooth LE devices using Windows.Devices.Enumeration and Windows.Devices.Bluetooth.
/// </summary>
public sealed class BluetoothLeDiscoveryProvider : DiscoveryProviderBase
{
    private DeviceWatcher? _watcher;
    private readonly Dictionary<string, Device> _discoveredDevices = new();
    private readonly SemaphoreSlim _lock = new(1, 1);

    public override string ProviderId => "bluetooth-le";
    public override string ProviderName => "Bluetooth LE Discovery";
    public override int Priority => 100;

    public BluetoothLeDiscoveryProvider(ILogger<BluetoothLeDiscoveryProvider> logger) : base(logger) { }

    public override async Task StartAsync(CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            if (_isActive) return;

            // Create watcher for Bluetooth LE devices
            var selector = BluetoothLEDevice.GetDeviceSelectorFromPairingState(true);
            _watcher = DeviceInformation.CreateWatcher(selector);

            _watcher.Added += OnDeviceAdded;
            _watcher.Updated += OnDeviceUpdated;
            _watcher.Removed += OnDeviceRemoved;
            _watcher.EnumerationCompleted += OnEnumerationCompleted;
            _watcher.Stopped += OnStopped;

            _watcher.Start();
            SetActive(true);
            _logger.LogInformation("Bluetooth LE discovery started");
        }
        finally
        {
            _lock.Release();
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            if (!_isActive) return;

            if (_watcher != null)
            {
                _watcher.Added -= OnDeviceAdded;
                _watcher.Updated -= OnDeviceUpdated;
                _watcher.Removed -= OnDeviceRemoved;
                _watcher.EnumerationCompleted -= OnEnumerationCompleted;
                _watcher.Stopped -= OnStopped;

                _watcher.Stop();
                _watcher = null;
            }

            _discoveredDevices.Clear();
            SetActive(false);
            _logger.LogInformation("Bluetooth LE discovery stopped");
        }
        finally
        {
            _lock.Release();
        }
    }

    public override IReadOnlyList<Device> GetDiscoveredDevices()
    {
        return _discoveredDevices.Values.ToList().AsReadOnly();
    }

    public override async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        // Re-query all known devices
        var devices = _discoveredDevices.Values.ToList();
        foreach (var device in devices)
        {
            await UpdateDeviceInfoAsync(device, cancellationToken);
        }
    }

    private void OnDeviceAdded(DeviceWatcher sender, DeviceInformation deviceInfo)
    {
        _ = Task.Run(async () =>
        {
            await _lock.WaitAsync();
            try
            {
                var device = await CreateDeviceFromInfoAsync(deviceInfo);
                if (device != null)
                {
                    _discoveredDevices[device.Id] = device;
                    RaiseDeviceDiscovered(device);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding Bluetooth LE device");
            }
            finally
            {
                _lock.Release();
            }
        });
    }

    private void OnDeviceUpdated(DeviceWatcher sender, DeviceInformationUpdate update)
    {
        _ = Task.Run(async () =>
        {
            await _lock.WaitAsync();
            try
            {
                if (_discoveredDevices.TryGetValue(update.Id, out var device))
                {
                    var updated = await UpdateDeviceFromInfoAsync(device, update);
                    if (updated)
                    {
                        RaiseDeviceUpdated(device, ["Name", "Properties"]);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating Bluetooth LE device");
            }
            finally
            {
                _lock.Release();
            }
        });
    }

    private void OnDeviceRemoved(DeviceWatcher sender, DeviceInformationUpdate update)
    {
        _ = Task.Run(async () =>
        {
            await _lock.WaitAsync();
            try
            {
                if (_discoveredDevices.TryGetValue(update.Id, out var device))
                {
                    _discoveredDevices.Remove(update.Id);
                    RaiseDeviceLost(device.Id, "Device removed from system");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing Bluetooth LE device");
            }
            finally
            {
                _lock.Release();
            }
        });
    }

    private void OnEnumerationCompleted(DeviceWatcher sender, object args)
    {
        _logger.LogDebug("Bluetooth LE enumeration completed");
    }

    private void OnStopped(DeviceWatcher sender, object args)
    {
        _logger.LogDebug("Bluetooth LE watcher stopped");
    }

    private async Task<Device?> CreateDeviceFromInfoAsync(DeviceInformation deviceInfo)
    {
        try
        {
            var bleDevice = await BluetoothLEDevice.FromIdAsync(deviceInfo.Id);
            if (bleDevice == null)
                return null;

            var device = new Device($"ble:{deviceInfo.Id}")
            {
                Name = bleDevice.Name ?? deviceInfo.Name,
                Type = ClassifyDeviceType(bleDevice),
                BluetoothAddress = bleDevice.BluetoothAddress.ToString("X12"),
                DeviceInstanceId = deviceInfo.Id,
                ConnectionState = bleDevice.ConnectionStatus == BluetoothConnectionStatus.Connected
                    ? DeviceConnectionState.Connected
                    : DeviceConnectionState.Paired,
            };

            // Get device properties
            if (deviceInfo.Properties.TryGetValue("System.Devices.Manufacturer", out var mfr))
                device.Manufacturer = mfr?.ToString();

            if (deviceInfo.Properties.TryGetValue("System.Devices.ModelName", out var model))
                device.Model = model?.ToString();

            // Try to get battery level from device properties
            if (deviceInfo.Properties.TryGetValue("System.Devices.BatteryLevel", out var batteryLevel))
            {
                if (batteryLevel is int level)
                {
                    device.BatteryInfo.PrimaryReading = new BatteryReading(
                        level,
                        BatteryChargeState.Unknown,
                        BatteryHealth.Unknown,
                        BatterySource.DeviceProperties);
                }
            }

            device.AddInterfacePath(deviceInfo.Id);
            device.AddBatterySource(BatterySource.DeviceProperties);

            return device;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to create device from Bluetooth LE info");
            return null;
        }
    }

    private async Task<bool> UpdateDeviceFromInfoAsync(Device device, DeviceInformationUpdate update)
    {
        var updated = false;

        if (update.Properties.TryGetValue("System.ItemNameDisplay", out var name) && name is string n)
        {
            device.Name = n;
            updated = true;
        }

        // Update connection status if available
        // Note: Connection status changes are better tracked via BluetoothLEDevice events

        device.Touch();
        return updated;
    }

    private async Task UpdateDeviceInfoAsync(Device device, CancellationToken cancellationToken)
    {
        try
        {
            var bleDevice = await BluetoothLEDevice.FromIdAsync(device.DeviceInstanceId!);
            if (bleDevice == null) return;

            device.ConnectionState = bleDevice.ConnectionStatus == BluetoothConnectionStatus.Connected
                ? DeviceConnectionState.Connected
                : DeviceConnectionState.Paired;

            // Try to read battery via GATT
            // This would be handled by the battery provider, not here
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to update device info for {DeviceId}", device.Id);
        }
    }

    private DeviceType ClassifyDeviceType(BluetoothLEDevice device)
    {
        var name = (device.Name ?? "").ToLowerInvariant();
        var appearance = device.Appearance?.RawValue ?? 0;

        // Bluetooth appearance values
        // 960 = Mouse, 961 = Keyboard, 962 = Trackpad, etc.
        return appearance switch
        {
            960 => DeviceType.Mouse,
            961 => DeviceType.Keyboard,
            962 => DeviceType.Trackpad,
            192 => DeviceType.Headphones, // Headset
            193 => DeviceType.Headphones, // Headphones
            194 => DeviceType.Headphones, // Neckband
            _ => name.Contains("mouse") ? DeviceType.Mouse :
                 name.Contains("keyboard") ? DeviceType.Keyboard :
                 name.Contains("headphone") || name.Contains("headset") || name.Contains("earbud") ? DeviceType.Headphones :
                 name.Contains("controller") || name.Contains("gamepad") ? DeviceType.GameController :
                 name.Contains("pen") || name.Contains("stylus") ? DeviceType.Stylus :
                 name.Contains("watch") || name.Contains("band") ? DeviceType.Wearable :
                 DeviceType.Unknown
        };
    }
}