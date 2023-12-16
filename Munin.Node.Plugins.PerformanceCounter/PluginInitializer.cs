namespace Munin.Node.Plugins.PerformanceCounter;

using System.Runtime.Versioning;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[SupportedOSPlatform("windows")]
internal sealed class PluginInitializer : IPluginInitializer
{
    public void Setup(IConfiguration config, IServiceCollection services)
    {
        var settings = config.GetSection("PerformanceCounter").Get<Settings>()!;
        if (settings.Counter?.Length > 0)
        {
            foreach (var counter in settings.Counter)
            {
#pragma warning disable CA2000
                services.AddSingleton<IPlugin>(new PerformanceCounterPlugin(counter));
#pragma warning restore CA2000
            }
        }
    }
}
