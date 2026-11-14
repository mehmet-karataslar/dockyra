using Dockyra.Contracts.Modules;
using Dockyra.Contracts.Results;

namespace Dockyra.Modules.Windows;

public sealed class WindowsModule : IModule
{
    public string Id => "MOD-WINDOWS";
    public string Name => "Open Window Tracking & Management";
    public bool IsEssential => true;

    public Task<OperationResult> InitializeAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(OperationResult.Success());

    public Task ShutdownAsync(CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}

