namespace Munin.Node;

#pragma warning disable CA1819
public interface IPlugin
{
    byte[] Name { get; }

    void BuildConfig(ResponseBuilder response);

    void BuildFetch(ResponseBuilder response);
}
#pragma warning restore CA1819
