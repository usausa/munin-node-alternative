namespace Munin.Node.Plugins.Hardware;

using LibreHardwareMonitor.Hardware;

#pragma warning disable CA1051
#pragma warning disable CA1815
internal struct SensorValue
{
    public HardwareType HardwareType;

    public SensorType SensorType;

    public int Index;

    public string HardwareName;

    public string SensorName;

    public float? Value;
}
#pragma warning restore CA1815
#pragma warning restore CA1051
