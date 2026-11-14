using System.Diagnostics;
using Dockyra.Contracts.Models;
using Dockyra.Contracts.Services;
using Microsoft.Extensions.Logging;

namespace Dockyra.WindowsIntegration.ProcessManagement;

public sealed class Win32ApplicationLauncher : IApplicationLauncher
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".exe",
        ".lnk"
    };

    private readonly ILogger<Win32ApplicationLauncher> _logger;

    public Win32ApplicationLauncher(ILogger<Win32ApplicationLauncher> logger)
    {
        _logger = logger;
    }

    public Task<LaunchResult> LaunchAsync(
        ApplicationLaunchRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(request.ExecutablePath))
        {
            return Task.FromResult(LaunchResult.Failed("EMPTY_PATH", "Executable path is required."));
        }

        string path = Environment.ExpandEnvironmentVariables(request.ExecutablePath.Trim());
        if (!Path.IsPathFullyQualified(path))
        {
            return Task.FromResult(LaunchResult.Failed("RELATIVE_PATH_REJECTED", "Executable path must be a fully qualified local path."));
        }

        string extension = Path.GetExtension(path);
        if (!AllowedExtensions.Contains(extension))
        {
            return Task.FromResult(LaunchResult.Failed("DISALLOWED_EXTENSION", $"Extension '{extension}' is not allowed for direct launching."));
        }

        if (!File.Exists(path))
        {
            return Task.FromResult(LaunchResult.Failed("FILE_NOT_FOUND", $"File '{path}' does not exist."));
        }

        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = path,
                Arguments = request.Arguments ?? string.Empty,
                WorkingDirectory = !string.IsNullOrWhiteSpace(request.WorkingDirectory)
                    ? request.WorkingDirectory
                    : Path.GetDirectoryName(path) ?? string.Empty,
                UseShellExecute = true
            };

            using Process? process = Process.Start(startInfo);
            _logger.LogInformation("Launched application {ExecutablePath}", path);
            return Task.FromResult(LaunchResult.Succeeded(process?.Id));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to launch application {ExecutablePath}", path);
            return Task.FromResult(LaunchResult.Failed("LAUNCH_EXCEPTION", ex.Message));
        }
    }
}

