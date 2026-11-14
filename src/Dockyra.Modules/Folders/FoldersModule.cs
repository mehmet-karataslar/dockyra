using Dockyra.Contracts.Modules;
using Dockyra.Contracts.Results;

namespace Dockyra.Modules.Folders;

public sealed class FoldersModule : IModule
{
    public string Id => "MOD-FOLDERS";
    public string Name => "Folder Flyouts & Hierarchy";
    public bool IsEssential => false;

    public Task<OperationResult> InitializeAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(OperationResult.Success());

    public Task ShutdownAsync(CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}

