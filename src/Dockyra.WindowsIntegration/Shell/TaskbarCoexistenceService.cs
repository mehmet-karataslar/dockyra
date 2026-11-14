using Dockyra.Contracts.Models;
using Dockyra.Contracts.Results;
using Dockyra.Contracts.Services;
using Dockyra.WindowsIntegration.Native;

namespace Dockyra.WindowsIntegration.Shell;

/// <summary>
/// Manages safe integration and reversible coexistence with the native Windows 11 taskbar (Shell_TrayWnd)
/// without terminating explorer.exe or injecting DLLs.
/// </summary>
public sealed class TaskbarCoexistenceService : ITaskbarVisibilityService
{
    private const string PrimaryTrayClassName = "Shell_TrayWnd";
    private const string SecondaryTrayClassName = "Shell_SecondaryTrayWnd";
    private const string TrayNotifyClassName = "TrayNotifyWnd";

    private readonly IMonitorService? _monitorService;
    private bool _hiddenByDockyra;
    private bool _disposed;

    public TaskbarCoexistenceService(IMonitorService? monitorService = null)
    {
        _monitorService = monitorService;
    }

    public bool IsNativeTaskbarAccessible =>
        Win32NativeMethods.FindWindowW(PrimaryTrayClassName, null) != IntPtr.Zero ||
        Win32NativeMethods.GetShellWindow() != IntPtr.Zero;

    public bool IsNativeTaskbarHiddenByDockyra => _hiddenByDockyra;

    public OperationResult EnsureNativeShellAccessible()
    {
        IntPtr trayWnd = Win32NativeMethods.FindWindowW(PrimaryTrayClassName, null);
        if (trayWnd != IntPtr.Zero && !Win32NativeMethods.IsWindowVisible(trayWnd))
        {
            // Recover native taskbar visibility if a previous session exited while in FullReplacement mode.
            Win32NativeMethods.ShowWindow(trayWnd, Win32NativeMethods.SW_SHOW);
            _hiddenByDockyra = false;
        }

        IntPtr shellWnd = Win32NativeMethods.GetShellWindow();
        if (shellWnd == IntPtr.Zero && trayWnd == IntPtr.Zero)
        {
            return OperationResult.Failure(
                "NATIVE_SHELL_NOT_DETECTED",
                "Windows Shell window was not detected; Dockyra will continue in standalone safe mode.",
                ErrorCategory.WindowsApi);
        }

        return OperationResult.Success();
    }

    public OperationResult SetNativeTaskbarVisible(bool visible)
    {
        IntPtr trayWnd = Win32NativeMethods.FindWindowW(PrimaryTrayClassName, null);
        if (trayWnd == IntPtr.Zero)
        {
            _hiddenByDockyra = false;
            return OperationResult.Failure(
                "NATIVE_TASKBAR_NOT_FOUND",
                "Native Windows taskbar (Shell_TrayWnd) could not be found.",
                ErrorCategory.WindowsApi);
        }

        int cmd = visible ? Win32NativeMethods.SW_SHOW : Win32NativeMethods.SW_HIDE;
        Win32NativeMethods.ShowWindow(trayWnd, cmd);

        IntPtr secondaryWnd = Win32NativeMethods.FindWindowW(SecondaryTrayClassName, null);
        if (secondaryWnd != IntPtr.Zero)
        {
            Win32NativeMethods.ShowWindow(secondaryWnd, cmd);
        }

        _hiddenByDockyra = !visible;
        return OperationResult.Success();
    }

    public NativeTaskbarGeometry GetNativeTaskbarGeometry()
    {
        MonitorInfo primary = _monitorService?.GetPrimaryMonitor()
            ?? new MonitorInfo(
                "PRIMARY",
                "Primary Display",
                true,
                new WorkAreaRect(0, 0, 1920, 1080),
                new WorkAreaRect(0, 0, 1920, 1032),
                1.0);

        IntPtr trayWnd = Win32NativeMethods.FindWindowW(PrimaryTrayClassName, null);
        double scale = primary.ScaleFactor > 0 ? primary.ScaleFactor : 1.0;

        if (trayWnd != IntPtr.Zero)
        {
            uint dpi = Win32NativeMethods.GetDpiForWindow(trayWnd);
            if (dpi >= 96)
            {
                scale = dpi / 96.0;
            }
        }

        int defaultBarHeightPx = (int)Math.Round(48 * scale);
        WorkAreaRect fallbackTaskbar = new(
            primary.Bounds.Left,
            primary.Bounds.Bottom - defaultBarHeightPx,
            primary.Bounds.Width,
            defaultBarHeightPx);

        if (trayWnd == IntPtr.Zero ||
            !Win32NativeMethods.GetWindowRect(trayWnd, out Win32NativeMethods.RECT rawTray) ||
            rawTray.Right <= rawTray.Left ||
            rawTray.Bottom <= rawTray.Top)
        {
            int fallbackTrayWidth = (int)Math.Round(220 * scale);
            WorkAreaRect fallbackSysTray = new(
                fallbackTaskbar.Right - fallbackTrayWidth,
                fallbackTaskbar.Top,
                fallbackTrayWidth,
                fallbackTaskbar.Height);
            WorkAreaRect fallbackOverlay = new(
                fallbackTaskbar.Left + (int)Math.Round(8 * scale),
                fallbackTaskbar.Top,
                Math.Max(400, fallbackSysTray.Left - fallbackTaskbar.Left - (int)Math.Round(16 * scale)),
                fallbackTaskbar.Height);

            return new NativeTaskbarGeometry(
                IsNativeTaskbarFound: false,
                IsNativeTaskbarVisible: false,
                TaskbarBounds: fallbackTaskbar,
                SystemTrayBounds: fallbackSysTray,
                OverlayDockBounds: fallbackOverlay,
                ScaleFactor: scale);
        }

        WorkAreaRect taskbarBounds = new(
            rawTray.Left,
            rawTray.Top,
            rawTray.Right - rawTray.Left,
            rawTray.Bottom - rawTray.Top);

        WorkAreaRect sysTrayBounds;
        IntPtr notifyWnd = Win32NativeMethods.FindWindowExW(trayWnd, IntPtr.Zero, TrayNotifyClassName, null);
        if (notifyWnd != IntPtr.Zero &&
            Win32NativeMethods.GetWindowRect(notifyWnd, out Win32NativeMethods.RECT rawNotify) &&
            rawNotify.Right > rawNotify.Left)
        {
            sysTrayBounds = new WorkAreaRect(
                rawNotify.Left,
                rawNotify.Top,
                rawNotify.Right - rawNotify.Left,
                rawNotify.Bottom - rawNotify.Top);
        }
        else
        {
            int estimatedWidth = (int)Math.Round(220 * scale);
            sysTrayBounds = new WorkAreaRect(
                taskbarBounds.Right - estimatedWidth,
                taskbarBounds.Top,
                estimatedWidth,
                taskbarBounds.Height);
        }

        int leftMargin = (int)Math.Round(6 * scale);
        int rightGap = (int)Math.Round(12 * scale);
        int availableWidth = Math.Max(360, sysTrayBounds.Left - (taskbarBounds.Left + leftMargin) - rightGap);

        WorkAreaRect overlayBounds = new(
            taskbarBounds.Left + leftMargin,
            taskbarBounds.Top,
            availableWidth,
            taskbarBounds.Height);

        bool isVisible = Win32NativeMethods.IsWindowVisible(trayWnd);
        return new NativeTaskbarGeometry(
            IsNativeTaskbarFound: true,
            IsNativeTaskbarVisible: isVisible,
            TaskbarBounds: taskbarBounds,
            SystemTrayBounds: sysTrayBounds,
            OverlayDockBounds: overlayBounds,
            ScaleFactor: scale);
    }

    public IReadOnlyList<PinnedTaskbarItem> GetNativePinnedTaskbarItems()
    {
        List<PinnedTaskbarItem> results = [];
        try
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string pinnedDir = Path.Combine(
                appData,
                "Microsoft",
                "Internet Explorer",
                "Quick Launch",
                "User Pinned",
                "TaskBar");

            if (!Directory.Exists(pinnedDir))
            {
                return results;
            }

            foreach (string filePath in Directory.EnumerateFiles(pinnedDir, "*.lnk").OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
            {
                string displayName = Path.GetFileNameWithoutExtension(filePath).Trim();
                if (string.IsNullOrWhiteSpace(displayName))
                {
                    continue;
                }

                string slug = new string(displayName
                    .ToLowerInvariant()
                    .Select(ch => char.IsLetterOrDigit(ch) ? ch : '-')
                    .ToArray()).Trim('-');

                string categoryGroupId = ClassifyShortcutByName(displayName);
                string? targetExePath = Win32IconExtractor.ResolveShortcutTargetPath(filePath);
                string iconSourcePath = !string.IsNullOrWhiteSpace(targetExePath) ? targetExePath : filePath;
                byte[]? iconBytes = Win32IconExtractor.ExtractNativeBgra32(iconSourcePath);
                results.Add(new PinnedTaskbarItem(
                    Id: $"pin-{slug}",
                    DisplayName: displayName,
                    ShortcutPath: filePath,
                    CategoryGroupId: categoryGroupId,
                    IconBgra32: iconBytes));
            }
        }
        catch
        {
            // Return partial or empty list safely if folder access is restricted.
        }

        return results;
    }

    public OperationResult OpenNativeStartMenu()
    {
        IntPtr targetWnd = Win32NativeMethods.FindWindowW(PrimaryTrayClassName, null);
        if (targetWnd == IntPtr.Zero)
        {
            targetWnd = Win32NativeMethods.GetShellWindow();
        }

        if (targetWnd == IntPtr.Zero)
        {
            return OperationResult.Failure(
                "START_MENU_TARGET_MISSING",
                "Could not locate Windows shell window to open Start Menu.",
                ErrorCategory.WindowsApi);
        }

        bool posted = Win32NativeMethods.PostMessageW(
            targetWnd,
            Win32NativeMethods.WM_SYSCOMMAND,
            new IntPtr(Win32NativeMethods.SC_TASKLIST),
            IntPtr.Zero);

        return posted
            ? OperationResult.Success()
            : OperationResult.Failure("START_MENU_POST_FAILED", "Failed to trigger Windows Start Menu.", ErrorCategory.WindowsApi);
    }

    public OperationResult OpenSystemQuickAction(string actionKey)
    {
        string targetUri = (actionKey ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "wifi" or "network" => "ms-availablenetworks:",
            "bluetooth" => "ms-settings:bluetooth",
            "battery" or "power" => "ms-settings:powersleep",
            "sound" or "audio" => "ms-settings:sound",
            "clock" or "date" => "ms-settings:dateandtime",
            _ => "ms-settings:"
        };

        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = targetUri,
                UseShellExecute = true
            });
            return OperationResult.Success();
        }
        catch (Exception ex)
        {
            return OperationResult.Failure("QUICK_ACTION_FAILED", ex.Message, ErrorCategory.WindowsApi);
        }
    }

    public bool IsForegroundWindowFullscreen()
    {
        IntPtr fgWnd = Win32NativeMethods.GetForegroundWindow();
        if (fgWnd == IntPtr.Zero || !Win32NativeMethods.IsWindowVisible(fgWnd) || Win32NativeMethods.IsIconic(fgWnd))
        {
            return false;
        }

        if (fgWnd == Win32NativeMethods.GetShellWindow())
        {
            return false;
        }

        Win32NativeMethods.GetWindowThreadProcessId(fgWnd, out uint pid);
        if (pid == 0 || pid == (uint)Environment.ProcessId)
        {
            return false;
        }

        var classBuilder = new System.Text.StringBuilder(128);
        int classLen = Win32NativeMethods.GetClassNameW(fgWnd, classBuilder, classBuilder.Capacity);
        if (classLen > 0)
        {
            string className = classBuilder.ToString();
            if (string.Equals(className, "Progman", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(className, "WorkerW", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(className, PrimaryTrayClassName, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(className, SecondaryTrayClassName, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        if (!Win32NativeMethods.GetWindowRect(fgWnd, out Win32NativeMethods.RECT winRect))
        {
            return false;
        }

        int winWidth = winRect.Right - winRect.Left;
        int winHeight = winRect.Bottom - winRect.Top;
        if (winWidth <= 0 || winHeight <= 0)
        {
            return false;
        }

        IReadOnlyList<MonitorInfo> monitors = _monitorService?.GetMonitors() ?? [];
        if (monitors.Count == 0 && _monitorService is not null)
        {
            monitors = [_monitorService.GetPrimaryMonitor()];
        }

        foreach (MonitorInfo monitor in monitors)
        {
            WorkAreaRect b = monitor.Bounds;
            if (b.Width <= 0 || b.Height <= 0)
            {
                continue;
            }

            // True fullscreen windows (YouTube fullscreen, F11, media players, borderless fullscreen)
            // match the exact monitor bounds (0,0 to Width,Height), whereas normal maximized windows
            // have an 8px invisible frame offset (-8,-8) or stop at WorkArea.
            bool exactMonitorMatch =
                Math.Abs(winRect.Left - b.Left) <= 1 &&
                Math.Abs(winRect.Top - b.Top) <= 1 &&
                Math.Abs(winRect.Right - b.Right) <= 1 &&
                Math.Abs(winRect.Bottom - b.Bottom) <= 1;

            if (exactMonitorMatch)
            {
                return true;
            }
        }

        // Also check Windows Shell notification state for D3D/fullscreen video presentation modes (QUNS_RUNNING_D3D_FULL_SCREEN = 3, QUNS_PRESENTATION_MODE = 4)
        if (Win32NativeMethods.SHQueryUserNotificationState(out int quns) == 0 && (quns == 3 || quns == 4))
        {
            return true;
        }

        return false;
    }

    private static string ClassifyShortcutByName(string displayName)
    {
        string lower = displayName.ToLowerInvariant();
        if (lower.Contains("studio") ||
            lower.Contains("code") ||
            lower.Contains("antigravity") ||
            lower.Contains("powershell") ||
            lower.Contains("terminal") ||
            lower.Contains("git") ||
            lower.Contains("docker") ||
            lower.Contains("rider") ||
            lower.Contains("intellij"))
        {
            return "grp-dev";
        }

        if (lower.Contains("chrome") && !lower.Contains("uzaktan") ||
            lower.Contains("brave") ||
            lower.Contains("edge") ||
            lower.Contains("firefox") ||
            lower.Contains("opera") ||
            lower.Contains("qwen") ||
            lower.Contains("arc"))
        {
            return "grp-browsers";
        }

        if (lower.Contains("discord") ||
            lower.Contains("slack") ||
            lower.Contains("teams") ||
            lower.Contains("whatsapp") ||
            lower.Contains("telegram") ||
            lower.Contains("music") ||
            lower.Contains("spotify") ||
            lower.Contains("zoom"))
        {
            return "grp-comm";
        }

        return "grp-system";
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        if (_hiddenByDockyra)
        {
            SetNativeTaskbarVisible(true);
        }
    }
}
