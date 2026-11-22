namespace VoltDock.Core.Devices.Providers;

using VoltDock.Core.Devices;
using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices;

/// <summary>
/// Discovers XInput devices (game controllers) and reads battery information.
/// </summary>
public sealed class XInputDiscoveryProvider : DiscoveryProviderBase
{
    private readonly Dictionary<string, Device> _discoveredDevices = new();
    private readonly Timer? _pollTimer;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private const int MaxControllers = 4;

    public override string ProviderId => "xinput";
    public override string ProviderName => "XInput Game Controller Discovery";
    public override int Priority => 80;

    public XInputDiscoveryProvider(ILogger<XInputDiscoveryProvider> logger) : base(logger)
    {
        _pollTimer = new Timer(PollControllers, null, Timeout.Infinite, Timeout.Infinite);
    }

    public override async Task StartAsync(CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            if (_isActive) return;

            // Initial scan
            await ScanControllersAsync(cancellationToken);

            // Start polling timer (every 5 seconds)
            _pollTimer?.Change(TimeSpan.Zero, TimeSpan.FromSeconds(5));

            SetActive(true);
            _logger.LogInformation("XInput discovery started");
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

            _pollTimer?.Change(Timeout.Infinite, Timeout.Infinite);
            _discoveredDevices.Clear();
            SetActive(false);
            _logger.LogInformation("XInput discovery stopped");
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
        await ScanControllersAsync(cancellationToken);
    }

    private void PollControllers(object? state)
    {
        _ = Task.Run(async () =>
        {
            try
            {
                await ScanControllersAsync(CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error polling XInput controllers");
            }
        });
    }

    private async Task ScanControllersAsync(CancellationToken cancellationToken)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            var currentIds = new HashSet<string>();

            for (uint i = 0; i < MaxControllers; i++)
            {
                if (cancellationToken.IsCancellationRequested) break;

                var batteryInfo = GetBatteryInformation(i);
                if (batteryInfo.HasValue)
                {
                    var deviceId = $"xinput:{i}";
                    currentIds.Add(deviceId);

                    if (!_discoveredDevices.TryGetValue(deviceId, out var device))
                    {
                        device = new Device(deviceId)
                        {
                            Name = $"XInput Controller {i + 1}",
                            Type = DeviceType.GameController,
                            ConnectionState = DeviceConnectionState.Connected,
                        };
                        device.AddInterfacePath($"xinput:{i}");
                        device.AddBatterySource(BatterySource.XInput);
                        _discoveredDevices[deviceId] = device;
                        RaiseDeviceDiscovered(device);
                    }

                    // Update battery info
                    var reading = batteryInfo.Value;
                    device.BatteryInfo.PrimaryReading = new BatteryReading(
                        reading.LevelPercent,
                        reading.ChargeState,
                        BatteryHealth.Unknown,
                        BatterySource.XInput,
                        componentId: "main",
                        componentName: "Controller");
                    device.Touch();
                }
            }

            // Remove disconnected controllers
            var toRemove = _discoveredDevices.Keys.Where(id => !currentIds.Contains(id)).ToList();
            foreach (var id in toRemove)
            {
                if (_discoveredDevices.TryGetValue(id, out var device))
                {
                    _discoveredDevices.Remove(id);
                    RaiseDeviceLost(device.Id, "Controller disconnected");
                }
            }
        }
        finally
        {
            _lock.Release();
        }
    }

    private (int? LevelPercent, BatteryChargeState ChargeState)? GetBatteryInformation(uint controllerIndex)
    {
        try
        {
            var result = XInputGetBatteryInformation(controllerIndex, 0, out var batteryInfo);
            if (result != 0) // ERROR_SUCCESS = 0
                return null;

            var batteryType = batteryInfo.BatteryType;
            var batteryLevel = batteryInfo.BatteryLevel;

            // Battery types: 0 = Disconnected, 1 = Wired, 2 = Alkaline, 3 = NiMH, 4 = Unknown
            if (batteryType == 0) // BATTERY_TYPE_DISCONNECTED
                return null;

            if (batteryType == 1) // BATTERY_TYPE_WIRED
                return (100, BatteryChargeState.Charging);

            // Battery levels: 0 = Empty, 1 = Low, 2 = Medium, 3 = Full
            var level = batteryLevel switch
            {
                0 => 0,      // BATTERY_LEVEL_EMPTY
                1 => 20,     // BATTERY_LEVEL_LOW
                2 => 50,     // BATTERY_LEVEL_MEDIUM
                3 => 100,    // BATTERY_LEVEL_FULL
                _ => (int?)null
            };

            var chargeState = batteryLevel switch
            {
                3 => BatteryChargeState.Full,
                0 => BatteryChargeState.Discharging,
                _ => BatteryChargeState.Discharging
            };

            return (level, chargeState);
        }
        catch
        {
            return null;
        }
    }

    // XInput DLL imports
    [DllImport("xinput1_4.dll", CallingConvention = CallingConvention.StdCall)]
    private static extern uint XInputGetBatteryInformation(uint dwUserIndex, byte devType, out XINPUT_BATTERY_INFORMATION pBatteryInformation);

    [StructLayout(LayoutKind.Sequential)]
    private struct XINPUT_BATTERY_INFORMATION
    {
        public byte BatteryType;
        public byte BatteryLevel;
    }

    public override async ValueTask DisposeAsync()
    {
        _pollTimer?.Dispose();
        await base.DisposeAsync();
    }
}