namespace Munin.Node.Plugins.Hardware;

public sealed class MemoryPlugin : IPlugin
{
    public byte[] Name { get; }

    public MemoryPlugin(byte[] name)
    {
        Name = name;
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
