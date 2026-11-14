namespace Dockyra.Contracts.Models;

public enum PowerMode
{
    PowerSaver = 0,
    Balanced = 1,
    Performance = 2
}

public enum PowerSourceKind
{
    Unknown = 0,
    AcPower = 1,
    Battery = 2
}

public sealed record PowerState(
    PowerSourceKind Source,
    int BatteryLifePercent,
    bool IsSystemEnergySaverActive);

public sealed class PowerStateChangedEventArgs : EventArgs
{
    public PowerStateChangedEventArgs(PowerState currentState)
    {
        CurrentState = currentState;
    }

    public PowerState CurrentState { get; }
}

