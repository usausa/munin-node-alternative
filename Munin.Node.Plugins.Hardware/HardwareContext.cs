namespace Munin.Node.Plugins.Hardware;

internal static class HardwareContext
{
    public static AsyncLocal<List<SensorValue>> Snapshot { get; } = new();
}
