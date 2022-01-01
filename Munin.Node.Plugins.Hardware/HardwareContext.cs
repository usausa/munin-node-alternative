namespace Munin.Node.Plugins.Hardware;

public static class HardwareContext
{
    public static AsyncLocal<List<SensorValue>> Snapshot { get; } = new();
}
