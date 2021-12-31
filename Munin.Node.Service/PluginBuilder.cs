namespace Munin.Node.Service;

using System.Reflection;

public sealed class PluginBuilder
{
    private readonly List<Assembly> modules = new();

    public void AddModule(string name)
    {
        modules.Add(Assembly.LoadFrom(name + ".dll"));
    }

    public void Build(IConfiguration config, IServiceCollection services)
    {
        foreach (var type in modules.Select(x => x.ExportedTypes.Where(typeof(IPluginInitializer).IsAssignableFrom)).SelectMany(x => x))
        {
            var initializer = (IPluginInitializer)Activator.CreateInstance(type)!;
            initializer.Setup(config, services);
        }
    }
}
