using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Text;
using Dockyra.Contracts.Services;
using Dockyra.WindowsIntegration.Native;

namespace Dockyra.WindowsIntegration.Shell;

/// <summary>
/// Extracts clean 32x32 32-bit BGRA application icons directly from executables (.exe) and live windows
/// without the Windows shortcut arrow overlay, caching results in memory.
/// </summary>
public sealed class Win32IconExtractor : IApplicationIconProvider
{
    private const int IconDimension = 32;
    private const int ByteCount = IconDimension * IconDimension * 4;
    private static readonly ConcurrentDictionary<string, byte[]?> Cache = new(StringComparer.OrdinalIgnoreCase);

    public byte[]? GetIconBgra32(string? fileOrShortcutPath)
    {
        if (string.IsNullOrWhiteSpace(fileOrShortcutPath))
        {
            return null;
        }

        string expanded = Environment.ExpandEnvironmentVariables(fileOrShortcutPath.Trim());
        if (Cache.TryGetValue(expanded, out byte[]? cached))
        {
            return cached;
        }

        byte[]? extracted = ExtractNativeBgra32(expanded);
        if (extracted is not null)
        {
            Cache[expanded] = extracted;
        }

        return extracted;
    }

    /// <summary>
    /// Resolves a Windows shell shortcut (.lnk) to its underlying target executable (.exe) path
    /// so icons can be extracted directly from the application binary without a shortcut overlay arrow.
    /// </summary>
    internal static string? ResolveShortcutTargetPath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        string expanded = Environment.ExpandEnvironmentVariables(path.Trim());
        if (!expanded.EndsWith(".lnk", StringComparison.OrdinalIgnoreCase) || !File.Exists(expanded))
        {
            return File.Exists(expanded) ? expanded : null;
        }

        // 1. Try Windows Script Host Shell Link COM resolution
        try
        {
            Type? shellType = Type.GetTypeFromProgID("WScript.Shell");
            if (shellType is not null)
            {
                object? shell = Activator.CreateInstance(shellType);
                if (shell is not null)
                {
                    try
                    {
                        dynamic dynShell = shell;
                        dynamic shortcut = dynShell.CreateShortcut(expanded);
                        try
                        {
                            string? iconLoc = shortcut.IconLocation as string;
                            if (!string.IsNullOrWhiteSpace(iconLoc))
                            {
                                string iconFile = iconLoc.Split(',')[0].Trim().Trim('"');
                                iconFile = Environment.ExpandEnvironmentVariables(iconFile);
                                if (File.Exists(iconFile) &&
                                    (iconFile.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) ||
                                     iconFile.EndsWith(".ico", StringComparison.OrdinalIgnoreCase) ||
                                     iconFile.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)))
                                {
                                    return iconFile;
                                }
                            }

                            string? targetPath = shortcut.TargetPath as string;
                            if (!string.IsNullOrWhiteSpace(targetPath))
                            {
                                string expandedTarget = Environment.ExpandEnvironmentVariables(targetPath.Trim().Trim('"'));
                                if (File.Exists(expandedTarget))
                                {
                                    return expandedTarget;
                                }
                            }
                        }
                        finally
                        {
                            if (Marshal.IsComObject(shortcut))
                            {
                                Marshal.ReleaseComObject(shortcut);
                            }
                        }
                    }
                    finally
                    {
                        if (Marshal.IsComObject(shell))
                        {
                            Marshal.ReleaseComObject(shell);
                        }
                    }
                }
            }
        }
        catch
        {
            // Fall through to binary .lnk scan
        }

        // 2. Fallback: scan binary .lnk bytes for an embedded .exe path
        try
        {
            byte[] raw = File.ReadAllBytes(expanded);
            string ascii = Encoding.ASCII.GetString(raw);
            string unicode = Encoding.Unicode.GetString(raw);

            string? resolved = TryFindExecutablePathInText(unicode) ?? TryFindExecutablePathInText(ascii);
            if (!string.IsNullOrWhiteSpace(resolved) && File.Exists(resolved))
            {
                return resolved;
            }
        }
        catch
        {
            // Ignore read errors
        }

        return null;
    }

    private static string? TryFindExecutablePathInText(string content)
    {
        int exeIndex = content.IndexOf(".exe", StringComparison.OrdinalIgnoreCase);
        while (exeIndex > 2)
        {
            int end = exeIndex + 4;
            int start = exeIndex - 1;
            while (start > 0 && content[start] >= 32 && content[start] != '"' && content[start] != '\0')
            {
                if (start + 2 <= exeIndex &&
                    char.IsLetter(content[start]) &&
                    content[start + 1] == ':' &&
                    (content[start + 2] == '\\' || content[start + 2] == '/'))
                {
                    string candidate = Environment.ExpandEnvironmentVariables(content[start..end].Trim());
                    if (File.Exists(candidate))
                    {
                        return candidate;
                    }
                    break;
                }
                start--;
            }

            exeIndex = content.IndexOf(".exe", end, StringComparison.OrdinalIgnoreCase);
        }

        return null;
    }

    internal static byte[]? ExtractWindowOrExecutableIconBgra32(IntPtr hWnd, string? executablePath)
    {
        // 1. Prefer direct clean executable icon if available and cached
        if (!string.IsNullOrWhiteSpace(executablePath))
        {
            if (Cache.TryGetValue(executablePath, out byte[]? cached) && cached is not null)
            {
                return cached;
            }

            byte[]? fromExe = ExtractNativeBgra32(executablePath);
            if (fromExe is not null)
            {
                Cache[executablePath] = fromExe;
                return fromExe;
            }
        }

        // 2. Fallback for UWP / ApplicationFrameHost or elevated processes: query window HICON directly
        if (hWnd != IntPtr.Zero)
        {
            IntPtr hIcon = GetWindowIconHandle(hWnd);
            if (hIcon != IntPtr.Zero)
            {
                return RenderHIconToBgra32(hIcon, destroyHandleAfterRender: false);
            }
        }

        return null;
    }

    private static IntPtr GetWindowIconHandle(IntPtr hWnd)
    {
        if (Win32NativeMethods.SendMessageTimeoutW(
                hWnd,
                Win32NativeMethods.WM_GETICON,
                new IntPtr(Win32NativeMethods.ICON_BIG),
                IntPtr.Zero,
                Win32NativeMethods.SMTO_ABORTIFHUNG,
                40,
                out IntPtr hIcon) != IntPtr.Zero && hIcon != IntPtr.Zero)
        {
            return hIcon;
        }

        if (Win32NativeMethods.SendMessageTimeoutW(
                hWnd,
                Win32NativeMethods.WM_GETICON,
                new IntPtr(Win32NativeMethods.ICON_SMALL2),
                IntPtr.Zero,
                Win32NativeMethods.SMTO_ABORTIFHUNG,
                40,
                out hIcon) != IntPtr.Zero && hIcon != IntPtr.Zero)
        {
            return hIcon;
        }

        hIcon = Win32NativeMethods.GetClassLongPtr(hWnd, Win32NativeMethods.GCLP_HICON);
        if (hIcon != IntPtr.Zero)
        {
            return hIcon;
        }

        return Win32NativeMethods.GetClassLongPtr(hWnd, Win32NativeMethods.GCLP_HICONSM);
    }

    internal static byte[]? ExtractNativeBgra32(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        string effectivePath = path;
        if (path.EndsWith(".lnk", StringComparison.OrdinalIgnoreCase))
        {
            string? resolved = ResolveShortcutTargetPath(path);
            if (!string.IsNullOrWhiteSpace(resolved) && File.Exists(resolved))
            {
                effectivePath = resolved;
            }
        }

        if (!File.Exists(effectivePath))
        {
            return null;
        }

        // 1. First use ExtractIconExW directly on the binary (.exe / .dll / .ico) so Windows NEVER draws a shortcut arrow overlay
        IntPtr[] largeIcons = new IntPtr[1];
        uint extractedCount = Win32NativeMethods.ExtractIconExW(effectivePath, 0, largeIcons, null, 1);
        if (extractedCount > 0 && largeIcons[0] != IntPtr.Zero)
        {
            byte[]? pureBytes = RenderHIconToBgra32(largeIcons[0], destroyHandleAfterRender: true);
            if (pureBytes is not null)
            {
                return pureBytes;
            }
        }

        // 2. Fallback to SHGetFileInfoW if the file is not a standard PE icon container
        var shfi = new Win32NativeMethods.SHFILEINFOW();
        IntPtr result = Win32NativeMethods.SHGetFileInfoW(
            effectivePath,
            0,
            ref shfi,
            (uint)Marshal.SizeOf<Win32NativeMethods.SHFILEINFOW>(),
            Win32NativeMethods.SHGFI_ICON | Win32NativeMethods.SHGFI_LARGEICON);

        if (result == IntPtr.Zero || shfi.hIcon == IntPtr.Zero)
        {
            return null;
        }

        return RenderHIconToBgra32(shfi.hIcon, destroyHandleAfterRender: true);
    }

    private static byte[]? RenderHIconToBgra32(IntPtr hIcon, bool destroyHandleAfterRender)
    {
        if (hIcon == IntPtr.Zero)
        {
            return null;
        }

        IntPtr hdc = Win32NativeMethods.CreateCompatibleDC(IntPtr.Zero);
        if (hdc == IntPtr.Zero)
        {
            if (destroyHandleAfterRender)
            {
                Win32NativeMethods.DestroyIcon(hIcon);
            }
            return null;
        }

        try
        {
            var bmi = new Win32NativeMethods.BITMAPINFOHEADER
            {
                biSize = (uint)Marshal.SizeOf<Win32NativeMethods.BITMAPINFOHEADER>(),
                biWidth = IconDimension,
                biHeight = -IconDimension, // Top-down DIB
                biPlanes = 1,
                biBitCount = 32
            };

            IntPtr hBitmap = Win32NativeMethods.CreateDIBSection(hdc, ref bmi, 0, out IntPtr ppvBits, IntPtr.Zero, 0);
            if (hBitmap == IntPtr.Zero || ppvBits == IntPtr.Zero)
            {
                return null;
            }

            IntPtr oldObj = Win32NativeMethods.SelectObject(hdc, hBitmap);
            try
            {
                Win32NativeMethods.DrawIconEx(
                    hdc,
                    0,
                    0,
                    hIcon,
                    IconDimension,
                    IconDimension,
                    0,
                    IntPtr.Zero,
                    Win32NativeMethods.DI_NORMAL);

                byte[] pixels = new byte[ByteCount];
                Marshal.Copy(ppvBits, pixels, 0, ByteCount);
                return pixels;
            }
            finally
            {
                Win32NativeMethods.SelectObject(hdc, oldObj);
                Win32NativeMethods.DeleteObject(hBitmap);
            }
        }
        finally
        {
            Win32NativeMethods.DeleteDC(hdc);
            if (destroyHandleAfterRender)
            {
                Win32NativeMethods.DestroyIcon(hIcon);
            }
        }
    }
}
