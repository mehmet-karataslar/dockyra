using Dockyra.Application.Interfaces;
using Dockyra.Application.Services;
using Dockyra.Application.UseCases;
using Dockyra.Contracts.Models;
using Dockyra.Contracts.Results;
using Dockyra.Contracts.Services;
using Dockyra.Domain.Entities;
using Dockyra.Domain.Enums;
using Dockyra.Domain.Rules;
using Dockyra.Domain.ValueObjects;
using Dockyra.Presentation.Wpf.ViewModels;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Dockyra.UnitTests;

public sealed class DomainAndApplicationUnitTests
{
    [Fact]
    public void SmartGroupingRuleEngine_PrioritizesUserRulesOverBuiltInRules()
    {
        var engine = new SmartGroupingRuleEngine();
        var app = new AppIdentity("Code", @"C:\Program Files\Microsoft VS Code\Code.exe", "Dockyra - VS Code");

        // Built-in rule maps "Code" -> "grp-dev"
        Assert.Equal("grp-dev", engine.SuggestGroupId(app));

        // User rule overrides "Code" -> "grp-custom-work"
        var userRules = new[]
        {
            new ClassificationRule
            {
                Id = "user-1",
                Pattern = "Code",
                MatchField = RuleMatchField.ProcessName,
                TargetGroupId = "grp-custom-work",
                IsUserDefined = true,
                Priority = 1
            }
        };

        Assert.Equal("grp-custom-work", engine.SuggestGroupId(app, userRules));
    }

    [Fact]
    public void FolderHierarchyRules_RejectsDepthExceedingMaximumAndDetectsCycles()
    {
        var folders = new Dictionary<string, FolderItem>
        {
            ["f1"] = new FolderItem { Id = "f1", ParentFolderId = null },
            ["f2"] = new FolderItem { Id = "f2", ParentFolderId = "f1" },
            ["f3"] = new FolderItem { Id = "f3", ParentFolderId = "f2" }
        };

        // f2 is depth 2, so child under f2 has depth 3 (allowed)
        Assert.True(FolderHierarchyRules.CanCreateSubFolder(folders, "f2", out string? err1));
        Assert.Null(err1);

        // f3 is depth 3, so child under f3 would be depth 4 (rejected)
        Assert.False(FolderHierarchyRules.CanCreateSubFolder(folders, "f3", out string? err2));
        Assert.NotNull(err2);

        // Cycle detection
        folders["f1"].ParentFolderId = "f2";
        Assert.False(FolderHierarchyRules.CanCreateSubFolder(folders, "f2", out string? cycleErr));
        Assert.Contains("Cyclic", cycleErr);
    }

    [Fact]
    public void TaskbarLayoutValidator_EnforcesIconSizeAndBarHeightBounds()
    {
        TaskbarLayout valid = TaskbarLayout.CreateDefault();
        Assert.True(TaskbarLayoutValidator.Validate(valid, out IReadOnlyList<string> noErrors));
        Assert.Empty(noErrors);

        valid.IconSize = 12; // below MinIconSize (24)
        Assert.False(TaskbarLayoutValidator.Validate(valid, out IReadOnlyList<string> errors));
        Assert.NotEmpty(errors);
    }

    [Fact]
    public async Task WindowSwitchUseCase_RestoresMinimized_MinimizesActive_AndActivatesInactive()
    {
        var fakeWinMgr = new FakeWindowManager();
        var useCase = new WindowSwitchUseCase(fakeWinMgr);

        var minimizedWindow = new WindowInfo(new WindowId(101), "Terminal", "WindowsTerminal", null, 1000, false, WindowStateKind.Minimized);
        OperationResult r1 = await useCase.SwitchOrToggleWindowAsync(minimizedWindow);
        Assert.True(r1.IsSuccess);
        Assert.Contains(new WindowId(101), fakeWinMgr.RestoredWindows);
        Assert.Contains(new WindowId(101), fakeWinMgr.ActivatedWindows);

        var activeWindow = new WindowInfo(new WindowId(102), "Browser", "chrome", null, 1001, true, WindowStateKind.Normal);
        OperationResult r2 = await useCase.SwitchOrToggleWindowAsync(activeWindow);
        Assert.True(r2.IsSuccess);
        Assert.Contains(new WindowId(102), fakeWinMgr.MinimizedWindows);

        var inactiveWindow = new WindowInfo(new WindowId(103), "Editor", "Code", null, 1002, false, WindowStateKind.Normal);
        OperationResult r3 = await useCase.SwitchOrToggleWindowAsync(inactiveWindow);
        Assert.True(r3.IsSuccess);
        Assert.Contains(new WindowId(103), fakeWinMgr.ActivatedWindows);
    }

    [Fact]
    public void PowerModeCoordinator_AutomaticallyEntersPowerSaverOnBattery_AndRespectsUserOverride()
    {
        var fakePower = new FakePowerMonitor(new PowerState(PowerSourceKind.AcPower, 100, false));
        using var coordinator = new PowerModeCoordinator(fakePower, NullLogger<PowerModeCoordinator>.Instance);

        Assert.Equal(PowerMode.Balanced, coordinator.CurrentMode);
        Assert.True(coordinator.AreAnimationsAllowed);

        fakePower.SimulateStateChange(new PowerState(PowerSourceKind.Battery, 65, false));
        Assert.Equal(PowerMode.PowerSaver, coordinator.CurrentMode);
        Assert.False(coordinator.AreAnimationsAllowed);

        coordinator.SetUserOverrideMode(PowerMode.Performance);
        Assert.Equal(PowerMode.Performance, coordinator.CurrentMode);
        Assert.True(coordinator.AreAnimationsAllowed);
    }

    [Fact]
    public void SafeModeStartupCoordinator_ActivatesSafeModeAfterThreeFailures_AndResetsOnHealthyStartup()
    {
        var store = new InMemorySafeModeStateStore();
        var coordinator = new SafeModeStartupCoordinator(store, NullLogger<SafeModeStartupCoordinator>.Instance);

        Assert.False(coordinator.EvaluateStartupShouldEnterSafeMode()); // 1
        Assert.False(coordinator.EvaluateStartupShouldEnterSafeMode()); // 2
        Assert.True(coordinator.EvaluateStartupShouldEnterSafeMode());  // 3 -> Safe Mode!
        Assert.True(coordinator.IsSafeModeActive);

        coordinator.MarkStartupHealthy();
        Assert.Equal(0, store.GetConsecutiveFailures());
        Assert.False(coordinator.IsSafeModeActive);
    }

    [Fact]
    public async Task TaskbarViewModel_InitializesGroupsAndWindows_AndFiltersFolderPopup()
    {
        var layoutRepo = new InMemoryLayoutRepo();
        var groupRepo = new InMemoryGroupRepo();
        var profileRepo = new InMemoryProfileRepo();
        var settingsRepo = new InMemorySettingsRepo();
        var fakeWinMgr = new FakeWindowManager();
        var fakePower = new FakePowerMonitor(new PowerState(PowerSourceKind.AcPower, 100, false));

        var taskbarService = new TaskbarOrchestrationService(layoutRepo, NullLogger<TaskbarOrchestrationService>.Instance);
        var groupService = new ApplicationGroupService(groupRepo, new SmartGroupingRuleEngine(), NullLogger<ApplicationGroupService>.Instance);
        var profileService = new ProfileManagementService(profileRepo, settingsRepo, NullLogger<ProfileManagementService>.Instance);
        var switchUseCase = new WindowSwitchUseCase(fakeWinMgr);
        using var powerCoord = new PowerModeCoordinator(fakePower, NullLogger<PowerModeCoordinator>.Instance);
        var folderVm = new FolderPopupViewModel(groupService, new FakeLauncher());

        using var vm = new TaskbarViewModel(
            taskbarService,
            groupService,
            profileService,
            fakeWinMgr,
            switchUseCase,
            powerCoord,
            folderVm);

        await vm.InitializeAsync();

        Assert.NotEmpty(vm.Groups);
        Assert.NotEmpty(vm.Profiles);
        Assert.NotNull(vm.ActiveProfile);
        Assert.Single(vm.OpenWindows);

        ApplicationGroup devGroup = vm.Groups.First(g => g.Id == "grp-dev");
        vm.OpenGroupFolder(devGroup);
        Assert.True(vm.FolderPopup.IsOpen);
        Assert.Equal(2, vm.FolderPopup.FilteredApplications.Count);

        vm.FolderPopup.SearchQuery = "Notepad";
        Assert.Single(vm.FolderPopup.FilteredApplications);
    }

    private sealed class FakeWindowManager : IWindowManager
    {
        public List<WindowId> ActivatedWindows { get; } = [];
        public List<WindowId> MinimizedWindows { get; } = [];
        public List<WindowId> RestoredWindows { get; } = [];

        public event EventHandler<WindowsChangedEventArgs>? WindowsChanged
        {
            add { }
            remove { }
        }

        public Task<IReadOnlyList<WindowInfo>> GetWindowsAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<WindowInfo>>(
            [
                new WindowInfo(new WindowId(1), "Visual Studio Code", "Code", null, 42, true, WindowStateKind.Normal)
            ]);

        public Task<OperationResult> ActivateWindowAsync(WindowId windowId, CancellationToken cancellationToken = default)
        {
            ActivatedWindows.Add(windowId);
            return Task.FromResult(OperationResult.Success());
        }

        public Task<OperationResult> MinimizeWindowAsync(WindowId windowId, CancellationToken cancellationToken = default)
        {
            MinimizedWindows.Add(windowId);
            return Task.FromResult(OperationResult.Success());
        }

        public Task<OperationResult> RestoreWindowAsync(WindowId windowId, CancellationToken cancellationToken = default)
        {
            RestoredWindows.Add(windowId);
            return Task.FromResult(OperationResult.Success());
        }

        public Task<OperationResult> CloseWindowAsync(WindowId windowId, CancellationToken cancellationToken = default) =>
            Task.FromResult(OperationResult.Success());

        public void Dispose() { }
    }

    private sealed class FakePowerMonitor(PowerState initial) : IPowerMonitor
    {
        private PowerState _current = initial;
        public event EventHandler<PowerStateChangedEventArgs>? PowerStateChanged;
        public PowerState GetCurrentState() => _current;
        public void SimulateStateChange(PowerState next)
        {
            _current = next;
            PowerStateChanged?.Invoke(this, new PowerStateChangedEventArgs(next));
        }
        public void Dispose() { }
    }

    private sealed class FakeLauncher : IApplicationLauncher
    {
        public Task<LaunchResult> LaunchAsync(ApplicationLaunchRequest request, CancellationToken cancellationToken = default) =>
            Task.FromResult(LaunchResult.Succeeded(999));
    }

    private sealed class InMemorySafeModeStateStore : ISafeModeStateStore
    {
        private int _count;
        public int RecordStartupAttempt() => ++_count;
        public void RecordStartupSuccess() => _count = 0;
        public int GetConsecutiveFailures() => _count;
    }

    private sealed class InMemoryLayoutRepo : ITaskbarLayoutRepository
    {
        private TaskbarLayout _layout = TaskbarLayout.CreateDefault();
        public Task<TaskbarLayout> GetLayoutAsync(string layoutId, CancellationToken cancellationToken = default) => Task.FromResult(_layout);
        public Task<OperationResult> SaveLayoutAsync(TaskbarLayout layout, CancellationToken cancellationToken = default)
        {
            _layout = layout;
            return Task.FromResult(OperationResult.Success());
        }
    }

    private sealed class InMemoryGroupRepo : IApplicationGroupRepository
    {
        private readonly List<ApplicationGroup> _groups = [];
        public Task<IReadOnlyList<ApplicationGroup>> GetGroupsAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<ApplicationGroup>>(_groups.ToList());
        public Task<OperationResult> SaveGroupAsync(ApplicationGroup group, CancellationToken cancellationToken = default)
        {
            _groups.RemoveAll(g => g.Id == group.Id);
            _groups.Add(group);
            return Task.FromResult(OperationResult.Success());
        }
        public Task<OperationResult> DeleteGroupAsync(string groupId, CancellationToken cancellationToken = default)
        {
            _groups.RemoveAll(g => g.Id == groupId);
            return Task.FromResult(OperationResult.Success());
        }
    }

    private sealed class InMemoryProfileRepo : IProfileRepository
    {
        private readonly List<WorkProfile> _profiles = [];
        public Task<IReadOnlyList<WorkProfile>> GetProfilesAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<WorkProfile>>(_profiles.ToList());
        public Task<OperationResult> SaveProfileAsync(WorkProfile profile, CancellationToken cancellationToken = default)
        {
            _profiles.RemoveAll(p => p.Id == profile.Id);
            _profiles.Add(profile);
            return Task.FromResult(OperationResult.Success());
        }
    }

    private sealed class InMemorySettingsRepo : IUserSettingsRepository
    {
        private UserPreferences _prefs = new();
        public Task<UserPreferences> GetPreferencesAsync(CancellationToken cancellationToken = default) => Task.FromResult(_prefs);
        public Task<OperationResult> SavePreferencesAsync(UserPreferences preferences, CancellationToken cancellationToken = default)
        {
            _prefs = preferences;
            return Task.FromResult(OperationResult.Success());
        }
    }
}

