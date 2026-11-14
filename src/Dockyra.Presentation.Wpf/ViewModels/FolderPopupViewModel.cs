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
    private readonly IApplicationLauncher _launcher;
    private ApplicationGroup? _activeGroup;

    public FolderPopupViewModel(
        IFolderService folderService,
        IApplicationLauncher launcher)
    {
        _folderService = folderService;
        _launcher = launcher;
    }

    [ObservableProperty]
    private string _groupTitle = string.Empty;

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

