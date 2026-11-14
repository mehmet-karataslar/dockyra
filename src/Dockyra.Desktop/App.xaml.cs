using System.Windows;
using System.Windows.Threading;
using Dockyra.Application.Services;
using Dockyra.Contracts.Modules;
using Dockyra.Contracts.Services;
using Dockyra.Desktop.Bootstrapper;
using Dockyra.Infrastructure.Persistence;
using Dockyra.Presentation.Wpf.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Dockyra.Desktop;

public partial class App : System.Windows.Application
{
    private const string SingleInstanceMutexName = @"Local\Dockyra_SingleInstance_Mutex_v1";
    private Mutex? _instanceMutex;
    private ServiceProvider? _serviceProvider;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _instanceMutex = new Mutex(true, SingleInstanceMutexName, out bool createdNew);
        if (!createdNew)
        {
            Shutdown(0);
            return;
        }

        DispatcherUnhandledException += OnDispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += OnDomainUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

        var services = new ServiceCollection();
        services.AddDockyraServices();
        _serviceProvider = services.BuildServiceProvider();

        var logger = _serviceProvider.GetRequiredService<ILogger<App>>();
        var safeModeCoordinator = _serviceProvider.GetRequiredService<SafeModeStartupCoordinator>();
        bool enterSafeMode = safeModeCoordinator.EvaluateStartupShouldEnterSafeMode();

        try
        {
            var migrator = _serviceProvider.GetRequiredService<DatabaseMigrator>();
            await migrator.MigrateAsync();

            var visibilityService = _serviceProvider.GetRequiredService<ITaskbarVisibilityService>();
            visibilityService.EnsureNativeShellAccessible();

            var moduleHost = _serviceProvider.GetRequiredService<IModuleHostService>();
            await moduleHost.InitializeModulesAsync(safeModeOnly: enterSafeMode);

            var mainWindow = _serviceProvider.GetRequiredService<TaskbarWindow>();
            MainWindow = mainWindow;
            mainWindow.Show();

            safeModeCoordinator.MarkStartupHealthy();
            logger.LogInformation("Dockyra started cleanly (SafeMode={SafeMode})", enterSafeMode);
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Fatal error during Dockyra startup");
            Shutdown(1);
        }
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (_serviceProvider is not null)
        {
            var moduleHost = _serviceProvider.GetService<IModuleHostService>();
            if (moduleHost is not null)
            {
                await moduleHost.ShutdownModulesAsync();
            }

            await _serviceProvider.DisposeAsync();
        }

        _instanceMutex?.Dispose();
        base.OnExit(e);
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        _serviceProvider?.GetService<ILogger<App>>()?.LogError(e.Exception, "Unhandled UI dispatcher exception");
        e.Handled = true;
    }

    private void OnDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
        {
            _serviceProvider?.GetService<ILogger<App>>()?.LogCritical(ex, "Unhandled AppDomain exception");
        }
    }

    private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        _serviceProvider?.GetService<ILogger<App>>()?.LogError(e.Exception, "Unobserved task exception");
        e.SetObserved();
    }
}

