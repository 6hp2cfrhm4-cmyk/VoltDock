using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using VoltDock.Core.Devices;

namespace VoltDock.App.Views;

/// <summary>
/// Converts battery level (0-100) to a color.
/// </summary>
public class BatteryLevelToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int level)
        {
            if (level >= 80)
                return (Color)ColorConverter.ConvertFromString("#4CAF50"); // Green
            if (level >= 50)
                return (Color)ColorConverter.ConvertFromString("#FFC107"); // Amber
            if (level >= 20)
                return (Color)ColorConverter.ConvertFromString("#FF9800"); // Orange
            return (Color)ColorConverter.ConvertFromString("#F44336"); // Red
        }
        return (Color)ColorConverter.ConvertFromString("#757575"); // Gray for unknown
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

/// <summary>
/// Converts battery level (0-100) to width for progress bar.
/// </summary>
public class BatteryLevelToWidthConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int level && parameter is double maxWidth)
        {
            return Math.Max(0, Math.Min(maxWidth, maxWidth * level / 100.0));
        }
        return 0.0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

/// <summary>
/// Converts charge state to visibility for charging animation.
/// </summary>
public class ChargeStateToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is BatteryChargeState state)
        {
            return state == BatteryChargeState.Charging ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

/// <summary>
/// Converts bool to Visibility.
/// </summary>
public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool b)
        {
            return b ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

/// <summary>
/// Converts device connection state to opacity.
/// </summary>
public class ConnectionStateToOpacityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is DeviceConnectionState state)
        {
            return state == DeviceConnectionState.Connected ? 1.0 : 0.5;
        }
        return 0.5;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}