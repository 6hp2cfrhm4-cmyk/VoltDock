using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace VoltDock.App.ViewModels;

/// <summary>
/// Base class for view models with INotifyPropertyChanged support.
/// </summary>
public abstract class ViewModelBase : INotifyPropertyChanged
{
    protected readonly ILogger? Logger;

    protected ViewModelBase(ILogger? logger = null)
    {
        Logger = logger;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    protected void LogDebug(string message, params object?[] args)
    {
        Logger?.LogDebug(message, args);
    }

    protected void LogInformation(string message, params object?[] args)
    {
        Logger?.LogInformation(message, args);
    }

    protected void LogWarning(string message, params object?[] args)
    {
        Logger?.LogWarning(message, args);
    }

    protected void LogError(Exception ex, string message, params object?[] args)
    {
        Logger?.LogError(ex, message, args);
    }
}