namespace Dockyra.Contracts.Models;

/// <summary>
/// Platform-agnostic identifier representing a tracked desktop window without leaking native HWND/IntPtr types.
/// </summary>
public readonly record struct WindowId(ulong Value)
{
    public bool IsValid => Value != 0;
    public override string ToString() => $"WIN-0x{Value:X}";
}

public enum WindowStateKind
{
    Normal = 0,
    Minimized = 1,
    Maximized = 2
}

public sealed record WindowInfo(
    WindowId Id,
    string Title,
    string ProcessName,
    string? ExecutablePath,
    int ProcessId,
    bool IsActive,
    WindowStateKind State,
    byte[]? IconBgra32 = null);

public sealed class WindowsChangedEventArgs : EventArgs
{
    public WindowsChangedEventArgs(IReadOnlyList<WindowInfo> windows, WindowId? activeWindowId)
    {
        Windows = windows;
        ActiveWindowId = activeWindowId;
    }

    public IReadOnlyList<WindowInfo> Windows { get; }
    public WindowId? ActiveWindowId { get; }
}

