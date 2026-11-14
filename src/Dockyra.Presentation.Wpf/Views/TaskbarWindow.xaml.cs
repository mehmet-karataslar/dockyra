using System.Windows;
using Dockyra.Contracts.Models;
using Dockyra.Contracts.Services;
using Dockyra.Domain.Entities;
using Dockyra.Presentation.Wpf.ViewModels;

namespace Dockyra.Presentation.Wpf.Views;

public partial class TaskbarWindow : Window
{
    private readonly TaskbarViewModel _viewModel;
    private readonly IMonitorService _monitorService;
    private readonly Func<SettingsWindow> _settingsWindowFactory;

    public TaskbarWindow(
        TaskbarViewModel viewModel,
        IMonitorService monitorService,
        Func<SettingsWindow> settingsWindowFactory)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _monitorService = monitorService;
        _settingsWindowFactory = settingsWindowFactory;
        DataContext = _viewModel;

        Loaded += OnTaskbarWindowLoaded;
    }

    private async void OnTaskbarWindowLoaded(object sender, RoutedEventArgs e)
    {
        PositionOnPrimaryMonitor();
        await _viewModel.InitializeAsync();
    }

    public void PositionOnPrimaryMonitor()
    {
        MonitorInfo primary = _monitorService.GetPrimaryMonitor();
        double scale = primary.ScaleFactor > 0 ? primary.ScaleFactor : 1.0;

        double workWidth = primary.WorkArea.Width / scale;
        double workLeft = primary.WorkArea.Left / scale;
        double workBottom = primary.WorkArea.Bottom / scale;

        Width = Math.Min(1180, Math.Max(640, workWidth - 48));
        Left = workLeft + ((workWidth - Width) / 2.0);
        Top = workBottom - Height - 8;
    }

    private async void OnCycleProfileClick(object sender, RoutedEventArgs e)
    {
        if (_viewModel.Profiles.Count == 0)
        {
            return;
        }

        int currentIndex = _viewModel.ActiveProfile is not null
            ? _viewModel.Profiles.IndexOf(_viewModel.ActiveProfile)
            : 0;

        int nextIndex = (currentIndex + 1) % _viewModel.Profiles.Count;
        WorkProfile nextProfile = _viewModel.Profiles[nextIndex];
        await _viewModel.SwitchProfileAsync(nextProfile);
    }

    private void OnOpenSettingsClick(object sender, RoutedEventArgs e)
    {
        SettingsWindow settingsWindow = _settingsWindowFactory();
        settingsWindow.Owner = this;
        settingsWindow.ShowDialog();
    }
}

