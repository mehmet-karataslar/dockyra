using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using Dockyra.Contracts.Services;
using Dockyra.WindowsIntegration.Native;

namespace Dockyra.WindowsIntegration.Shell;

/// <summary>
/// Extracts 32x32 32-bit BGRA application icons from local executables (.exe) and shell shortcuts (.lnk)
/// using Win32 SHGetFileInfoW and GDI DIB sections, caching results in memory.
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

    internal static byte[]? ExtractNativeBgra32(string path)
    {
        if (!File.Exists(path))
        {
            return null;
        }

        var shfi = new Win32NativeMethods.SHFILEINFOW();
        IntPtr result = Win32NativeMethods.SHGetFileInfoW(
            path,
            0,
            ref shfi,
            (uint)Marshal.SizeOf<Win32NativeMethods.SHFILEINFOW>(),
            Win32NativeMethods.SHGFI_ICON | Win32NativeMethods.SHGFI_LARGEICON);

        if (result == IntPtr.Zero || shfi.hIcon == IntPtr.Zero)
        {
            return null;
        }

        IntPtr hdc = Win32NativeMethods.CreateCompatibleDC(IntPtr.Zero);
        if (hdc == IntPtr.Zero)
        {
            Win32NativeMethods.DestroyIcon(shfi.hIcon);
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
                    shfi.hIcon,
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
            Win32NativeMethods.DestroyIcon(shfi.hIcon);
        }
    }
}

