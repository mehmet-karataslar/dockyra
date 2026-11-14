using Dockyra.Contracts.Modules;
using Dockyra.Contracts.Results;
using Microsoft.Extensions.Logging;

namespace Dockyra.Modules;

/// <summary>
/// Fault-isolated module host. A failure in a non-essential module is logged and contained without crashing Dockyra.
/// </summary>
public sealed class ModuleHostService : IModuleHostService
{
    private readonly IReadOnlyList<IModule> _modules;
    private readonly ILogger<ModuleHostService> _logger;
    private readonly Dictionary<string, ModuleStatusSnapshot> _statuses = new(StringComparer.Ordinal);

    public ModuleHostService(
        IEnumerable<IModule> modules,
        ILogger<ModuleHostService> logger)
    {
        _modules = modules.ToList();
        _logger = logger;
        foreach (IModule module in _modules)
        {
            _statuses[module.Id] = new ModuleStatusSnapshot(
                module.Id,
                module.Name,
                module.IsEssential,
                IsInitialized: false,
                IsFaulted: false);
        }
    }

    public IReadOnlyList<ModuleStatusSnapshot> GetModuleStatuses() =>
        _statuses.Values.ToList();

    public async Task<OperationResult> InitializeModulesAsync(
        bool safeModeOnly,
        CancellationToken cancellationToken = default)
    {
        foreach (IModule module in _modules)
        {
            if (safeModeOnly && !module.IsEssential)
            {
                _logger.LogInformation("Skipping non-essential module {ModuleId} in Safe Mode", module.Id);
                continue;
            }

            try
            {
                OperationResult result = await module.InitializeAsync(cancellationToken).ConfigureAwait(false);
                if (result.IsSuccess)
                {
                    _statuses[module.Id] = new ModuleStatusSnapshot(
                        module.Id,
                        module.Name,
                        module.IsEssential,
                        IsInitialized: true,
                        IsFaulted: false);
                }
                else
                {
                    _statuses[module.Id] = new ModuleStatusSnapshot(
                        module.Id,
                        module.Name,
                        module.IsEssential,
                        IsInitialized: false,
                        IsFaulted: true,
                        FaultReason: result.Error?.Message);

                    if (module.IsEssential)
                    {
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Module {ModuleId} faulted during initialization", module.Id);
                _statuses[module.Id] = new ModuleStatusSnapshot(
                    module.Id,
                    module.Name,
                    module.IsEssential,
                    IsInitialized: false,
                    IsFaulted: true,
                    FaultReason: ex.Message);

                if (module.IsEssential)
                {
                    return OperationResult.Failure(
                        "ESSENTIAL_MODULE_FAULTED",
                        $"Essential module '{module.Id}' failed to initialize.",
                        ErrorCategory.Module,
                        ex.Message);
                }
            }
        }

        return OperationResult.Success();
    }

    public async Task ShutdownModulesAsync(CancellationToken cancellationToken = default)
    {
        foreach (IModule module in _modules.Reverse())
        {
            try
            {
                await module.ShutdownAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Module {ModuleId} threw an exception during shutdown", module.Id);
            }
        }
    }
}

