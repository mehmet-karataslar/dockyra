using Dockyra.Contracts.Modules;
using Dockyra.Contracts.Results;

namespace Dockyra.Modules.PowerManagement;

public sealed class PowerManagementModule : IModule
{
    public string Id => "MOD-POWER";
    public string Name => "Adaptive Power Management";
    public bool IsEssential => false;

    public Task<OperationResult> InitializeAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(OperationResult.Success());

    public Task ShutdownAsync(CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}

