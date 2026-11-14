using Dockyra.Application.Interfaces;
using Dockyra.Contracts.Results;
using Dockyra.Domain.Entities;
using Dockyra.Domain.Enums;
using Dockyra.Domain.Rules;
using Microsoft.Extensions.Logging;

namespace Dockyra.Application.Services;

public sealed class TaskbarOrchestrationService : ITaskbarService
{
    private readonly ITaskbarLayoutRepository _layoutRepository;
    private readonly ILogger<TaskbarOrchestrationService> _logger;

    public TaskbarOrchestrationService(
        ITaskbarLayoutRepository layoutRepository,
        ILogger<TaskbarOrchestrationService> logger)
    {
        _layoutRepository = layoutRepository;
        _logger = logger;
    }

    public Task<TaskbarLayout> GetCurrentLayoutAsync(CancellationToken cancellationToken = default) =>
        _layoutRepository.GetLayoutAsync("default-layout", cancellationToken);

    public async Task<OperationResult> SaveLayoutAsync(
        TaskbarLayout layout,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(layout);
        if (!TaskbarLayoutValidator.Validate(layout, out IReadOnlyList<string> errors))
        {
            return OperationResult.Failure(
                "INVALID_LAYOUT",
                string.Join("; ", errors),
                ErrorCategory.Validation);
        }

        return await _layoutRepository.SaveLayoutAsync(layout, cancellationToken).ConfigureAwait(false);
    }

    public async Task<OperationResult> UpdateDockEdgeAsync(
        TaskbarDockEdge edge,
        CancellationToken cancellationToken = default)
    {
        TaskbarLayout layout = await GetCurrentLayoutAsync(cancellationToken).ConfigureAwait(false);
        if (layout.IsLocked)
        {
            return OperationResult.Failure(
                "TASKBAR_LOCKED",
                "Taskbar layout is currently locked.",
                ErrorCategory.Validation);
        }

        layout.DockEdge = edge;
        if (!TaskbarLayoutValidator.Validate(layout, out IReadOnlyList<string> errors))
        {
            return OperationResult.Failure(
                "INVALID_LAYOUT",
                string.Join("; ", errors),
                ErrorCategory.Validation);
        }

        _logger.LogInformation("Updating taskbar dock edge to {DockEdge}", edge);
        return await _layoutRepository.SaveLayoutAsync(layout, cancellationToken).ConfigureAwait(false);
    }

    public async Task<OperationResult> ResetToDefaultLayoutAsync(CancellationToken cancellationToken = default)
    {
        TaskbarLayout defaultLayout = TaskbarLayout.CreateDefault();
        _logger.LogInformation("Resetting taskbar layout to factory default");
        return await _layoutRepository.SaveLayoutAsync(defaultLayout, cancellationToken).ConfigureAwait(false);
    }
}

