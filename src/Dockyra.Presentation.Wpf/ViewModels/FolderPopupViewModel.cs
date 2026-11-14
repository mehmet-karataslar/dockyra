using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dockyra.Application.Interfaces;
using Dockyra.Contracts.Models;
using Dockyra.Contracts.Services;
using Dockyra.Domain.Entities;
using Dockyra.Domain.Enums;

namespace Dockyra.Presentation.Wpf.ViewModels;

public sealed partial class FolderPopupViewModel : ObservableObject
{
    private readonly IFolderService _folderService;
    private readonly IGroupService? _groupService;
    private readonly IApplicationLauncher _launcher;
    private readonly IWindowManager? _windowManager;
    private ApplicationGroup? _activeGroup;

    public event EventHandler? GroupModified;

    public FolderPopupViewModel(
        IFolderService folderService,
        IApplicationLauncher launcher,
        IGroupService? groupService = null,
        IWindowManager? windowManager = null)
    {
        _folderService = folderService;
        _launcher = launcher;
        _groupService = groupService ?? (folderService as IGroupService);
        _windowManager = windowManager;
    }

    public ApplicationGroup? ActiveGroup => _activeGroup;

    [ObservableProperty]
    private string _groupTitle = string.Empty;

    partial void OnGroupTitleChanged(string value)
    {
        if (_activeGroup is not null && !string.IsNullOrWhiteSpace(value) && !string.Equals(_activeGroup.Name, value.Trim(), StringComparison.Ordinal))
        {
            _activeGroup.Name = value.Trim();
            if (_groupService is not null)
            {
                _ = _groupService.SaveGroupAsync(_activeGroup);
            }
        }
    }

    [ObservableProperty]
    private string _groupColorHex = "#3B82F6";

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private FolderViewMode _viewMode = FolderViewMode.Grid;

    [ObservableProperty]
    private bool _isOpen;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public ObservableCollection<ApplicationEntry> FilteredApplications { get; } = [];

    public void LoadGroup(ApplicationGroup group)
    {
        ArgumentNullException.ThrowIfNull(group);
        _activeGroup = group;
        GroupTitle = group.Name;
        GroupColorHex = group.ColorHex;
        ViewMode = group.ViewMode;
        SearchQuery = string.Empty;
        RefreshFilteredItems();
        IsOpen = true;
    }

    partial void OnSearchQueryChanged(string value) => RefreshFilteredItems();

    [RelayCommand]
    private async Task ToggleViewModeAsync(CancellationToken cancellationToken)
    {
        if (_activeGroup is null)
        {
            return;
        }

        ViewMode = ViewMode == FolderViewMode.Grid ? FolderViewMode.List : FolderViewMode.Grid;
        _activeGroup.ViewMode = ViewMode;
        await _folderService.SetViewModeAsync(_activeGroup.Id, ViewMode, cancellationToken);
    }

    [RelayCommand]
    private async Task LaunchApplicationAsync(ApplicationEntry? entry, CancellationToken cancellationToken)
    {
        if (entry is null)
        {
            return;
        }

        // If this app inside the folder box is already running, focus/restore its window!
        if (entry.IsRunning && entry.RunningWindowHandle is > 0 && _windowManager is not null)
        {
            var winId = new WindowId(entry.RunningWindowHandle.Value);
            await _windowManager.RestoreWindowAsync(winId, cancellationToken);
            await _windowManager.ActivateWindowAsync(winId, cancellationToken);
            IsOpen = false;
            return;
        }

        LaunchResult result = await _launcher.LaunchAsync(
            new ApplicationLaunchRequest(entry.ExecutablePath, entry.Arguments),
            cancellationToken);

        StatusMessage = result.IsSuccess
            ? $"{entry.DisplayName} başlatıldı."
            : result.ErrorMessage ?? "Uygulama başlatılamadı.";

        if (result.IsSuccess)
        {
            IsOpen = false;
        }
    }

    [RelayCommand]
    private async Task CloseFolderAppWindowAsync(ApplicationEntry? entry, CancellationToken cancellationToken)
    {
        if (entry?.RunningWindowHandle is > 0 && _windowManager is not null)
        {
            await _windowManager.CloseWindowAsync(new WindowId(entry.RunningWindowHandle.Value), cancellationToken);
            entry.IsRunning = false;
            entry.IsActive = false;
            entry.RunningWindowHandle = null;
            RefreshFilteredItems();
            GroupModified?.Invoke(this, EventArgs.Empty);
        }
    }

    [RelayCommand]
    private async Task EndFolderAppTaskAsync(ApplicationEntry? entry, CancellationToken cancellationToken)
    {
        if (entry?.RunningProcessId is > 0 && _windowManager is not null)
        {
            await _windowManager.TerminateProcessAsync(entry.RunningProcessId.Value, cancellationToken);
            entry.IsRunning = false;
            entry.IsActive = false;
            entry.RunningWindowHandle = null;
            entry.RunningProcessId = null;
            RefreshFilteredItems();
            GroupModified?.Invoke(this, EventArgs.Empty);
        }
    }

    [RelayCommand]
    private async Task RemoveApplicationFromFolderAsync(ApplicationEntry? entry, CancellationToken cancellationToken)
    {
        if (entry is null || _activeGroup is null || _groupService is null)
        {
            return;
        }

        await _groupService.RemoveApplicationFromGroupAsync(_activeGroup.Id, entry.Id, cancellationToken);
        _activeGroup.Applications.RemoveAll(a => string.Equals(a.Id, entry.Id, StringComparison.Ordinal));
        RefreshFilteredItems();
        StatusMessage = $"{entry.DisplayName} kutudan çıkarıldı.";

        if (_activeGroup.Applications.Count == 0)
        {
            await _groupService.DeleteGroupAsync(_activeGroup.Id, cancellationToken);
            IsOpen = false;
        }

        GroupModified?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private async Task DeleteCurrentFolderAsync(CancellationToken cancellationToken)
    {
        if (_activeGroup is null || _groupService is null)
        {
            return;
        }

        await _groupService.DeleteGroupAsync(_activeGroup.Id, cancellationToken);
        IsOpen = false;
        GroupModified?.Invoke(this, EventArgs.Empty);
    }

    private void RefreshFilteredItems()
    {
        FilteredApplications.Clear();
        if (_activeGroup is null)
        {
            return;
        }

        IReadOnlyList<ApplicationEntry> matches = _folderService.FilterItems(_activeGroup, SearchQuery);
        foreach (ApplicationEntry item in matches)
        {
            FilteredApplications.Add(item);
        }
    }
}

