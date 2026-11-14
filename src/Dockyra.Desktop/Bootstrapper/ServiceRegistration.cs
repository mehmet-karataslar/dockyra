using Dockyra.Application.Interfaces;
using Dockyra.Application.Services;
using Dockyra.Application.UseCases;
using Dockyra.Contracts.Modules;
using Dockyra.Contracts.Services;
using Dockyra.Domain.Rules;
using Dockyra.Infrastructure.Backup;
using Dockyra.Infrastructure.Logging;
using Dockyra.Infrastructure.Persistence;
using Dockyra.Infrastructure.Recovery;
using Dockyra.Modules;
using Dockyra.Modules.Folders;
using Dockyra.Modules.Groups;
using Dockyra.Modules.Performance;
using Dockyra.Modules.PowerManagement;
using Dockyra.Modules.Profiles;
using Dockyra.Modules.Settings;
using Dockyra.Modules.Taskbar;
using Dockyra.Modules.Themes;
using Dockyra.Modules.Windows;
using Dockyra.Presentation.Wpf.DesignTokens;
using Dockyra.Presentation.Wpf.Localization;
using Dockyra.Presentation.Wpf.ViewModels;
using Dockyra.Presentation.Wpf.Views;
using Dockyra.WindowsIntegration.MonitorManagement;
using Dockyra.WindowsIntegration.PowerManagement;
using Dockyra.WindowsIntegration.ProcessManagement;
using Dockyra.WindowsIntegration.Shell;
using Dockyra.WindowsIntegration.WindowTracking;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Dockyra.Desktop.Bootstrapper;

public static class ServiceRegistration
{
    public static IServiceCollection AddDockyraServices(
        this IServiceCollection services,
        string? databasePath = null,
        string? safeModeFilePath = null)
    {
        // Logging
        services.AddSingleton<ILoggerFactory>(_ => LoggingConfiguration.CreateLoggerFactory());
        services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));

        // Domain
        services.AddSingleton<SmartGroupingRuleEngine>();

        // Infrastructure (SQLite, Backup, Safe Mode Store)
        services.AddSingleton(_ => new SqliteConnectionFactory(databasePath));
        services.AddSingleton<DatabaseMigrator>();
        services.AddSingleton<ITaskbarLayoutRepository, SqliteTaskbarLayoutRepository>();
        services.AddSingleton<IApplicationGroupRepository, SqliteApplicationGroupRepository>();
        services.AddSingleton<IProfileRepository, SqliteProfileRepository>();
        services.AddSingleton<IUserSettingsRepository, SqliteUserSettingsRepository>();
        services.AddSingleton<ISettingsBackupService, JsonSettingsBackupService>();
        services.AddSingleton<ISafeModeStateStore>(_ => new FileSafeModeStateStore(safeModeFilePath));

        // WindowsIntegration (Win32, DWM, Monitor, Power, Process Launch, Shell Coexistence, Icons)
        services.AddSingleton<IApplicationIconProvider, Win32IconExtractor>();
        services.AddSingleton<IWindowManager, Win32WindowManager>();
        services.AddSingleton<IMonitorService, Win32MonitorService>();
        services.AddSingleton<IPowerMonitor, Win32PowerMonitor>();
        services.AddSingleton<IApplicationLauncher, Win32ApplicationLauncher>();
        services.AddSingleton<ITaskbarVisibilityService, TaskbarCoexistenceService>();

        // Application Services & Use Cases
        services.AddSingleton<SafeModeStartupCoordinator>();
        services.AddSingleton<ITaskbarService, TaskbarOrchestrationService>();
        services.AddSingleton<ApplicationGroupService>();
        services.AddSingleton<IGroupService>(sp => sp.GetRequiredService<ApplicationGroupService>());
        services.AddSingleton<IFolderService>(sp => sp.GetRequiredService<ApplicationGroupService>());
        services.AddSingleton<IProfileService, ProfileManagementService>();
        services.AddSingleton<IWindowSwitchUseCase, WindowSwitchUseCase>();
        services.AddSingleton<IPowerManagementService, PowerModeCoordinator>();

        // Modules & Fault-Isolated Module Host
        services.AddSingleton<IModule, TaskbarModule>();
        services.AddSingleton<IModule, WindowsModule>();
        services.AddSingleton<IModule, ThemesModule>();
        services.AddSingleton<IModule, SettingsModule>();
        services.AddSingleton<IModule, GroupsModule>();
        services.AddSingleton<IModule, FoldersModule>();
        services.AddSingleton<IModule, ProfilesModule>();
        services.AddSingleton<IModule, PerformanceModule>();
        services.AddSingleton<IModule, PowerManagementModule>();
        services.AddSingleton<IModuleHostService, ModuleHostService>();

        // Presentation.Wpf (Themes, Localization, ViewModels, Views)
        services.AddSingleton<IThemeService, WpfThemeService>();
        services.AddSingleton<ILocalizationService, WpfLocalizationService>();
        services.AddTransient<FolderPopupViewModel>(sp => new FolderPopupViewModel(
            sp.GetRequiredService<IFolderService>(),
            sp.GetRequiredService<IApplicationLauncher>(),
            sp.GetRequiredService<IGroupService>(),
            sp.GetRequiredService<IWindowManager>()));
        services.AddSingleton<TaskbarViewModel>();
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<SettingsWindow>();
        services.AddSingleton<Func<SettingsWindow>>(sp => () => sp.GetRequiredService<SettingsWindow>());
        services.AddSingleton<TaskbarWindow>();

        return services;
    }
}

