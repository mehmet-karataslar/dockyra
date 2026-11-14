namespace Dockyra.Contracts.Models;

public readonly record struct WorkAreaRect(int Left, int Top, int Width, int Height)
{
    public int Right => Left + Width;
    public int Bottom => Top + Height;
}

public sealed record MonitorInfo(
    string DeviceId,
    string FriendlyName,
    bool IsPrimary,
    WorkAreaRect Bounds,
    WorkAreaRect WorkArea,
    double ScaleFactor);

