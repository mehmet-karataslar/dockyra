using Dockyra.Contracts.Results;
using Dockyra.Contracts.Services;
using Dockyra.WindowsIntegration.Native;

namespace Dockyra.WindowsIntegration.Shell;

/// <summary>
/// Ensures Dockyra coexists safely with the native Windows 11 shell without terminating explorer.exe or injecting DLLs.
/// </summary>
public sealed class TaskbarCoexistenceService : ITaskbarVisibilityService
{
    public bool IsNativeTaskbarAccessible => Win32NativeMethods.GetShellWindow() != IntPtr.Zero;

    public OperationResult EnsureNativeShellAccessible()
    {
        IntPtr shellWnd = Win32NativeMethods.GetShellWindow();
        if (shellWnd == IntPtr.Zero)
        {
            return OperationResult.Failure(
                "NATIVE_SHELL_NOT_DETECTED",
                "Windows Shell window was not detected; Dockyra will continue in standalone safe mode.",
                ErrorCategory.WindowsApi);
        }

        return OperationResult.Success();
    }
}

