using System.Runtime.InteropServices;
using Dockyra.Contracts.Models;
using Dockyra.Contracts.Services;
using Dockyra.WindowsIntegration.Native;

namespace Dockyra.WindowsIntegration.MonitorManagement;

public sealed class Win32MonitorService : IMonitorService
{
    private const uint MONITORINFOF_PRIMARY = 0x00000001;

    public IReadOnlyList<MonitorInfo> GetMonitors()
    {
        List<MonitorInfo> monitors = [];
        uint systemDpi = Win32NativeMethods.GetDpiForSystem();
        double scale = systemDpi > 0 ? Math.Round(systemDpi / 96.0, 2) : 1.0;

        Win32NativeMethods.EnumDisplayMonitors(
            IntPtr.Zero,
            IntPtr.Zero,
            (IntPtr hMonitor, IntPtr _, ref Win32NativeMethods.RECT _, IntPtr _) =>
            {
                var info = new Win32NativeMethods.MONITORINFOEXW
                {
                    cbSize = Marshal.SizeOf<Win32NativeMethods.MONITORINFOEXW>(),
                    szDevice = string.Empty
                };

                if (Win32NativeMethods.GetMonitorInfoW(hMonitor, ref info))
                {
                    var bounds = new WorkAreaRect(
                        info.rcMonitor.Left,
                        info.rcMonitor.Top,
                        info.rcMonitor.Right - info.rcMonitor.Left,
                        info.rcMonitor.Bottom - info.rcMonitor.Top);

                    var workArea = new WorkAreaRect(
                        info.rcWork.Left,
                        info.rcWork.Top,
                        info.rcWork.Right - info.rcWork.Left,
                        info.rcWork.Bottom - info.rcWork.Top);

                    bool isPrimary = (info.dwFlags & MONITORINFOF_PRIMARY) != 0;
                    string id = string.IsNullOrWhiteSpace(info.szDevice)
                        ? $"DISPLAY-{monitors.Count + 1}"
                        : info.szDevice.Trim();

                    monitors.Add(new MonitorInfo(
                        DeviceId: id,
                        FriendlyName: isPrimary ? $"{id} (Primary)" : id,
                        IsPrimary: isPrimary,
                        Bounds: bounds,
                        WorkArea: workArea,
                        ScaleFactor: scale));
                }

                return true;
            },
            IntPtr.Zero);

        if (monitors.Count == 0)
        {
            monitors.Add(new MonitorInfo(
                "DISPLAY-DEFAULT",
                "Primary Display",
                true,
                new WorkAreaRect(0, 0, 1920, 1080),
                new WorkAreaRect(0, 0, 1920, 1032),
                scale));
        }

        return monitors;
    }

    public MonitorInfo GetPrimaryMonitor()
    {
        IReadOnlyList<MonitorInfo> monitors = GetMonitors();
        return monitors.FirstOrDefault(m => m.IsPrimary) ?? monitors[0];
    }
}

