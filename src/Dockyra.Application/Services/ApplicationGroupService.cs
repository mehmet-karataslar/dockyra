using Dockyra.Application.Interfaces;
using Dockyra.Contracts.Models;
using Dockyra.Contracts.Results;
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

    public ApplicationGroupService(
        IApplicationGroupRepository groupRepository,
        SmartGroupingRuleEngine ruleEngine,
        ILogger<ApplicationGroupService> logger)
    {
        _groupRepository = groupRepository;
        _ruleEngine = ruleEngine;
        _logger = logger;
    }

    public async Task<IReadOnlyList<ApplicationGroup>> GetAllGroupsAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<ApplicationGroup> groups = await _groupRepository.GetGroupsAsync(cancellationToken).ConfigureAwait(false);
        if (groups.Count > 0)
        {
            return groups;
        }

        IReadOnlyList<ApplicationGroup> defaults = CreateDefaultSeedGroups();
        foreach (ApplicationGroup group in defaults)
        {
            await _groupRepository.SaveGroupAsync(group, cancellationToken).ConfigureAwait(false);
        }

        return defaults;
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

