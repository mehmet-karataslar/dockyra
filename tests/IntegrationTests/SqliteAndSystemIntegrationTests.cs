using Dockyra.Contracts.Models;
using Dockyra.Contracts.Modules;
using Dockyra.Contracts.Results;
using Dockyra.Domain.Entities;
using Dockyra.Domain.Enums;
using Dockyra.Infrastructure.Backup;
using Dockyra.Infrastructure.Persistence;
using Dockyra.Modules;
using Dockyra.Modules.Taskbar;
using Dockyra.WindowsIntegration.MonitorManagement;
using Dockyra.WindowsIntegration.PowerManagement;
using Dockyra.WindowsIntegration.ProcessManagement;
using Dockyra.WindowsIntegration.WindowTracking;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Dockyra.IntegrationTests;

public sealed class SqliteAndSystemIntegrationTests
{
    [Fact]
    public async Task SqliteRepositories_And_JsonBackupService_RoundTripFullConfiguration()
    {
        string tempDb = Path.Combine(Path.GetTempPath(), $"dockyra-test-{Guid.NewGuid():N}.db");
        try
        {
            var factory = new SqliteConnectionFactory(tempDb);
            var migrator = new DatabaseMigrator(factory, NullLogger<DatabaseMigrator>.Instance);
            await migrator.MigrateAsync();

            Assert.Equal(DatabaseMigrator.CurrentSchemaVersion, await migrator.GetAppliedVersionAsync());

            var layoutRepo = new SqliteTaskbarLayoutRepository(factory);
            var groupRepo = new SqliteApplicationGroupRepository(factory);
            var profileRepo = new SqliteProfileRepository(factory);
            var settingsRepo = new SqliteUserSettingsRepository(factory);

            var customGroup = new ApplicationGroup
            {
                Id = "grp-design",
                Name = "Tasarım",
                ColorHex = "#EC4899",
                SortOrder = 1,
                ViewMode = FolderViewMode.List,
                Applications =
                [
                    new ApplicationEntry { Id = "app-1", DisplayName = "Paint", ExecutablePath = @"C:\Windows\System32\mspaint.exe", SortOrder = 0 }
                ]
            };

            await groupRepo.SaveGroupAsync(customGroup);
            IReadOnlyList<ApplicationGroup> loadedGroups = await groupRepo.GetGroupsAsync();
            Assert.Single(loadedGroups);
            Assert.Equal("Tasarım", loadedGroups[0].Name);
            Assert.Single(loadedGroups[0].Applications);

            var backupService = new JsonSettingsBackupService(settingsRepo, layoutRepo, groupRepo, profileRepo);
            OperationResult<string> exportResult = await backupService.ExportToJsonAsync();
            Assert.True(exportResult.IsSuccess);
            Assert.NotNull(exportResult.Value);

            OperationResult<Dockyra.Application.Interfaces.SettingsBackupPackage> importResult =
                await backupService.ImportFromJsonAsync(exportResult.Value!);
            Assert.True(importResult.IsSuccess);
        }
        finally
        {
            if (File.Exists(tempDb))
            {
                try { File.Delete(tempDb); } catch { }
            }
        }
    }

    [Fact]
    public async Task ModuleHostService_IsolatesNonEssentialModuleFailure_WhileKeepingEssentialModulesActive()
    {
        IModule[] modules =
        [
            new TaskbarModule(),
            new FaultyOptionalModule()
        ];

        var host = new ModuleHostService(modules, NullLogger<ModuleHostService>.Instance);
        OperationResult result = await host.InitializeModulesAsync(safeModeOnly: false);

        Assert.True(result.IsSuccess);
        IReadOnlyList<ModuleStatusSnapshot> statuses = host.GetModuleStatuses();
        Assert.Contains(statuses, s => s.Id == "MOD-TASKBAR" && s.IsInitialized && !s.IsFaulted);
        Assert.Contains(statuses, s => s.Id == "MOD-OPTIONAL-FAULTY" && !s.IsInitialized && s.IsFaulted);
    }

    [Fact]
    public async Task WindowsIntegrationAdapters_EnumerateMonitorsPowerAndValidateProcessLaunching()
    {
        var monitorService = new Win32MonitorService();
        IReadOnlyList<MonitorInfo> monitors = monitorService.GetMonitors();
        Assert.NotEmpty(monitors);
        Assert.True(monitors[0].Bounds.Width > 0);

        using var powerMonitor = new Win32PowerMonitor();
        PowerState power = powerMonitor.GetCurrentState();
        Assert.InRange(power.BatteryLifePercent, 0, 100);

        var launcher = new Win32ApplicationLauncher(NullLogger<Win32ApplicationLauncher>.Instance);
        LaunchResult rejectedRelative = await launcher.LaunchAsync(new ApplicationLaunchRequest("relative\\app.exe"));
        Assert.False(rejectedRelative.IsSuccess);
        Assert.Equal("RELATIVE_PATH_REJECTED", rejectedRelative.ErrorCode);

        LaunchResult rejectedExt = await launcher.LaunchAsync(new ApplicationLaunchRequest(@"C:\Windows\System32\script.bat"));
        Assert.False(rejectedExt.IsSuccess);
        Assert.Equal("DISALLOWED_EXTENSION", rejectedExt.ErrorCode);

        using var winManager = new Win32WindowManager(NullLogger<Win32WindowManager>.Instance);
        IReadOnlyList<WindowInfo> windows = await winManager.GetWindowsAsync();
        Assert.NotNull(windows);
    }

    private sealed class FaultyOptionalModule : IModule
    {
        public string Id => "MOD-OPTIONAL-FAULTY";
        public string Name => "Faulty Optional Module";
        public bool IsEssential => false;

        public Task<OperationResult> InitializeAsync(CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("Simulated optional module failure.");

        public Task ShutdownAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}

