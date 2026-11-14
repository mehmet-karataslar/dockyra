using Dockyra.Contracts.Modules;
using Dockyra.Contracts.Results;

namespace Dockyra.Modules.Themes;

public sealed class ThemesModule : IModule
{
    public string Id => "MOD-THEMES";
    public string Name => "Fluent Design Tokens, Themes & Localization";
    public bool IsEssential => true;

    public Task<OperationResult> InitializeAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(OperationResult.Success());

    public Task ShutdownAsync(CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}

