namespace Munin.Node.Plugins.Hardware;

using LibreHardwareMonitor.Hardware;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public sealed class PluginInitializer : IPluginInitializer
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Ignore")]
    public void Setup(IConfiguration config, IServiceCollection services)
    {
        var settings = config.GetSection("Hardware").Get<Settings>();

        var computer = new Computer
        {
            IsMotherboardEnabled = settings.IsMotherboardControlEnable() ||
                                   settings.IsMotherboardFanEnable() ||
                                   settings.IsMotherboardTemperatureEnable() ||
                                   settings.IsMotherboardVoltageEnable(),
            IsCpuEnabled = settings.IsCpuClockEnable() ||
                           settings.IsCpuLoadEnable() ||
                           settings.IsCpuPowerEnable() ||
                           settings.IsCpuTemperatureEnable() ||
                           settings.IsCpuVoltageEnable(),
            IsMemoryEnabled = settings.IsMemoryLoadEnable() ||
                              settings.IsMemoryDataEnable(),
            IsGpuEnabled = settings.IsGpuClockEnable() ||
                           settings.IsGpuControlEnable() ||
                           settings.IsGpuFanEnable() ||
                           settings.IsGpuLoadEnable() ||
                           settings.IsGpuPowerEnable() ||
                           settings.IsGpuTemperatureEnable() ||
                           settings.IsGpuVoltageEnable(),
            IsStorageEnabled = settings.IsStorageDataEnable() ||
                               settings.IsStorageLevelEnable() ||
                               settings.IsStorageTemperatureEnable() ||
                               settings.IsStorageThroughputEnable() ||
                               settings.IsStorageUsedEnable(),
            IsNetworkEnabled = settings.IsNetworkDataEnable() ||
                               settings.IsNetworkLoadEnable() ||
                               settings.IsNetworkThroughputEnable()
        };
        computer.Open();
        services.AddSingleton<ISessionInitializer>(new SessionInitializer(computer));

        // TODO センサー一覧は分配してプラグインにも渡す

        // TODO Plugin
        services.AddSensorPlugin(settings.IsMemoryLoadEnable(), "memory_load", x => x.MemoryLoad);

        // TODO Custom plugin
    }
}
public static class ServiceCollectionExtensions
{
    public static void AddSensorPlugin(this IServiceCollection services, bool condition, string name, Func<SensorValue, float?[]> accessor)
    {
        if (condition)
        {
            services.AddSingleton<IPlugin>(new SensorPlugin(name, accessor));
        }
    }
}
