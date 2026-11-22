namespace VoltDock.Core.Devices;

/// <summary>
/// Represents the type of a detected device.
/// </summary>
public enum DeviceType
{
    /// <summary>
    /// Unknown or unclassified device.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Computer mouse (Bluetooth, USB, 2.4GHz).
    /// </summary>
    Mouse = 1,

    /// <summary>
    /// Keyboard (Bluetooth, USB, 2.4GHz).
    /// </summary>
    Keyboard = 2,

    /// <summary>
    /// Headphones or headset (Bluetooth, USB, 2.4GHz).
    /// </summary>
    Headphones = 3,

    /// <summary>
    /// True Wireless Stereo earbuds with separate left/right/case batteries.
    /// </summary>
    TwsEarbuds = 4,

    /// <summary>
    /// Game controller / gamepad (XInput, Bluetooth, USB).
    /// </summary>
    GameController = 5,

    /// <summary>
    /// Laptop internal battery.
    /// </summary>
    LaptopBattery = 6,

    /// <summary>
    /// External battery pack / power bank.
    /// </summary>
    PowerBank = 7,

    /// <summary>
    /// Stylus / pen device.
    /// </summary>
    Stylus = 8,

    /// <summary>
    /// Trackpad or touchpad.
    /// </summary>
    Trackpad = 9,

    /// <summary>
    /// Presentation remote / clicker.
    /// </summary>
    Presenter = 10,

    /// <summary>
    /// Generic HID device with battery reporting.
    /// </summary>
    GenericHid = 11,

    /// <summary>
    /// Smartphone connected via Bluetooth/USB.
    /// </summary>
    Smartphone = 12,

    /// <summary>
    /// Tablet device.
    /// </summary>
    Tablet = 13,

    /// <summary>
    /// Smart watch or fitness band.
    /// </summary>
    Wearable = 14,
}

/// <summary>
/// Extension methods for DeviceType.
/// </summary>
public static class DeviceTypeExtensions
{
    /// <summary>
    /// Gets the default icon name for the device type.
    /// </summary>
    public static string GetDefaultIconName(this DeviceType type) => type switch
    {
        DeviceType.Mouse => "Mouse",
        DeviceType.Keyboard => "Keyboard",
        DeviceType.Headphones => "Headphones",
        DeviceType.TwsEarbuds => "Earbuds",
        DeviceType.GameController => "GameController",
        DeviceType.LaptopBattery => "LaptopBattery",
        DeviceType.PowerBank => "PowerBank",
        DeviceType.Stylus => "Stylus",
        DeviceType.Trackpad => "Trackpad",
        DeviceType.Presenter => "Presenter",
        DeviceType.GenericHid => "GenericHid",
        DeviceType.Smartphone => "Smartphone",
        DeviceType.Tablet => "Tablet",
        DeviceType.Wearable => "Wearable",
        _ => "UnknownDevice"
    };

    /// <summary>
    /// Gets the localized display name for the device type.
    /// </summary>
    public static string GetDisplayName(this DeviceType type) => type switch
    {
        DeviceType.Mouse => "Мышь",
        DeviceType.Keyboard => "Клавиатура",
        DeviceType.Headphones => "Наушники",
        DeviceType.TwsEarbuds => "TWS-наушники",
        DeviceType.GameController => "Геймпад",
        DeviceType.LaptopBattery => "Батарея ноутбука",
        DeviceType.PowerBank => "Пауэрбанк",
        DeviceType.Stylus => "Стилус",
        DeviceType.Trackpad => "Трекпад",
        DeviceType.Presenter => "Презентер",
        DeviceType.GenericHid => "HID-устройство",
        DeviceType.Smartphone => "Смартфон",
        DeviceType.Tablet => "Планшет",
        DeviceType.Wearable => "Носимое устройство",
        _ => "Неизвестное устройство"
    };

    /// <summary>
    /// Determines if this device type typically supports per-component battery reporting (e.g., TWS left/right/case).
    /// </summary>
    public static bool SupportsMultiBattery(this DeviceType type) =>
        type == DeviceType.TwsEarbuds;
}