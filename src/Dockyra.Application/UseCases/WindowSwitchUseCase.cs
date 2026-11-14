using Dockyra.Application.Interfaces;
using Dockyra.Contracts.Models;
using Dockyra.Contracts.Results;
using Dockyra.Contracts.Services;

namespace Dockyra.Application.UseCases;

public sealed class WindowSwitchUseCase : IWindowSwitchUseCase
{
    private readonly IWindowManager _windowManager;

    public WindowSwitchUseCase(IWindowManager windowManager)
    {
        _windowManager = windowManager;
    }

    public async Task<OperationResult> SwitchOrToggleWindowAsync(
        WindowInfo window,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(window);

        if (!window.Id.IsValid)
        {
            return OperationResult.Failure(
                "INVALID_WINDOW_ID",
                "Cannot switch to an invalid window handle.",
                ErrorCategory.Validation);
        }

        if (window.State == WindowStateKind.Minimized)
        {
            OperationResult restoreResult = await _windowManager.RestoreWindowAsync(window.Id, cancellationToken).ConfigureAwait(false);
            if (!restoreResult.IsSuccess)
            {
                return restoreResult;
            }

            return await _windowManager.ActivateWindowAsync(window.Id, cancellationToken).ConfigureAwait(false);
        }

        if (window.IsActive)
        {
            return await _windowManager.MinimizeWindowAsync(window.Id, cancellationToken).ConfigureAwait(false);
        }

        return await _windowManager.ActivateWindowAsync(window.Id, cancellationToken).ConfigureAwait(false);
    }
}

