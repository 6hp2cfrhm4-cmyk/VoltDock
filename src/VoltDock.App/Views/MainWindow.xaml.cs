using System.Windows;
using VoltDock.App.ViewModels;

namespace VoltDock.App.Views;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;

        // Handle window state
        StateChanged += MainWindow_StateChanged;
        Closing += MainWindow_Closing;
    }

    private void MainWindow_StateChanged(object? sender, EventArgs e)
    {
        if (WindowState == WindowState.Minimized)
        {
            // Optionally hide to tray instead of minimizing
            if (DataContext is MainViewModel vm && vm.Settings?.Behavior?.StartMinimized == true)
            {
                Hide();
            }
        }
    }

    private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        // Hide instead of closing if app should run in background
        if (DataContext is MainViewModel vm && vm.Settings?.Behavior?.StartMinimized == true)
        {
            e.Cancel = true;
            Hide();
        }
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        // Additional window initialization if needed
    }
}