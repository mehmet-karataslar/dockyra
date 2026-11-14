using System.Diagnostics;
using System.Text;
using Dockyra.Application.Services;
using Dockyra.Contracts.Models;
using Dockyra.Contracts.Results;
using Dockyra.Contracts.Services;
using Dockyra.WindowsIntegration.Native;
using Microsoft.Extensions.Logging;

namespace Dockyra.WindowsIntegration.WindowTracking;

public sealed class Win32WindowManager : IWindowManager
{
    private readonly ILogger<Win32WindowManager> _logger;
    private readonly EventCoalescer _coalescer;
    private readonly int _currentProcessId;
    private Win32NativeMethods.WinEventDelegate? _winEventDelegate;
    private IntPtr _foregroundHook;
    private IntPtr _objectHook;
    private bool _disposed;

    public Win32WindowManager(ILogger<Win32WindowManager> logger)
    {
        _logger = logger;
        _currentProcessId = Environment.ProcessId;
        _coalescer = new EventCoalescer(OnCoalescedWindowEvent, TimeSpan.FromMilliseconds(80));
        TryInstallHooks();
    }

    public event EventHandler<WindowsChangedEventArgs>? WindowsChanged;

    public Task<IReadOnlyList<WindowInfo>> GetWindowsAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IReadOnlyList<WindowInfo> list = EnumerateVisibleUserWindows();
        return Task.FromResult(list);
    }

    public Task<OperationResult> ActivateWindowAsync(WindowId windowId, CancellationToken cancellationToken = default)
    {
        if (!TryValidateWindow(windowId, out IntPtr hWnd, out OperationResult failure))
        {
            return Task.FromResult(failure);
        }

        if (Win32NativeMethods.IsIconic(hWnd))
        {
            Win32NativeMethods.ShowWindow(hWnd, Win32NativeMethods.SW_RESTORE);
        }

        bool activated = Win32NativeMethods.SetForegroundWindow(hWnd);
        return Task.FromResult(activated
            ? OperationResult.Success()
            : OperationResult.Failure(
                "WIN32_FOREGROUND_DENIED",
                "Windows prevented changing foreground window focus.",
                ErrorCategory.WindowsApi));
    }

    public Task<OperationResult> MinimizeWindowAsync(WindowId windowId, CancellationToken cancellationToken = default)
    {
        if (!TryValidateWindow(windowId, out IntPtr hWnd, out OperationResult failure))
        {
            return Task.FromResult(failure);
        }

        Win32NativeMethods.ShowWindow(hWnd, Win32NativeMethods.SW_MINIMIZE);
        return Task.FromResult(OperationResult.Success());
    }

    public Task<OperationResult> RestoreWindowAsync(WindowId windowId, CancellationToken cancellationToken = default)
    {
        if (!TryValidateWindow(windowId, out IntPtr hWnd, out OperationResult failure))
        {
            return Task.FromResult(failure);
        }

        Win32NativeMethods.ShowWindow(hWnd, Win32NativeMethods.SW_RESTORE);
        return Task.FromResult(OperationResult.Success());
    }

    public Task<OperationResult> CloseWindowAsync(WindowId windowId, CancellationToken cancellationToken = default)
    {
        if (!TryValidateWindow(windowId, out IntPtr hWnd, out OperationResult failure))
        {
            return Task.FromResult(failure);
        }

        bool posted = Win32NativeMethods.PostMessageW(hWnd, Win32NativeMethods.WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
        return Task.FromResult(posted
            ? OperationResult.Success()
            : OperationResult.Failure("WIN32_CLOSE_FAILED", "Could not post WM_CLOSE message.", ErrorCategory.WindowsApi));
    }

    public Task<OperationResult> TerminateProcessAsync(int processId, CancellationToken cancellationToken = default)
    {
        if (processId <= 4 || processId == _currentProcessId)
        {
            return Task.FromResult(OperationResult.Failure(
                "PROCESS_PROTECTED",
                "Cannot terminate system or current Dockyra process.",
                ErrorCategory.Validation));
        }

        try
        {
            using var proc = Process.GetProcessById(processId);
            string procName = proc.ProcessName ?? string.Empty;
            if (string.Equals(procName, "explorer", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(procName, "csrss", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(procName, "winlogon", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(procName, "dwm", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(procName, "svchost", StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(OperationResult.Failure(
                    "SHELL_PROCESS_PROTECTED",
                    $"Protected Windows process '{procName}' cannot be terminated.",
                    ErrorCategory.Validation));
            }

            proc.Kill(entireProcessTree: false);
            _coalescer.Signal();
            return Task.FromResult(OperationResult.Success());
        }
        catch (Exception ex)
        {
            return Task.FromResult(OperationResult.Failure(
                "PROCESS_TERMINATE_FAILED",
                ex.Message,
                ErrorCategory.WindowsApi));
        }
    }

    private void TryInstallHooks()
    {
        try
        {
            _winEventDelegate = OnWinEventProc;
            uint flags = Win32NativeMethods.WINEVENT_OUTOFCONTEXT | Win32NativeMethods.WINEVENT_SKIPOWNPROCESS;
            _foregroundHook = Win32NativeMethods.SetWinEventHook(
                Win32NativeMethods.EVENT_SYSTEM_FOREGROUND,
                Win32NativeMethods.EVENT_SYSTEM_FOREGROUND,
                IntPtr.Zero,
                _winEventDelegate,
                0,
                0,
                flags);

            _objectHook = Win32NativeMethods.SetWinEventHook(
                Win32NativeMethods.EVENT_OBJECT_CREATE,
                Win32NativeMethods.EVENT_OBJECT_NAMECHANGE,
                IntPtr.Zero,
                _winEventDelegate,
                0,
                0,
                flags);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not install WinEventHook; manual window refresh remains operational.");
        }
    }

    private void OnWinEventProc(
        IntPtr hWinEventHook,
        uint eventType,
        IntPtr hwnd,
        int idObject,
        int idChild,
        uint dwEventThread,
        uint dwmsEventTime)
    {
        if (idObject != 0 || hwnd == IntPtr.Zero)
        {
            return;
        }

        _coalescer.Signal();
    }

    private void OnCoalescedWindowEvent()
    {
        if (_disposed || WindowsChanged is null)
        {
            return;
        }

        try
        {
            IReadOnlyList<WindowInfo> windows = EnumerateVisibleUserWindows();
            WindowId? activeId = windows.FirstOrDefault(w => w.IsActive)?.Id;
            WindowsChanged?.Invoke(this, new WindowsChangedEventArgs(windows, activeId));
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Ignored transient error during coalesced window enumeration.");
        }
    }

    private IReadOnlyList<WindowInfo> EnumerateVisibleUserWindows()
    {
        List<WindowInfo> results = [];
        IntPtr shellWindow = Win32NativeMethods.GetShellWindow();
        IntPtr foregroundWindow = Win32NativeMethods.GetForegroundWindow();

        Win32NativeMethods.EnumWindows((hWnd, _) =>
        {
            if (hWnd == IntPtr.Zero || hWnd == shellWindow)
            {
                return true;
            }

            if (!Win32NativeMethods.IsWindowVisible(hWnd))
            {
                return true;
            }

            long exStyle = Win32NativeMethods.GetWindowLongPtr(hWnd, Win32NativeMethods.GWL_EXSTYLE).ToInt64();
            bool isToolWindow = (exStyle & Win32NativeMethods.WS_EX_TOOLWINDOW) != 0;
            bool isAppWindow = (exStyle & Win32NativeMethods.WS_EX_APPWINDOW) != 0;
            if (isToolWindow && !isAppWindow)
            {
                return true;
            }

            if (IsWindowCloaked(hWnd))
            {
                return true;
            }

            int length = Win32NativeMethods.GetWindowTextLengthW(hWnd);
            if (length <= 0)
            {
                return true;
            }

            var sb = new StringBuilder(length + 1);
            Win32NativeMethods.GetWindowTextW(hWnd, sb, sb.Capacity);
            string title = sb.ToString().Trim();
            if (string.IsNullOrWhiteSpace(title))
            {
                return true;
            }

            Win32NativeMethods.GetWindowThreadProcessId(hWnd, out uint pid);
            if (pid == 0 || pid == _currentProcessId)
            {
                return true;
            }

            string processName = "unknown";
            string? executablePath = TryGetProcessExecutablePath(pid);
            try
            {
                using var proc = Process.GetProcessById((int)pid);
                processName = proc.ProcessName;
            }
            catch
            {
                // Process may have exited or restricted access (UIPI).
            }

            byte[]? iconBytes = Shell.Win32IconExtractor.ExtractWindowOrExecutableIconBgra32(hWnd, executablePath);

            WindowStateKind state = Win32NativeMethods.IsIconic(hWnd)
                ? WindowStateKind.Minimized
                : Win32NativeMethods.IsZoomed(hWnd)
                    ? WindowStateKind.Maximized
                    : WindowStateKind.Normal;

            results.Add(new WindowInfo(
                new WindowId((ulong)hWnd.ToInt64()),
                title,
                processName,
                executablePath,
                (int)pid,
                IsActive: hWnd == foregroundWindow,
                State: state,
                IconBgra32: iconBytes));

            return true;
        }, IntPtr.Zero);

        return results;
    }

    private static string? TryGetProcessExecutablePath(uint pid)
    {
        IntPtr hProcess = Win32NativeMethods.OpenProcess(Win32NativeMethods.PROCESS_QUERY_LIMITED_INFORMATION, false, pid);
        if (hProcess == IntPtr.Zero)
        {
            return null;
        }

        try
        {
            uint capacity = 1024;
            var sb = new StringBuilder((int)capacity);
            if (Win32NativeMethods.QueryFullProcessImageNameW(hProcess, 0, sb, ref capacity) && capacity > 0)
            {
                return sb.ToString();
            }
        }
        catch
        {
            // Ignore access errors
        }
        finally
        {
            Win32NativeMethods.CloseHandle(hProcess);
        }

        return null;
    }

    private static bool IsWindowCloaked(IntPtr hWnd)
    {
        int hr = Win32NativeMethods.DwmGetWindowAttribute(
            hWnd,
            Win32NativeMethods.DWMWA_CLOAKED,
            out int cloaked,
            sizeof(int));

        return hr == 0 && cloaked != 0;
    }

    private static bool TryValidateWindow(WindowId windowId, out IntPtr hWnd, out OperationResult failure)
    {
        if (!windowId.IsValid)
        {
            hWnd = IntPtr.Zero;
            failure = OperationResult.Failure("INVALID_WINDOW_ID", "WindowId cannot be zero.", ErrorCategory.Validation);
            return false;
        }

        hWnd = new IntPtr((long)windowId.Value);
        if (!Win32NativeMethods.IsWindow(hWnd))
        {
            failure = OperationResult.Failure("WINDOW_NOT_FOUND", "Target window no longer exists.", ErrorCategory.WindowsApi);
            return false;
        }

        failure = OperationResult.Success();
        return true;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _coalescer.Dispose();

        if (_foregroundHook != IntPtr.Zero)
        {
            Win32NativeMethods.UnhookWinEvent(_foregroundHook);
            _foregroundHook = IntPtr.Zero;
        }

        if (_objectHook != IntPtr.Zero)
        {
            Win32NativeMethods.UnhookWinEvent(_objectHook);
            _objectHook = IntPtr.Zero;
        }
    }
}

