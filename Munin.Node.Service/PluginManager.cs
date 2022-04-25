namespace Munin.Node.Service;

internal sealed class PluginManager
{
    private readonly IPlugin[] plugins;

    public PluginManager(IEnumerable<IPlugin> plugins)
    {
        this.plugins = plugins.ToArray();
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

    public void BuildNames(ResponseBuilder buffer)
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
