namespace VoltDock.Core.Devices.Providers;

using VoltDock.Core.Devices;
using Microsoft.Extensions.Logging;
using Windows.Devices.Enumeration;
using Windows.Devices.HumanInterfaceDevice;
using Windows.Storage;

/// <summary>
/// Discovers HID devices (USB, 2.4GHz receivers, Bluetooth HID) using Windows.Devices.HumanInterfaceDevice.
/// </summary>
public sealed class HidDiscoveryProvider : DiscoveryProviderBase
{
    private DeviceWatcher? _watcher;
    private readonly Dictionary<string, Device> _discoveredDevices = new();
    private readonly SemaphoreSlim _lock = new(1, 1);

    public override string ProviderId => "hid";
    public override string ProviderName => "HID Device Discovery";
    public override int Priority => 90;

    public HidDiscoveryProvider(ILogger<HidDiscoveryProvider> logger) : base(logger) { }

    public override async Task StartAsync(CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            if (_isActive) return;

            // Create watcher for all HID devices
            var selector = HidDevice.GetDeviceSelector(0, 0);
            _watcher = DeviceInformation.CreateWatcher(selector);

            _watcher.Added += OnDeviceAdded;
            _watcher.Updated += OnDeviceUpdated;
            _watcher.Removed += OnDeviceRemoved;
            _watcher.EnumerationCompleted += OnEnumerationCompleted;
            _watcher.Stopped += OnStopped;

            _watcher.Start();
            SetActive(true);
            _logger.LogInformation("HID discovery started");
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
            _logger.LogInformation("HID discovery stopped");
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
                _logger.LogError(ex, "Error adding HID device");
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
                _logger.LogError(ex, "Error updating HID device");
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
                    RaiseDeviceLost(device.Id, "HID device removed");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing HID device");
            }
            finally
            {
                _lock.Release();
            }
        });
    }

    private void OnEnumerationCompleted(DeviceWatcher sender, object args)
    {
        _logger.LogDebug("HID enumeration completed, found {Count} devices", _discoveredDevices.Count);
    }

    private void OnStopped(DeviceWatcher sender, object args)
    {
        _logger.LogDebug("HID watcher stopped");
    }

    private async Task<Device?> CreateDeviceFromInfoAsync(DeviceInformation deviceInfo)
    {
        try
        {
            var hidDevice = await HidDevice.FromIdAsync(deviceInfo.Id, FileAccessMode.Read);
            if (hidDevice == null)
                return null;

            // Check if device has battery report (Usage Page 0x000C, Usage 0x0001)
            var hasBatteryReport = HasBatteryReport(hidDevice);
            if (!hasBatteryReport)
            {
                // Still register for potential manufacturer-specific providers
                // But mark as not having standard HID battery
            }

            var device = new Device($"hid:{deviceInfo.Id}")
            {
                Name = GetDeviceName(hidDevice, deviceInfo),
                Type = ClassifyHidDevice(hidDevice, deviceInfo),
                DeviceInstanceId = deviceInfo.Id,
                ConnectionState = DeviceConnectionState.Connected,
            };

            // Get USB VID/PID
            if (ushort.TryParse(deviceInfo.Properties["System.Devices.Usb.VendorId"]?.ToString(), out var vid))
                device.UsbVendorId = vid;
            if (ushort.TryParse(deviceInfo.Properties["System.Devices.Usb.ProductId"]?.ToString(), out var pid))
                device.UsbProductId = pid;

            // Get manufacturer/model
            if (deviceInfo.Properties.TryGetValue("System.Devices.Manufacturer", out var mfr))
                device.Manufacturer = mfr?.ToString();
            if (deviceInfo.Properties.TryGetValue("System.Devices.ModelName", out var model))
                device.Model = model?.ToString();

            device.AddInterfacePath(deviceInfo.Id);
            if (hasBatteryReport)
            {
                device.AddBatterySource(BatterySource.HidReport);
            }

            return device;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to create device from HID info");
            return null;
        }
    }

    private string GetDeviceName(HidDevice hidDevice, DeviceInformation deviceInfo)
    {
        try
        {
            // Try to get product name from device properties first
            if (deviceInfo.Properties.TryGetValue("System.ItemNameDisplay", out var name) && name is string n && !string.IsNullOrEmpty(n))
                return n;
            
            return deviceInfo.Name ?? "Unknown HID Device";
        }
        catch
        {
            return deviceInfo.Name ?? "Unknown HID Device";
        }
    }

    private bool HasBatteryReport(HidDevice device)
    {
        // GetCollections is not available in the current Windows SDK version
        // Battery report detection would require parsing the HID report descriptor
        // For now, return false and rely on manufacturer-specific providers
        return false;
    }

    private async Task<bool> UpdateDeviceFromInfoAsync(Device device, DeviceInformationUpdate update)
    {
        var updated = false;

        if (update.Properties.TryGetValue("System.ItemNameDisplay", out var name) && name is string n)
        {
            device.Name = n;
            updated = true;
        }

        device.Touch();
        return updated;
    }

    private async Task UpdateDeviceInfoAsync(Device device, CancellationToken cancellationToken)
    {
        try
        {
            var hidDevice = await HidDevice.FromIdAsync(device.DeviceInstanceId!, FileAccessMode.Read);
            if (hidDevice == null)
            {
                device.ConnectionState = DeviceConnectionState.Disconnected;
                return;
            }

            device.ConnectionState = DeviceConnectionState.Connected;
        }
        catch
        {
            device.ConnectionState = DeviceConnectionState.Disconnected;
        }
    }

    private DeviceType ClassifyHidDevice(HidDevice device, DeviceInformation deviceInfo)
    {
        var name = GetDeviceName(device, deviceInfo).ToLowerInvariant();
        var usagePage = device.UsagePage;
        var usageId = device.UsageId;

        // Standard HID usage pages
        // 0x0001 = Generic Desktop
        //   0x0002 = Mouse
        //   0x0006 = Keyboard
        //   0x0008 = Gamepad/Joystick
        // 0x000C = Consumer (battery, etc.)

        if (usagePage == 0x0001)
        {
            return usageId switch
            {
                0x0002 => DeviceType.Mouse,
                0x0006 => DeviceType.Keyboard,
                0x0004 => DeviceType.Trackpad,
                0x0005 => DeviceType.GameController,
                0x0008 => DeviceType.GameController,
                _ => DeviceType.Unknown
            };
        }

        // Fallback to name-based classification
        return name.Contains("mouse") ? DeviceType.Mouse :
               name.Contains("keyboard") ? DeviceType.Keyboard :
               name.Contains("headphone") || name.Contains("headset") ? DeviceType.Headphones :
               name.Contains("earbud") ? DeviceType.TwsEarbuds :
               name.Contains("controller") || name.Contains("gamepad") ? DeviceType.GameController :
               name.Contains("trackpad") || name.Contains("touchpad") ? DeviceType.Trackpad :
               name.Contains("presenter") || name.Contains("clicker") ? DeviceType.Presenter :
               name.Contains("pen") || name.Contains("stylus") ? DeviceType.Stylus :
               DeviceType.GenericHid;
    }
}