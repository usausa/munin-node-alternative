namespace Munin.Node.Plugins.Hardware;

using System.Diagnostics;

using LibreHardwareMonitor.Hardware;

internal sealed class SensorRepository
{
    private readonly Computer computer;

    private readonly long expire;

    private long lastTick;

    public object Sync => computer;

    public SensorRepository(Computer computer, long expire)
    {
        this.computer = computer;
        this.expire = expire;

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

    public void Update()
    {
        var timestamp = Stopwatch.GetTimestamp();
        if ((timestamp - lastTick) < expire)
        {
            return;
        }

        foreach (var hardware in computer.Hardware)
        {
            Update(hardware);
        }

        lastTick = timestamp;
    }

    public IEnumerable<ISensor> EnumerableSensor()
    {
        return computer.Hardware.SelectMany(EnumerableSensor);
    }

    public IEnumerable<ISensor> EnumerableSensor(IHardware hardware)
    {
        foreach (var sensor in hardware.Sensors)
        {
            yield return sensor;
        }

        foreach (var subHardware in hardware.SubHardware)
        {
            foreach (var sensor in EnumerableSensor(subHardware))
            {
                yield return sensor;
            }
        }
    }
}
