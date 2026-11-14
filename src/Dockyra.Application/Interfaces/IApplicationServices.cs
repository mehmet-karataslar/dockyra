using Dockyra.Contracts.Models;
using Dockyra.Contracts.Results;
using Dockyra.Domain.Entities;
using Dockyra.Domain.Enums;

namespace Dockyra.Application.Interfaces;

public interface ITaskbarService
{
    Task<TaskbarLayout> GetCurrentLayoutAsync(CancellationToken cancellationToken = default);
    Task<OperationResult> SaveLayoutAsync(TaskbarLayout layout, CancellationToken cancellationToken = default);
    Task<OperationResult> UpdateDockEdgeAsync(TaskbarDockEdge edge, CancellationToken cancellationToken = default);
    Task<OperationResult> ResetToDefaultLayoutAsync(CancellationToken cancellationToken = default);
}

public interface IGroupService
{
    Task<IReadOnlyList<ApplicationGroup>> GetAllGroupsAsync(CancellationToken cancellationToken = default);
    Task<OperationResult<ApplicationGroup>> CreateGroupAsync(
        string name,
        string colorHex,
        string icon,
        CancellationToken cancellationToken = default);
    Task<OperationResult> SaveGroupAsync(
        ApplicationGroup group,
        CancellationToken cancellationToken = default);
    Task<OperationResult> DeleteGroupAsync(
        string groupId,
        CancellationToken cancellationToken = default);
    Task<OperationResult> ReorderGroupsAsync(
        IReadOnlyList<string> orderedGroupIds,
        CancellationToken cancellationToken = default);
    Task<OperationResult> AddApplicationToGroupAsync(
        string groupId,
        ApplicationEntry entry,
        CancellationToken cancellationToken = default);
    Task<OperationResult> RemoveApplicationFromGroupAsync(
        string groupId,
        string applicationId,
        CancellationToken cancellationToken = default);
    Task SyncNativePinnedAppsAsync(
        IReadOnlyList<PinnedTaskbarItem> pinnedItems,
        CancellationToken cancellationToken = default);
    string? SuggestGroupForWindow(WindowInfo window);
}

public interface IFolderService
{
    IReadOnlyList<ApplicationEntry> FilterItems(
        ApplicationGroup group,
        string? searchQuery);
    Task<OperationResult> SetViewModeAsync(
        string groupId,
        FolderViewMode viewMode,
        CancellationToken cancellationToken = default);
}

public interface IProfileService
{
    Task<IReadOnlyList<WorkProfile>> GetProfilesAsync(CancellationToken cancellationToken = default);
    Task<WorkProfile> GetActiveProfileAsync(CancellationToken cancellationToken = default);
    Task<OperationResult<WorkProfile>> SwitchProfileAsync(string profileId, CancellationToken cancellationToken = default);
}

public interface IWindowSwitchUseCase
{
    Task<OperationResult> SwitchOrToggleWindowAsync(
        WindowInfo window,
        CancellationToken cancellationToken = default);
}

public interface IPowerManagementService
{
    PowerMode CurrentMode { get; }
    TimeSpan RecommendedCoalesceDelay { get; }
    bool AreAnimationsAllowed { get; }
    event EventHandler<PowerMode>? PowerModeChanged;
    void SetUserOverrideMode(PowerMode? overrideMode);
}

public interface IEventCoalescer : IDisposable
{
    long CoalescedOutputCount { get; }
    void Signal();
}

