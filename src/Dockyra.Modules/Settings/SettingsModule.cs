using Dockyra.Contracts.Modules;
using Dockyra.Contracts.Results;

namespace Dockyra.Modules.Settings;

public sealed class SettingsModule : IModule
{
    public string Id => "MOD-SETTINGS";
    public string Name => "Settings, SQLite Persistence & Backup";
    public bool IsEssential => true;

    public Task<OperationResult> InitializeAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(OperationResult.Success());

    public Task ShutdownAsync(CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}

