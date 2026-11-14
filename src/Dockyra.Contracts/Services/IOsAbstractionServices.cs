using Dockyra.Contracts.Models;
using Dockyra.Contracts.Results;

namespace Dockyra.Contracts.Services;

public interface IWindowManager : IDisposable
{
    event EventHandler<WindowsChangedEventArgs>? WindowsChanged;

    Task<IReadOnlyList<WindowInfo>> GetWindowsAsync(CancellationToken cancellationToken = default);

    Task<OperationResult> ActivateWindowAsync(WindowId windowId, CancellationToken cancellationToken = default);

    Task<OperationResult> MinimizeWindowAsync(WindowId windowId, CancellationToken cancellationToken = default);

    Task<OperationResult> RestoreWindowAsync(WindowId windowId, CancellationToken cancellationToken = default);

    Task<OperationResult> CloseWindowAsync(WindowId windowId, CancellationToken cancellationToken = default);
}

public interface IMonitorService
{
    IReadOnlyList<MonitorInfo> GetMonitors();
    MonitorInfo GetPrimaryMonitor();
}

public interface IPowerMonitor : IDisposable
{
    PowerState GetCurrentState();
    event EventHandler<PowerStateChangedEventArgs>? PowerStateChanged;
}

public interface IApplicationLauncher
{
    Task<LaunchResult> LaunchAsync(ApplicationLaunchRequest request, CancellationToken cancellationToken = default);
}

public interface ITaskbarVisibilityService
{
    bool IsNativeTaskbarAccessible { get; }
    OperationResult EnsureNativeShellAccessible();
}

