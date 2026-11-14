using Dockyra.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Dockyra.Application.Services;

/// <summary>
/// Tracks consecutive ungraceful startups and triggers Safe Mode if 3 or more consecutive crashes occur.
/// </summary>
public sealed class SafeModeStartupCoordinator
{
    public const int SafeModeThreshold = 3;
    private readonly ISafeModeStateStore _stateStore;
    private readonly ILogger<SafeModeStartupCoordinator> _logger;

    public SafeModeStartupCoordinator(
        ISafeModeStateStore stateStore,
        ILogger<SafeModeStartupCoordinator> logger)
    {
        _stateStore = stateStore;
        _logger = logger;
    }

    public bool IsSafeModeActive { get; private set; }

    public bool EvaluateStartupShouldEnterSafeMode()
    {
        int failures = _stateStore.RecordStartupAttempt();
        IsSafeModeActive = failures >= SafeModeThreshold;

        if (IsSafeModeActive)
        {
            _logger.LogWarning(
                "Safe Mode activated after {ConsecutiveFailures} consecutive startup failures.",
                failures);
        }

        return IsSafeModeActive;
    }

    public void MarkStartupHealthy()
    {
        _stateStore.RecordStartupSuccess();
        IsSafeModeActive = false;
    }
}

