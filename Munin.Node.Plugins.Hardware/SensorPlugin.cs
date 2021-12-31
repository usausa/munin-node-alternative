namespace Munin.Node.Plugins.Hardware;

using System.Text;

public sealed class SensorPlugin : IPlugin
{
    private readonly Func<SensorValue, float?[]> accessor;

    public byte[] Name { get; }

    public SensorPlugin(string name, Func<SensorValue, float?[]> accessor)
    {
        Name = Encoding.ASCII.GetBytes(name);
        this.accessor = accessor;
    }

    public void BuildConfig(BufferSegment buffer)
    {
        // TODO
    }

    public void BuildFetch(BufferSegment buffer)
    {
        // TODO
    }
}
