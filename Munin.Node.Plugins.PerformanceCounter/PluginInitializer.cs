namespace Munin.Node.Plugins.PerformanceCounter;

using System.Runtime.Versioning;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[SupportedOSPlatform("windows")]
internal sealed class PluginInitializer : IPluginInitializer
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Ignore")]
    public void Setup(IConfiguration config, IServiceCollection services)
    {
        var settings = config.GetSection("PerformanceCounter").Get<Settings>()!;
        if (settings.Counter?.Length > 0)
        {
            foreach (var counter in settings.Counter)
            {
                services.AddSingleton<IPlugin>(new PerformanceCounterPlugin(counter));
            }
        }
    }
}
