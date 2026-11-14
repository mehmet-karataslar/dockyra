using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Dockyra.Contracts.Models;
using Dockyra.Contracts.Services;
using Dockyra.Domain.Entities;
using Dockyra.Domain.Enums;
using Dockyra.Presentation.Wpf.ViewModels;

namespace Dockyra.Presentation.Wpf.Views;

public partial class TaskbarWindow : Window
{
    private readonly TaskbarViewModel _viewModel;
    private readonly IMonitorService _monitorService;
    private readonly ITaskbarVisibilityService _taskbarVisibilityService;
    private readonly Func<SettingsWindow> _settingsWindowFactory;
    private readonly DispatcherTimer _clockAndSyncTimer;
    private readonly DispatcherTimer _fullscreenWatcherTimer;
    private bool _hiddenForFullscreenApp;
    private Point _dragStartPoint;

    public TaskbarWindow(
        TaskbarViewModel viewModel,
        IMonitorService monitorService,
        ITaskbarVisibilityService taskbarVisibilityService,
        Func<SettingsWindow> settingsWindowFactory)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _monitorService = monitorService;
        _taskbarVisibilityService = taskbarVisibilityService;
        _settingsWindowFactory = settingsWindowFactory;
        DataContext = _viewModel;

        _viewModel.ConfigurationUpdated += OnConfigurationUpdated;
        _viewModel.TaskbarVisibilityRequested += OnTaskbarVisibilityRequested;

        _clockAndSyncTimer = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = TimeSpan.FromSeconds(10)
        };
        _clockAndSyncTimer.Tick += OnClockAndSyncTimerTick;

        _fullscreenWatcherTimer = new DispatcherTimer(DispatcherPriority.Normal)
        {
            Interval = TimeSpan.FromMilliseconds(350)
        };
        _fullscreenWatcherTimer.Tick += OnFullscreenWatcherTimerTick;

        Loaded += OnTaskbarWindowLoaded;
        Closed += OnTaskbarWindowClosed;
    }

    private void OnTaskbarWindowLoaded(object sender, RoutedEventArgs e)
    {
        PositionOnPrimaryMonitor();
        _clockAndSyncTimer.Start();
        _fullscreenWatcherTimer.Start();
    }

    private void OnTaskbarWindowClosed(object? sender, EventArgs e)
    {
        _clockAndSyncTimer.Stop();
        _fullscreenWatcherTimer.Stop();
        _viewModel.ConfigurationUpdated -= OnConfigurationUpdated;
        _viewModel.TaskbarVisibilityRequested -= OnTaskbarVisibilityRequested;
        _taskbarVisibilityService.SetNativeTaskbarVisible(true);
    }

    private void OnFullscreenWatcherTimerTick(object? sender, EventArgs e)
    {
        if (!_viewModel.IsDockyraActive)
        {
            return;
        }

        bool isFullscreen = _taskbarVisibilityService.IsForegroundWindowFullscreen();
        if (isFullscreen)
        {
            if (IsVisible)
            {
                _viewModel.FolderPopup.IsOpen = false;
                _hiddenForFullscreenApp = true;
                Hide();
            }
        }
        else if (_hiddenForFullscreenApp)
        {
            _hiddenForFullscreenApp = false;
            if (!IsVisible)
            {
                Show();
                PositionOnPrimaryMonitor();
            }
        }
    }

    private void OnConfigurationUpdated(object? sender, EventArgs e)
    {
        Dispatcher.InvokeAsync(PositionOnPrimaryMonitor);
    }

    private void OnTaskbarVisibilityRequested(object? sender, bool shouldShow)
    {
        Dispatcher.InvokeAsync(() =>
        {
            if (shouldShow)
            {
                _hiddenForFullscreenApp = false;
                if (!IsVisible)
                {
                    Show();
                }
                PositionOnPrimaryMonitor();
                _fullscreenWatcherTimer.Start();
                Activate();
            }
            else
            {
                _hiddenForFullscreenApp = false;
                _fullscreenWatcherTimer.Stop();
                if (IsVisible)
                {
                    Hide();
                }
                OpenOrActivateStudioWindow();
            }
        });
    }

    private void OnClockAndSyncTimerTick(object? sender, EventArgs e)
    {
        _viewModel.UpdateClockText();
    }

    public void PositionOnPrimaryMonitor()
    {
        _ = _monitorService.GetPrimaryMonitor();
        DpiScale dpi = VisualTreeHelper.GetDpi(this);
        double scaleX = dpi.DpiScaleX > 0 ? dpi.DpiScaleX : 1.0;
        double scaleY = dpi.DpiScaleY > 0 ? dpi.DpiScaleY : 1.0;

        NativeTaskbarGeometry geometry = _taskbarVisibilityService.GetNativeTaskbarGeometry();
        double screenWidth = SystemParameters.PrimaryScreenWidth;
        double screenHeight = SystemParameters.PrimaryScreenHeight;

        double targetBarHeight = Math.Clamp(_viewModel.BarHeight, 40, 72);
        double radius = Math.Clamp(_viewModel.CornerRadius, 0, 24);
        double widthRatio = Math.Clamp(_viewModel.BarWidthPercent, 50, 100) / 100.0;

        if (_viewModel.IsFullReplacementMode)
        {
            SizeToContent = SizeToContent.Manual;
            MainTaskbarBorder.CornerRadius = new CornerRadius(radius);
            MainTaskbarBorder.Margin = (radius > 0 || widthRatio < 1.0) ? new Thickness(4, 2, 4, 2) : new Thickness(0);
            MainTaskbarBorder.BorderThickness = new Thickness(1);

            Height = targetBarHeight;
            Width = Math.Round(screenWidth * widthRatio);
            Left = _viewModel.Alignment == TaskbarAlignment.Left
                ? 0
                : Math.Round((screenWidth - Width) / 2.0);
            Top = _viewModel.DockEdge == TaskbarDockEdge.Top ? 0 : screenHeight - Height;
        }
        else
        {
            MainTaskbarBorder.CornerRadius = new CornerRadius(Math.Max(6, radius));
            MainTaskbarBorder.Margin = new Thickness(2);
            MainTaskbarBorder.BorderThickness = new Thickness(1);

            SizeToContent = SizeToContent.Width;
            UpdateLayout();

            Height = Math.Clamp(targetBarHeight, 42.0, 68.0);

            double taskbarLeftDip = geometry.IsNativeTaskbarFound
                ? geometry.TaskbarBounds.Left / scaleX
                : 0;
            double taskbarTopDip = geometry.IsNativeTaskbarFound
                ? (geometry.TaskbarBounds.Bottom / scaleY) - Height
                : screenHeight - Height;

            Left = taskbarLeftDip + 6;
            Top = taskbarTopDip;
        }
    }

    private void OnGroupPillButtonClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is ApplicationGroup group)
        {
            GroupFlyoutPopup.PlacementTarget = btn;
            _viewModel.OpenGroupFolder(group);
        }
    }

    private void OnGroupPillPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _dragStartPoint = e.GetPosition(null);
    }

    private void OnGroupPillPreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed)
        {
            return;
        }

        Point current = e.GetPosition(null);
        Vector diff = _dragStartPoint - current;
        if (Math.Abs(diff.X) <= SystemParameters.MinimumHorizontalDragDistance &&
            Math.Abs(diff.Y) <= SystemParameters.MinimumVerticalDragDistance)
        {
            return;
        }

        if (sender is FrameworkElement fe && fe.DataContext is ApplicationGroup group)
        {
            DragDrop.DoDragDrop(fe, new DataObject("Dockyra.TaskbarGroup", group), DragDropEffects.Move);
        }
    }

    private void OnGroupPillDragOver(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent("Dockyra.TaskbarGroup") ||
            e.Data.GetDataPresent("Dockyra.UnifiedApp") ||
            e.Data.GetDataPresent("Dockyra.PinnedApp") ||
            e.Data.GetDataPresent("Dockyra.OpenWindow"))
        {
            e.Effects = e.Data.GetDataPresent("Dockyra.TaskbarGroup") ? DragDropEffects.Move : DragDropEffects.Copy;
        }
        else
        {
            e.Effects = DragDropEffects.None;
        }
        e.Handled = true;
    }

    private async void OnGroupPillDrop(object sender, DragEventArgs e)
    {
        if (sender is not FrameworkElement targetFe || targetFe.DataContext is not ApplicationGroup targetGroup)
        {
            return;
        }

        if (e.Data.GetDataPresent("Dockyra.TaskbarGroup") &&
            e.Data.GetData("Dockyra.TaskbarGroup") is ApplicationGroup sourceGroup)
        {
            await _viewModel.MoveGroupAsync(sourceGroup.Id, targetGroup.Id);
            return;
        }

        if (e.Data.GetDataPresent("Dockyra.UnifiedApp") &&
            e.Data.GetData("Dockyra.UnifiedApp") is UnifiedTaskbarAppItem unifiedApp)
        {
            string? path = unifiedApp.LaunchPath ?? unifiedApp.PrimaryWindow?.ExecutablePath;
            string name = unifiedApp.PinnedSource?.DisplayName ?? unifiedApp.PrimaryWindow?.ProcessName ?? unifiedApp.DisplayName;
            if (!string.IsNullOrWhiteSpace(path))
            {
                GroupFlyoutPopup.PlacementTarget = targetFe;
                await _viewModel.DropApplicationOntoGroupAsync(targetGroup, name, path);
            }
            return;
        }

        if (e.Data.GetDataPresent("Dockyra.PinnedApp") &&
            e.Data.GetData("Dockyra.PinnedApp") is PinnedTaskbarItem pinnedApp)
        {
            GroupFlyoutPopup.PlacementTarget = targetFe;
            await _viewModel.DropApplicationOntoGroupAsync(targetGroup, pinnedApp.DisplayName, pinnedApp.ShortcutPath);
            return;
        }

        if (e.Data.GetDataPresent("Dockyra.OpenWindow") &&
            e.Data.GetData("Dockyra.OpenWindow") is WindowInfo openWindow &&
            !string.IsNullOrWhiteSpace(openWindow.ExecutablePath))
        {
            GroupFlyoutPopup.PlacementTarget = targetFe;
            await _viewModel.DropApplicationOntoGroupAsync(targetGroup, openWindow.ProcessName, openWindow.ExecutablePath);
        }
    }

    private void OnUnifiedAppPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _dragStartPoint = e.GetPosition(null);
    }

    private void OnUnifiedAppPreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed)
        {
            return;
        }

        Point current = e.GetPosition(null);
        Vector diff = _dragStartPoint - current;
        if (Math.Abs(diff.X) <= SystemParameters.MinimumHorizontalDragDistance &&
            Math.Abs(diff.Y) <= SystemParameters.MinimumVerticalDragDistance)
        {
            return;
        }

        if (sender is FrameworkElement fe && fe.DataContext is UnifiedTaskbarAppItem item)
        {
            DragDrop.DoDragDrop(fe, new DataObject("Dockyra.UnifiedApp", item), DragDropEffects.Copy);
        }
    }

    private void OnUnifiedAppDragOver(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent("Dockyra.UnifiedApp"))
        {
            e.Effects = DragDropEffects.Copy;
        }
        else
        {
            e.Effects = DragDropEffects.None;
        }
        e.Handled = true;
    }

    private async void OnUnifiedAppDrop(object sender, DragEventArgs e)
    {
        if (sender is not FrameworkElement targetFe ||
            targetFe.DataContext is not UnifiedTaskbarAppItem targetItem ||
            !e.Data.GetDataPresent("Dockyra.UnifiedApp") ||
            e.Data.GetData("Dockyra.UnifiedApp") is not UnifiedTaskbarAppItem draggedItem)
        {
            return;
        }

        if (string.Equals(draggedItem.Id, targetItem.Id, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        GroupFlyoutPopup.PlacementTarget = MainTaskbarBorder;
        await _viewModel.CreateFolderFromTwoAppsAsync(draggedItem, targetItem);
    }

    private void OnTaskbarBorderMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (!_viewModel.IsFullReplacementMode && e.ButtonState == MouseButtonState.Pressed)
        {
            DragMove();
        }
    }

    private void OnWindowsScrollViewerPreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (e.Delta != 0)
        {
            WindowsScrollViewer.ScrollToHorizontalOffset(WindowsScrollViewer.HorizontalOffset - (e.Delta / 2.0));
            e.Handled = true;
        }
    }

    private void OnOpenSettingsClick(object sender, RoutedEventArgs e)
    {
        OpenOrActivateStudioWindow();
    }

    private void OpenOrActivateStudioWindow()
    {
        SettingsWindow? existing = System.Windows.Application.Current?.Windows
            .OfType<SettingsWindow>()
            .FirstOrDefault();

        if (existing is not null)
        {
            if (!existing.IsVisible)
            {
                existing.Show();
            }
            if (existing.WindowState == WindowState.Minimized)
            {
                existing.WindowState = WindowState.Normal;
            }
            existing.Activate();
            return;
        }

        SettingsWindow studio = _settingsWindowFactory();
        studio.Closed += (_, _) =>
        {
            if (!_viewModel.IsDockyraActive || !IsVisible)
            {
                _taskbarVisibilityService.SetNativeTaskbarVisible(true);
                System.Windows.Application.Current?.Shutdown(0);
            }
        };
        studio.Show();
        studio.Activate();
    }
}
