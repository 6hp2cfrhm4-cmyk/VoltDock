using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using VoltDock.App.ViewModels;
using VoltDock.Core.Settings;

namespace VoltDock.App.Views;

/// <summary>
/// Floating dock window showing device battery levels.
/// </summary>
public partial class DockWindow : Window
{
    private readonly DockViewModel _viewModel;
    private Point _dragStartPoint;
    private bool _isDragging;
    private DispatcherTimer? _autoHideTimer;
    private bool _isMouseOver;

    public DockWindow(DockViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;

        // Apply settings
        ApplySettings(_viewModel.Settings?.Dock);

        // Setup auto-hide timer
        _autoHideTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(500)
        };
        _autoHideTimer.Tick += AutoHideTimer_Tick;

        // Subscribe to settings changes
        if (_viewModel.Settings != null)
        {
            _viewModel.Settings.Dock.PropertyChanged += DockSettings_PropertyChanged;
        }
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        // Position dock based on settings
        PositionDock();

        // Register for DPI changes
        // This would require handling WM_DPICHANGED in a production app
    }

    private void ApplySettings(DockSettings? settings)
    {
        if (settings == null) return;

        // Corner radius
        var cornerRadius = new CornerRadius(settings.CornerRadius);
        DockBorder.CornerRadius = cornerRadius;

        // Opacity
        DockBorder.Opacity = settings.Opacity;

        // Blur effect
        if (settings.EnableBlur)
        {
            // Apply acrylic/blur effect
            // This requires WindowChrome or custom implementation
        }

        // Animation duration
        // Animations are defined in XAML with DynamicResource
    }

    private void PositionDock()
    {
        if (_viewModel.Settings?.Dock == null) return;

        var settings = _viewModel.Settings.Dock;
        var screen = GetTargetScreen();
        var workArea = screen.WorkingArea;

        double left = 0, top = 0;

        switch (settings.Position)
        {
            case DockPosition.Bottom:
                left = workArea.Left + (workArea.Width - ActualWidth) / 2 + settings.Offset;
                top = workArea.Bottom - ActualHeight - settings.EdgeMargin;
                break;
            case DockPosition.Top:
                left = workArea.Left + (workArea.Width - ActualWidth) / 2 + settings.Offset;
                top = workArea.Top + settings.EdgeMargin;
                break;
            case DockPosition.Left:
                left = workArea.Left + settings.EdgeMargin;
                top = workArea.Top + (workArea.Height - ActualHeight) / 2 + settings.Offset;
                break;
            case DockPosition.Right:
                left = workArea.Right - ActualWidth - settings.EdgeMargin;
                top = workArea.Top + (workArea.Height - ActualHeight) / 2 + settings.Offset;
                break;
        }

        Left = left;
        Top = top;
    }

    private System.Windows.Forms.Screen GetTargetScreen()
    {
        if (!string.IsNullOrEmpty(_viewModel.Settings?.Dock?.MonitorId))
        {
            // Find specific monitor
            foreach (System.Windows.Forms.Screen screen in System.Windows.Forms.Screen.AllScreens)
            {
                if (screen.DeviceName == _viewModel.Settings.Dock.MonitorId)
                    return screen;
            }
        }

        // Default to primary screen
        return System.Windows.Forms.Screen.PrimaryScreen ?? System.Windows.Forms.Screen.AllScreens[0];
    }

    private void DockSettings_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(DockSettings.Position) ||
            e.PropertyName == nameof(DockSettings.EdgeMargin) ||
            e.PropertyName == nameof(DockSettings.Offset))
        {
            PositionDock();
        }
        else if (e.PropertyName == nameof(DockSettings.CornerRadius))
        {
            DockBorder.CornerRadius = new CornerRadius(_viewModel.Settings.Dock.CornerRadius);
        }
        else if (e.PropertyName == nameof(DockSettings.Opacity))
        {
            DockBorder.Opacity = _viewModel.Settings.Dock.Opacity;
        }
        else if (e.PropertyName == nameof(DockSettings.AutoHide))
        {
            UpdateAutoHideBehavior();
        }
    }

    private void UpdateAutoHideBehavior()
    {
        if (_viewModel.Settings?.Dock == null) return;

        if (_viewModel.Settings.Dock.AutoHide != AutoHideMode.Never)
        {
            _autoHideTimer?.Start();
        }
        else
        {
            _autoHideTimer?.Stop();
            DockBorder.Opacity = _viewModel.Settings.Dock.Opacity;
        }
    }

    private void AutoHideTimer_Tick(object? sender, EventArgs e)
    {
        if (_viewModel.Settings?.Dock?.AutoHide == AutoHideMode.Timeout)
        {
            if (!_isMouseOver && !_isDragging)
            {
                FadeOut();
            }
        }
    }

    private void Window_MouseEnter(object sender, MouseEventArgs e)
    {
        _isMouseOver = true;
        FadeIn();
    }

    private void Window_MouseLeave(object sender, MouseEventArgs e)
    {
        _isMouseOver = false;

        if (_viewModel.Settings?.Dock?.AutoHide == AutoHideMode.OnMouseLeave)
        {
            FadeOut();
        }
    }

    private void Window_Deactivated(object sender, EventArgs e)
    {
        if (_viewModel.Settings?.Dock?.AutoHide == AutoHideMode.EdgeHover)
        {
            FadeOut();
        }
    }

    private void FadeIn()
    {
        var animation = new DoubleAnimation
        {
            To = _viewModel.Settings?.Dock?.Opacity ?? 0.85,
            Duration = TimeSpan.FromMilliseconds(_viewModel.Settings?.Dock?.AnimationDuration ?? 200),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        DockBorder.BeginAnimation(OpacityProperty, animation);
    }

    private void FadeOut()
    {
        var animation = new DoubleAnimation
        {
            To = 0.0,
            Duration = TimeSpan.FromMilliseconds(_viewModel.Settings?.Dock?.AnimationDuration ?? 200),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
        };
        animation.Completed += (s, e) => Hide();
        DockBorder.BeginAnimation(OpacityProperty, animation);
    }

    private void DeviceItem_MouseEnter(object sender, MouseEventArgs e)
    {
        if (sender is Border border)
        {
            var animation = new DoubleAnimation
            {
                To = 1.05,
                Duration = TimeSpan.FromMilliseconds(100),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            border.RenderTransform = new ScaleTransform(1, 1);
            border.RenderTransformOrigin = new Point(0.5, 0.5);
            border.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, animation);
            border.RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, animation);
        }
    }

    private void DeviceItem_MouseLeave(object sender, MouseEventArgs e)
    {
        if (sender is Border border && border.RenderTransform is ScaleTransform transform)
        {
            var animation = new DoubleAnimation
            {
                To = 1.0,
                Duration = TimeSpan.FromMilliseconds(100),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            transform.BeginAnimation(ScaleTransform.ScaleXProperty, animation);
            transform.BeginAnimation(ScaleTransform.ScaleYProperty, animation);
        }
    }

    // Drag to reposition
    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);
        _dragStartPoint = e.GetPosition(this);
        _isDragging = true;
        CaptureMouse();
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if (_isDragging && e.LeftButton == MouseButtonState.Pressed)
        {
            var currentPos = e.GetPosition(this);
            var diff = currentPos - _dragStartPoint;
            Left += diff.X;
            Top += diff.Y;
        }
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonUp(e);
        if (_isDragging)
        {
            _isDragging = false;
            ReleaseMouseCapture();

            // Save new position as offset
            if (_viewModel.Settings?.Dock != null)
            {
                // Calculate offset from center/edge
                // This would update the settings
            }
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        _autoHideTimer?.Stop();
        _autoHideTimer = null;

        if (_viewModel.Settings != null)
        {
            _viewModel.Settings.Dock.PropertyChanged -= DockSettings_PropertyChanged;
        }

        base.OnClosed(e);
    }
}