namespace Munin.Node;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public interface IPluginInitializer
{
    void Setup(IConfiguration config, IServiceCollection services);
}
