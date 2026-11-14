using System.Text.Json;
using Dockyra.Application.Interfaces;

namespace Dockyra.Infrastructure.Recovery;

public sealed class FileSafeModeStateStore : ISafeModeStateStore
{
    private readonly string _filePath;
    private readonly object _sync = new();

    public FileSafeModeStateStore(string? filePath = null)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string dir = Path.Combine(appData, "Dockyra");
            Directory.CreateDirectory(dir);
            _filePath = Path.Combine(dir, "safemode.json");
        }
        else
        {
            string? dir = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrWhiteSpace(dir))
            {
                Directory.CreateDirectory(dir);
            }

            _filePath = filePath;
        }
    }

    public int RecordStartupAttempt()
    {
        lock (_sync)
        {
            int next = GetConsecutiveFailuresCore() + 1;
            WriteFailuresCore(next);
            return next;
        }
    }

    public void RecordStartupSuccess()
    {
        lock (_sync)
        {
            WriteFailuresCore(0);
        }
    }

    public int GetConsecutiveFailures()
    {
        lock (_sync)
        {
            return GetConsecutiveFailuresCore();
        }
    }

    private int GetConsecutiveFailuresCore()
    {
        if (!File.Exists(_filePath))
        {
            return 0;
        }

        try
        {
            string json = File.ReadAllText(_filePath);
            SafeModeStatePayload? payload = JsonSerializer.Deserialize<SafeModeStatePayload>(json);
            return payload?.ConsecutiveFailures ?? 0;
        }
        catch
        {
            return 0;
        }
    }

    private void WriteFailuresCore(int count)
    {
        var payload = new SafeModeStatePayload(count, DateTimeOffset.UtcNow);
        string json = JsonSerializer.Serialize(payload);
        File.WriteAllText(_filePath, json);
    }

    private sealed record SafeModeStatePayload(int ConsecutiveFailures, DateTimeOffset UpdatedAtUtc);
}

