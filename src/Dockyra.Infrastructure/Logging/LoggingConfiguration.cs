using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;

namespace Dockyra.Infrastructure.Logging;

public static class LoggingConfiguration
{
    public static ILoggerFactory CreateLoggerFactory(string? customLogDirectory = null)
    {
        string logDir = customLogDirectory ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Dockyra",
            "logs");

        Directory.CreateDirectory(logDir);
        string logFilePath = Path.Combine(logDir, "dockyra-.log");

        Serilog.Core.Logger serilogLogger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.File(
                path: logFilePath,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                fileSizeLimitBytes: 10 * 1024 * 1024,
                rollOnFileSizeLimit: true)
            .CreateLogger();

        return new SerilogLoggerFactory(serilogLogger, dispose: true);
    }
}

