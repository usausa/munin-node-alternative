namespace Munin.Node.Plugins.Hardware;

using System.Text;

public sealed class SensorPlugin : IPlugin
{
    private readonly SensorEntry entry;

    public byte[] Name { get; }

    public SensorPlugin(SensorEntry entry)
    {
        Name = Encoding.ASCII.GetBytes(entry.Name);
        this.entry = entry;
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
