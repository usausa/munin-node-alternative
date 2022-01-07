namespace Munin.Node;

public interface IPlugin
{
    byte[] Name { get; }

    void BuildConfig(ResponseBuilder response);

    void BuildFetch(ResponseBuilder response);
}
