using Dockyra.Contracts.Modules;
using Dockyra.Contracts.Results;

namespace Dockyra.Modules.Profiles;

public sealed class ProfilesModule : IModule
{
    public string Id => "MOD-PROFILES";
    public string Name => "Work Profiles & Context Switching";
    public bool IsEssential => false;

    public Task<OperationResult> InitializeAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(OperationResult.Success());

    public Task ShutdownAsync(CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}

