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

    public static IEnumerable<IHardware> EnumerableHardware(Computer computer)
    {
        foreach (var hardware in computer.Hardware)
        {
            yield return hardware;

            foreach (var subHardware in EnumerableSubHardware(hardware))
            {
                yield return subHardware;
            }
        }
    }

    private static IEnumerable<IHardware> EnumerableSubHardware(IHardware hardware)
    {
        foreach (var subHardware in hardware.SubHardware)
        {
            yield return subHardware;

            foreach (var subSubHardware in EnumerableSubHardware(subHardware))
            {
                yield return subSubHardware;
            }
        }
    }
}
