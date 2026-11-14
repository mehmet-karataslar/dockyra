using Dockyra.Contracts.Modules;
using Dockyra.Contracts.Results;

namespace Dockyra.Modules.Taskbar;

public sealed class TaskbarModule : IModule
{
    public string Id => "MOD-TASKBAR";
    public string Name => "Taskbar Shell & Layout";
    public bool IsEssential => true;

    public Task<OperationResult> InitializeAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(OperationResult.Success());

    public Task ShutdownAsync(CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}

