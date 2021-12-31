namespace Munin.Node;

public interface ISessionInitializer
{
    void Setup();

    void Shutdown();
}
