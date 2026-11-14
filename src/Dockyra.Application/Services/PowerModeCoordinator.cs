using Dockyra.Application.Interfaces;
using Dockyra.Contracts.Models;
using Dockyra.Contracts.Services;
using Microsoft.Extensions.Logging;

namespace Dockyra.Application.Services;

public sealed class PowerModeCoordinator : IPowerManagementService, IDisposable
{
    private readonly IPowerMonitor _powerMonitor;
    private readonly ILogger<PowerModeCoordinator> _logger;
    private PowerMode? _userOverride;
    private PowerMode _currentMode = PowerMode.Balanced;
    private bool _autoSaveOnBattery = true;

    public PowerModeCoordinator(
        IPowerMonitor powerMonitor,
        ILogger<PowerModeCoordinator> logger)
    {
        _powerMonitor = powerMonitor;
        _logger = logger;
        _powerMonitor.PowerStateChanged += OnPowerStateChanged;
        EvaluateMode(_powerMonitor.GetCurrentState());
    }

    public PowerMode CurrentMode => _currentMode;

    public TimeSpan RecommendedCoalesceDelay => _currentMode switch
    {
        PowerMode.PowerSaver => TimeSpan.FromMilliseconds(200),
        PowerMode.Balanced => TimeSpan.FromMilliseconds(80),
        PowerMode.Performance => TimeSpan.FromMilliseconds(50),
        _ => TimeSpan.FromMilliseconds(80)
    };

    public bool AreAnimationsAllowed => _currentMode != PowerMode.PowerSaver;

    public event EventHandler<PowerMode>? PowerModeChanged;

    public void SetUserOverrideMode(PowerMode? overrideMode)
    {
        _userOverride = overrideMode;
        EvaluateMode(_powerMonitor.GetCurrentState());
    }

    public void SetAutoPowerSaveOnBattery(bool enabled)
    {
        _autoSaveOnBattery = enabled;
        EvaluateMode(_powerMonitor.GetCurrentState());
    }

    private void OnPowerStateChanged(object? sender, PowerStateChangedEventArgs e) =>
        EvaluateMode(e.CurrentState);

    private void EvaluateMode(PowerState state)
    {
        PowerMode nextMode;
        if (_userOverride.HasValue)
        {
            nextMode = _userOverride.Value;
        }
        else if (_autoSaveOnBattery && (state.Source == PowerSourceKind.Battery || state.IsSystemEnergySaverActive))
        {
            nextMode = PowerMode.PowerSaver;
        }
        else
        {
            nextMode = PowerMode.Balanced;
        }

        if (nextMode != _currentMode)
        {
            _currentMode = nextMode;
            _logger.LogInformation("Power mode transitioned to {PowerMode}", _currentMode);
            PowerModeChanged?.Invoke(this, _currentMode);
        }
    }

    public void Dispose()
    {
        _powerMonitor.PowerStateChanged -= OnPowerStateChanged;
    }
}

