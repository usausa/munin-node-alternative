namespace Munin.Node;

public interface IPlugin
{
    byte[] Name { get; }

    void BuildConfig(BufferSegment buffer);

    void BuildFetch(BufferSegment buffer);
}
