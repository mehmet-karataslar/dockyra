using Dockyra.Application.Interfaces;
using Dockyra.Contracts.Models;
using Dockyra.Contracts.Results;
using Dockyra.Contracts.Services;
using Dockyra.Domain.Entities;
using Dockyra.Domain.Enums;
using Dockyra.Domain.Rules;
using Dockyra.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Dockyra.Application.Services;

public sealed class ApplicationGroupService : IGroupService, IFolderService
{
    private readonly IApplicationGroupRepository _groupRepository;
    private readonly SmartGroupingRuleEngine _ruleEngine;
    private readonly ILogger<ApplicationGroupService> _logger;
    private readonly IApplicationIconProvider? _iconProvider;

    public ApplicationGroupService(
        IApplicationGroupRepository groupRepository,
        SmartGroupingRuleEngine ruleEngine,
        ILogger<ApplicationGroupService> logger,
        IApplicationIconProvider? iconProvider = null)
    {
        _groupRepository = groupRepository;
        _ruleEngine = ruleEngine;
        _logger = logger;
        _iconProvider = iconProvider;
    }

    public async Task<IReadOnlyList<ApplicationGroup>> GetAllGroupsAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<ApplicationGroup> groups = await _groupRepository.GetGroupsAsync(cancellationToken).ConfigureAwait(false);
        if (groups.Count > 0)
        {
            PopulateGroupIcons(groups);
            return groups;
        }

        IReadOnlyList<ApplicationGroup> defaults = CreateDefaultSeedGroups();
        foreach (ApplicationGroup group in defaults)
        {
            await _groupRepository.SaveGroupAsync(group, cancellationToken).ConfigureAwait(false);
        }

        PopulateGroupIcons(defaults);
        return defaults;
    }

    private void PopulateGroupIcons(IEnumerable<ApplicationGroup> groups)
    {
        foreach (ApplicationGroup grp in groups)
        {
            grp.ColorHex = NormalizeToSoftMatteColor(grp.Id, grp.ColorHex);
            if (_iconProvider is null)
            {
                continue;
            }

            foreach (ApplicationEntry app in grp.Applications)
            {
                app.IconBgra32 ??= _iconProvider.GetIconBgra32(app.ExecutablePath);
            }
        }
    }

    private static string NormalizeToSoftMatteColor(string groupId, string currentHex)
    {
        return groupId switch
        {
            "grp-pinned" => "#5B7C99",
            "grp-dev" when currentHex is "#3B82F6" => "#4F7CAC",
            "grp-browsers" when currentHex is "#10B981" => "#5E8C76",
            "grp-comm" when currentHex is "#8B5CF6" => "#847996",
            "grp-system" when currentHex is "#F59E0B" => "#A68A71",
            _ => currentHex
        };
    }

    public async Task<OperationResult<ApplicationGroup>> CreateGroupAsync(
        string name,
        string colorHex,
        string icon,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return OperationResult<ApplicationGroup>.Failure(
                "GROUP_NAME_EMPTY",
                "Group name cannot be empty.",
                ErrorCategory.Validation);
        }

        IReadOnlyList<ApplicationGroup> existing = await _groupRepository.GetGroupsAsync(cancellationToken).ConfigureAwait(false);
        var group = new ApplicationGroup
        {
            Id = $"grp-{Guid.NewGuid():N}"[..12],
            Name = name.Trim(),
            ColorHex = new HexColor(colorHex).Value,
            Icon = new IconReference(icon).ToString(),
            SortOrder = existing.Count,
            ViewMode = FolderViewMode.Grid
        };

        OperationResult saveResult = await _groupRepository.SaveGroupAsync(group, cancellationToken).ConfigureAwait(false);
        if (!saveResult.IsSuccess)
        {
            return OperationResult<ApplicationGroup>.Failure(
                saveResult.Error?.Code ?? "GROUP_SAVE_FAILED",
                saveResult.Error?.Message ?? "Failed to save group.",
                ErrorCategory.Persistence);
        }

        _logger.LogInformation("Created application group {GroupId} ({GroupName})", group.Id, group.Name);
        return OperationResult<ApplicationGroup>.Success(group);
    }

    public Task<OperationResult> SaveGroupAsync(
        ApplicationGroup group,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(group);
        return _groupRepository.SaveGroupAsync(group, cancellationToken);
    }

    public Task<OperationResult> DeleteGroupAsync(
        string groupId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(groupId))
        {
            return Task.FromResult(OperationResult.Failure("GROUP_ID_EMPTY", "Group ID is required.", ErrorCategory.Validation));
        }

        return _groupRepository.DeleteGroupAsync(groupId, cancellationToken);
    }

    public async Task<OperationResult> ReorderGroupsAsync(
        IReadOnlyList<string> orderedGroupIds,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(orderedGroupIds);
        IReadOnlyList<ApplicationGroup> groups = await GetAllGroupsAsync(cancellationToken).ConfigureAwait(false);
        Dictionary<string, ApplicationGroup> map = groups.ToDictionary(g => g.Id, StringComparer.Ordinal);

        int order = 0;
        foreach (string id in orderedGroupIds)
        {
            if (map.TryGetValue(id, out ApplicationGroup? group))
            {
                group.SortOrder = order++;
                await _groupRepository.SaveGroupAsync(group, cancellationToken).ConfigureAwait(false);
            }
        }

        return OperationResult.Success();
    }

    public async Task<OperationResult> RemoveApplicationFromGroupAsync(
        string groupId,
        string applicationId,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<ApplicationGroup> groups = await GetAllGroupsAsync(cancellationToken).ConfigureAwait(false);
        ApplicationGroup? target = groups.FirstOrDefault(g => string.Equals(g.Id, groupId, StringComparison.Ordinal));
        if (target is null)
        {
            return OperationResult.Failure("GROUP_NOT_FOUND", $"Group '{groupId}' not found.", ErrorCategory.Validation);
        }

        target.Applications.RemoveAll(a => string.Equals(a.Id, applicationId, StringComparison.Ordinal));
        for (int i = 0; i < target.Applications.Count; i++)
        {
            target.Applications[i].SortOrder = i;
        }

        return await _groupRepository.SaveGroupAsync(target, cancellationToken).ConfigureAwait(false);
    }

    public async Task<OperationResult> AddApplicationToGroupAsync(
        string groupId,
        ApplicationEntry entry,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entry);
        if (string.IsNullOrWhiteSpace(entry.DisplayName) || string.IsNullOrWhiteSpace(entry.ExecutablePath))
        {
            return OperationResult.Failure(
                "INVALID_APP_ENTRY",
                "Application display name and executable path are required.",
                ErrorCategory.Validation);
        }

        IReadOnlyList<ApplicationGroup> groups = await GetAllGroupsAsync(cancellationToken).ConfigureAwait(false);
        ApplicationGroup? target = groups.FirstOrDefault(g => string.Equals(g.Id, groupId, StringComparison.Ordinal));
        if (target is null)
        {
            return OperationResult.Failure("GROUP_NOT_FOUND", $"Group '{groupId}' not found.", ErrorCategory.Validation);
        }

        if (target.Applications.Count >= FolderHierarchyRules.MaxItemsPerFolder)
        {
            return OperationResult.Failure("GROUP_FULL", "Group has reached maximum item capacity.", ErrorCategory.Validation);
        }

        entry.SortOrder = target.Applications.Count;
        target.Applications.Add(entry);
        return await _groupRepository.SaveGroupAsync(target, cancellationToken).ConfigureAwait(false);
    }

    public async Task SyncNativePinnedAppsAsync(
        IReadOnlyList<PinnedTaskbarItem> pinnedItems,
        CancellationToken cancellationToken = default)
    {
        if (pinnedItems is null || pinnedItems.Count == 0)
        {
            return;
        }

        List<ApplicationGroup> groups = (await GetAllGroupsAsync(cancellationToken).ConfigureAwait(false)).ToList();

        ApplicationGroup? pinnedGroup = groups.FirstOrDefault(g => string.Equals(g.Id, "grp-pinned", StringComparison.Ordinal));
        if (pinnedGroup is null)
        {
            pinnedGroup = new ApplicationGroup
            {
                Id = "grp-pinned",
                Name = "Sabitlenenler",
                Icon = "PinGlyph",
                ColorHex = "#5B7C99",
                Description = "Mevcut Windows 11 görev çubuğundaki sabitlenmiş uygulamalarınız",
                SortOrder = -1,
                ViewMode = FolderViewMode.Grid,
                Applications = []
            };
            groups.Insert(0, pinnedGroup);
        }

        pinnedGroup.Applications.Clear();
        int pinOrder = 0;
        foreach (PinnedTaskbarItem item in pinnedItems)
        {
            pinnedGroup.Applications.Add(new ApplicationEntry
            {
                Id = item.Id,
                DisplayName = item.DisplayName,
                ExecutablePath = item.ShortcutPath,
                SortOrder = pinOrder++
            });

            ApplicationGroup? categoryTarget = groups.FirstOrDefault(g => string.Equals(g.Id, item.CategoryGroupId, StringComparison.Ordinal));
            if (categoryTarget is not null &&
                !categoryTarget.Applications.Any(a =>
                    string.Equals(a.DisplayName, item.DisplayName, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(a.ExecutablePath, item.ShortcutPath, StringComparison.OrdinalIgnoreCase)))
            {
                categoryTarget.Applications.Add(new ApplicationEntry
                {
                    Id = $"{item.Id}-{categoryTarget.Id}",
                    DisplayName = item.DisplayName,
                    ExecutablePath = item.ShortcutPath,
                    SortOrder = categoryTarget.Applications.Count
                });
                await _groupRepository.SaveGroupAsync(categoryTarget, cancellationToken).ConfigureAwait(false);
            }
        }

        await _groupRepository.SaveGroupAsync(pinnedGroup, cancellationToken).ConfigureAwait(false);
    }

    public string? SuggestGroupForWindow(WindowInfo window)
    {
        ArgumentNullException.ThrowIfNull(window);
        return _ruleEngine.SuggestGroupId(new AppIdentity(window.ProcessName, window.ExecutablePath, window.Title));
    }

    public IReadOnlyList<ApplicationEntry> FilterItems(ApplicationGroup group, string? searchQuery)
    {
        ArgumentNullException.ThrowIfNull(group);
        if (string.IsNullOrWhiteSpace(searchQuery))
        {
            return group.Applications.OrderBy(a => a.SortOrder).ToList();
        }

        string trimmed = searchQuery.Trim();
        return group.Applications
            .Where(a =>
                a.DisplayName.Contains(trimmed, StringComparison.OrdinalIgnoreCase) ||
                a.ExecutablePath.Contains(trimmed, StringComparison.OrdinalIgnoreCase))
            .OrderBy(a => a.SortOrder)
            .ToList();
    }

    public async Task<OperationResult> SetViewModeAsync(
        string groupId,
        FolderViewMode viewMode,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<ApplicationGroup> groups = await GetAllGroupsAsync(cancellationToken).ConfigureAwait(false);
        ApplicationGroup? target = groups.FirstOrDefault(g => string.Equals(g.Id, groupId, StringComparison.Ordinal));
        if (target is null)
        {
            return OperationResult.Failure("GROUP_NOT_FOUND", $"Group '{groupId}' not found.", ErrorCategory.Validation);
        }

        target.ViewMode = viewMode;
        return await _groupRepository.SaveGroupAsync(target, cancellationToken).ConfigureAwait(false);
    }

    private static IReadOnlyList<ApplicationGroup> CreateDefaultSeedGroups() =>
    [
        new ApplicationGroup
        {
            Id = "grp-dev",
            Name = "Yazılım",
            Icon = "CodeGlyph",
            ColorHex = "#3B82F6",
            Description = "IDE, terminal ve geliştirme araçları",
            SortOrder = 0,
            ViewMode = FolderViewMode.Grid,
            Applications =
            [
                new ApplicationEntry { Id = "app-notepad", DisplayName = "Notepad", ExecutablePath = @"C:\Windows\System32\notepad.exe", SortOrder = 0 },
                new ApplicationEntry { Id = "app-cmd", DisplayName = "Command Prompt", ExecutablePath = @"C:\Windows\System32\cmd.exe", SortOrder = 1 }
            ]
        },
        new ApplicationGroup
        {
            Id = "grp-browsers",
            Name = "Tarayıcılar",
            Icon = "GlobeGlyph",
            ColorHex = "#10B981",
            Description = "Web tarayıcıları ve araştırma araçları",
            SortOrder = 1,
            ViewMode = FolderViewMode.Grid,
            Applications = []
        },
        new ApplicationGroup
        {
            Id = "grp-comm",
            Name = "İletişim",
            Icon = "ChatGlyph",
            ColorHex = "#8B5CF6",
            Description = "Mesajlaşma ve toplantı uygulamaları",
            SortOrder = 2,
            ViewMode = FolderViewMode.Grid,
            Applications = []
        },
        new ApplicationGroup
        {
            Id = "grp-system",
            Name = "Sistem Araçları",
            Icon = "SettingsGlyph",
            ColorHex = "#F59E0B",
            Description = "Dosya gezgini ve sistem yardımcı araçları",
            SortOrder = 3,
            ViewMode = FolderViewMode.List,
            Applications =
            [
                new ApplicationEntry { Id = "app-explorer", DisplayName = "Windows Explorer", ExecutablePath = @"C:\Windows\explorer.exe", SortOrder = 0 }
            ]
        }
    ];
}

