using Dockyra.Contracts.Results;

namespace Dockyra.Contracts.Modules;

public interface IModule
{
    string Id { get; }
    string Name { get; }
    bool IsEssential { get; }
    Task<OperationResult> InitializeAsync(CancellationToken cancellationToken = default);
    Task ShutdownAsync(CancellationToken cancellationToken = default);
}

public sealed record ModuleStatusSnapshot(
    string Id,
    string Name,
    bool IsEssential,
    bool IsInitialized,
    bool IsFaulted,
    string? FaultReason = null);

public interface IModuleHostService
{
    IReadOnlyList<ModuleStatusSnapshot> GetModuleStatuses();
    Task<OperationResult> InitializeModulesAsync(bool safeModeOnly, CancellationToken cancellationToken = default);
    Task ShutdownModulesAsync(CancellationToken cancellationToken = default);
}

