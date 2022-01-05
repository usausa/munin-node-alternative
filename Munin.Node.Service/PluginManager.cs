namespace Munin.Node.Service;

using System.Collections.Generic;
using System.Linq;

public sealed class PluginManager
{
    private readonly ISessionInitializer[] initializers;

    private readonly IPlugin[] plugins;

    public PluginManager(IEnumerable<ISessionInitializer> initializers, IEnumerable<IPlugin> plugins)
    {
        this.initializers = initializers.ToArray();
        this.plugins = plugins.ToArray();
    }

    public void SetupSession()
    {
        foreach (var initializer in initializers)
        {
            initializer.Setup();
        }
    }

    public void ShutdownSession()
    {
        foreach (var initializer in initializers)
        {
            initializer.Shutdown();
        }
    }

    public IPlugin? LookupPlugin(Span<byte> name)
    {
        var values = plugins;
        for (var i = 0; i < plugins.Length; i++)
        {
            var plugin = values[i];
            if (name.SequenceEqual(plugin.Name))
            {
                return plugin;
            }
        }

        return null;
    }

    public void BuildNames(BufferSegment buffer)
    {
        var values = plugins;
        for (var i = 0; i < values.Length; i++)
        {
            buffer.Add(values[i].Name);
            buffer.Add((byte)' ');
        }
        buffer.AddLineFeed();
    }
}
