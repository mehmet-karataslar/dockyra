using Dockyra.Contracts.Modules;
using Dockyra.Contracts.Results;

namespace Dockyra.Modules.Groups;

public sealed class GroupsModule : IModule
{
    public string Id => "MOD-GROUPS";
    public string Name => "Application Groups";
    public bool IsEssential => false;

    public Task<OperationResult> InitializeAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(OperationResult.Success());

    public Task ShutdownAsync(CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}

