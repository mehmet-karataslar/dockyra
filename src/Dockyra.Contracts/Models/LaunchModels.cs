namespace Dockyra.Contracts.Models;

public sealed record ApplicationLaunchRequest(
    string ExecutablePath,
    string? Arguments = null,
    string? WorkingDirectory = null);

public sealed record LaunchResult(
    bool IsSuccess,
    int? ProcessId = null,
    string? ErrorCode = null,
    string? ErrorMessage = null)
{
    public static LaunchResult Succeeded(int? processId) => new(true, processId, null, null);
    public static LaunchResult Failed(string code, string message) => new(false, null, code, message);
}

