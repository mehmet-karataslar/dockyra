using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dockyra.Application.Interfaces;
using Dockyra.Contracts.Models;
using Dockyra.Contracts.Results;
using Dockyra.Contracts.Services;
using Dockyra.Domain.Entities;
using Dockyra.Domain.Enums;

namespace Dockyra.Presentation.Wpf.ViewModels;

public sealed partial class TaskbarViewModel : ObservableObject, IDisposable
{
    private readonly ITaskbarService _taskbarService;
    private readonly IGroupService _groupService;
    private readonly IProfileService _profileService;
    private readonly IWindowManager _windowManager;
    private readonly IWindowSwitchUseCase _windowSwitchUseCase;
    private readonly IPowerManagementService _powerManagementService;
    private bool _disposed;

    public TaskbarViewModel(
        ITaskbarService taskbarService,
        IGroupService groupService,
        IProfileService profileService,
        IWindowManager windowManager,
        IWindowSwitchUseCase windowSwitchUseCase,
        IPowerManagementService powerManagementService,
        FolderPopupViewModel folderPopupViewModel)
    {
        _taskbarService = taskbarService;
        _groupService = groupService;
        _profileService = profileService;
        _windowManager = windowManager;
        _windowSwitchUseCase = windowSwitchUseCase;
        _powerManagementService = powerManagementService;
        FolderPopup = folderPopupViewModel;

        _windowManager.WindowsChanged += OnWindowsChanged;
        _powerManagementService.PowerModeChanged += OnPowerModeChanged;
        CurrentPowerMode = _powerManagementService.CurrentMode;
    }

    public FolderPopupViewModel FolderPopup { get; }

    public ObservableCollection<ApplicationGroup> Groups { get; } = [];
    public ObservableCollection<WindowInfo> OpenWindows { get; } = [];
    public ObservableCollection<WorkProfile> Profiles { get; } = [];

    [ObservableProperty]
    private WorkProfile? _activeProfile;

    [ObservableProperty]
    private PowerMode _currentPowerMode;

    [ObservableProperty]
    private TaskbarDockEdge _dockEdge = TaskbarDockEdge.Bottom;

    [ObservableProperty]
    private int _barHeight = 52;

    [ObservableProperty]
    private string _statusBanner = "Hazır";

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        TaskbarLayout layout = await _taskbarService.GetCurrentLayoutAsync(cancellationToken);
        DockEdge = layout.DockEdge;
        BarHeight = layout.BarHeight;

        IReadOnlyList<WorkProfile> profiles = await _profileService.GetProfilesAsync(cancellationToken);
        Profiles.Clear();
        foreach (WorkProfile profile in profiles)
        {
            Profiles.Add(profile);
        }

        ActiveProfile = await _profileService.GetActiveProfileAsync(cancellationToken);
        await ReloadGroupsForActiveProfileAsync(cancellationToken);
        await RefreshWindowsAsync(cancellationToken);
    }

    [RelayCommand]
    public async Task RefreshWindowsAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<WindowInfo> windows = await _windowManager.GetWindowsAsync(cancellationToken);
        ApplyWindowList(windows);
    }

    [RelayCommand]
    public void OpenGroupFolder(ApplicationGroup? group)
    {
        if (group is null)
        {
            return;
        }

        if (FolderPopup.IsOpen && string.Equals(FolderPopup.GroupTitle, group.Name, StringComparison.Ordinal))
        {
            FolderPopup.IsOpen = false;
            return;
        }

        FolderPopup.LoadGroup(group);
    }

    [RelayCommand]
    public async Task ActivateOrToggleWindowAsync(WindowInfo? window, CancellationToken cancellationToken = default)
    {
        if (window is null)
        {
            return;
        }

        OperationResult result = await _windowSwitchUseCase.SwitchOrToggleWindowAsync(window, cancellationToken);
        StatusBanner = result.IsSuccess
            ? $"{window.Title}"
            : result.Error?.Message ?? "Pencere odaklanamadı.";

        await RefreshWindowsAsync(cancellationToken);
    }

    [RelayCommand]
    public async Task SwitchProfileAsync(WorkProfile? profile, CancellationToken cancellationToken = default)
    {
        if (profile is null)
        {
            return;
        }

        OperationResult<WorkProfile> result = await _profileService.SwitchProfileAsync(profile.Id, cancellationToken);
        if (result.IsSuccess && result.Value is not null)
        {
            ActiveProfile = result.Value;
            await ReloadGroupsForActiveProfileAsync(cancellationToken);
            StatusBanner = $"Profil: {ActiveProfile.Name}";
        }
    }

    [RelayCommand]
    public void CyclePowerMode()
    {
        PowerMode next = CurrentPowerMode switch
        {
            PowerMode.PowerSaver => PowerMode.Balanced,
            PowerMode.Balanced => PowerMode.Performance,
            _ => PowerMode.PowerSaver
        };

        _powerManagementService.SetUserOverrideMode(next);
        CurrentPowerMode = _powerManagementService.CurrentMode;
        StatusBanner = $"Güç Modu: {CurrentPowerMode}";
    }

    private async Task ReloadGroupsForActiveProfileAsync(CancellationToken cancellationToken)
    {
        IReadOnlyList<ApplicationGroup> allGroups = await _groupService.GetAllGroupsAsync(cancellationToken);
        Groups.Clear();

        HashSet<string>? filterIds = ActiveProfile is { EnabledGroupIds.Count: > 0 }
            ? new HashSet<string>(ActiveProfile.EnabledGroupIds, StringComparer.Ordinal)
            : null;

        foreach (ApplicationGroup group in allGroups.OrderBy(g => g.SortOrder))
        {
            if (filterIds is null || filterIds.Contains(group.Id))
            {
                Groups.Add(group);
            }
        }
    }

    private void OnWindowsChanged(object? sender, WindowsChangedEventArgs e)
    {
        if (System.Windows.Application.Current?.Dispatcher is { } dispatcher && !dispatcher.CheckAccess())
        {
            _ = dispatcher.InvokeAsync(() => ApplyWindowList(e.Windows));
            return;
        }

        ApplyWindowList(e.Windows);
    }

    private void OnPowerModeChanged(object? sender, PowerMode mode) =>
        CurrentPowerMode = mode;

    private void ApplyWindowList(IReadOnlyList<WindowInfo> windows)
    {
        OpenWindows.Clear();
        foreach (WindowInfo win in windows)
        {
            OpenWindows.Add(win);
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _windowManager.WindowsChanged -= OnWindowsChanged;
        _powerManagementService.PowerModeChanged -= OnPowerModeChanged;
    }
}

