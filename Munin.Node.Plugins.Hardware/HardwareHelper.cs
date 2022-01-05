namespace Munin.Node.Plugins.Hardware;

using LibreHardwareMonitor.Hardware;

internal static class HardwareHelper
{
    public static void Update(Computer computer)
    {
        foreach (var hardware in computer.Hardware)
        {
            Update(hardware);
        }
    }

    private static void Update(IHardware hardware)
    {
        hardware.Update();
        foreach (var subHardware in hardware.SubHardware)
        {
            Update(subHardware);
        }
    }
}
