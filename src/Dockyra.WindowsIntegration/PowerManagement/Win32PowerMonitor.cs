using Dockyra.Contracts.Models;
using Dockyra.Contracts.Services;
using Dockyra.WindowsIntegration.Native;

namespace Dockyra.WindowsIntegration.PowerManagement;

public sealed class Win32PowerMonitor : IPowerMonitor
{
    private PowerState _lastKnownState;
    private bool _disposed;

    public Win32PowerMonitor()
    {
        _lastKnownState = QueryCurrentOsPowerState();
    }

    public event EventHandler<PowerStateChangedEventArgs>? PowerStateChanged;

    public PowerState GetCurrentState()
    {
        PowerState current = QueryCurrentOsPowerState();
        if (!_disposed && current != _lastKnownState)
        {
            _lastKnownState = current;
            PowerStateChanged?.Invoke(this, new PowerStateChangedEventArgs(current));
        }

        return current;
    }

    private static PowerState QueryCurrentOsPowerState()
    {
        if (Win32NativeMethods.GetSystemPowerStatus(out Win32NativeMethods.SYSTEM_POWER_STATUS status))
        {
            PowerSourceKind source = status.ACLineStatus switch
            {
                0 => PowerSourceKind.Battery,
                1 => PowerSourceKind.AcPower,
                _ => PowerSourceKind.Unknown
            };

            int batteryPercent = status.BatteryLifePercent <= 100 ? status.BatteryLifePercent : 100;
            bool energySaver = status.SystemStatusFlag == 1;
            return new PowerState(source, batteryPercent, energySaver);
        }

        return new PowerState(PowerSourceKind.AcPower, 100, false);
    }

    public void Dispose()
    {
        _disposed = true;
        PowerStateChanged = null;
    }
}
