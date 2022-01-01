namespace Munin.Node.Plugins.Hardware;

using System.Text;

public sealed class MemoryPlugin : IPlugin
{
    public byte[] Name { get; }

    public MemoryPlugin(string name)
    {
        Name = Encoding.ASCII.GetBytes(name);
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
