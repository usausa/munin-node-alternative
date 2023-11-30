namespace Munin.Node.Service;

using System.Reflection;

internal sealed class PluginBuilder
{
    private readonly List<Assembly> modules = [];

    public void AddModule(string path)
    {
        var context = new PluginLoadContext(path);
        modules.Add(context.LoadFromAssemblyPath(path));
    }

    public void Build(IConfiguration config, IServiceCollection services)
    {
        foreach (var type in modules.Select(static x => x.GetTypes().Where(typeof(IPluginInitializer).IsAssignableFrom)).SelectMany(static x => x))
        {
            var initializer = (IPluginInitializer)Activator.CreateInstance(type)!;
            initializer.Setup(config, services);
        }
    }
}
