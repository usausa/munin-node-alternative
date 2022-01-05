namespace Munin.Node.Plugins.Hardware;

using System.Diagnostics.CodeAnalysis;

using LibreHardwareMonitor.Hardware;

[SuppressMessage("Microsoft.Performance", "CA1815:Override equals and operator equals on value types", Justification = "Ignore")]
public struct SensorValue
{
    public HardwareType HardwareType { get; set; }

    public SensorType SensorType { get; set; }

    public int Index { get; set; }

    public string HardwareName { get; set; }

    public string SensorName { get; set; }

    public float? Value { get; set; }
}
