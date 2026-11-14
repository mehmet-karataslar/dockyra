using Dockyra.Application.Interfaces;

namespace Dockyra.Application.Services;

/// <summary>
/// Coalesces rapid bursts of events into a single callback invocation after a quiet window.
/// </summary>
public sealed class EventCoalescer : IEventCoalescer
{
    private readonly Action _callback;
    private readonly TimeSpan _quietWindow;
    private readonly Timer _timer;
    private readonly object _sync = new();
    private long _coalescedOutputCount;
    private bool _disposed;

    public EventCoalescer(Action callback, TimeSpan quietWindow)
    {
        ArgumentNullException.ThrowIfNull(callback);
        _callback = callback;
        _quietWindow = quietWindow <= TimeSpan.Zero ? TimeSpan.FromMilliseconds(50) : quietWindow;
        _timer = new Timer(OnTimerFired, null, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
    }

    public long CoalescedOutputCount => Interlocked.Read(ref _coalescedOutputCount);

    public void Signal()
    {
        lock (_sync)
        {
            if (_disposed)
            {
                return;
            }

            _timer.Change(_quietWindow, Timeout.InfiniteTimeSpan);
        }
    }

    private void OnTimerFired(object? state)
    {
        lock (_sync)
        {
            if (_disposed)
            {
                return;
            }
        }

        Interlocked.Increment(ref _coalescedOutputCount);
        _callback();
    }

    public void Dispose()
    {
        lock (_sync)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _timer.Dispose();
        }
    }
}

