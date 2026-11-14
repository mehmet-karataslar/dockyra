using Dockyra.Contracts.Modules;
using Dockyra.Contracts.Results;

namespace Dockyra.Modules.Performance;

public sealed class PerformanceModule : IModule
{
    public string Id => "MOD-PERFORMANCE";
    public string Name => "Performance & Event Coalescing";
    public bool IsEssential => false;

    public Task<OperationResult> InitializeAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(OperationResult.Success());

    public Task ShutdownAsync(CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}

