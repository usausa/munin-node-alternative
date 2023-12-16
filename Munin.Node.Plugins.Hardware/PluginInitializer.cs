namespace Munin.Node.Plugins.Hardware;

using LibreHardwareMonitor.Hardware;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

internal sealed class PluginInitializer : IPluginInitializer
{
    public void Setup(IConfiguration config, IServiceCollection services)
    {
        var settings = config.GetSection("Hardware").Get<Settings>()!;
        if ((settings.Sensor?.Length == 0) && !settings.Memory.IsEnable())
        {
            return;
        }

        var computer = new Computer
        {
            IsMotherboardEnabled = settings.Sensor.IsHardwareEnable(HardwareType.SuperIO),
            IsCpuEnabled = settings.Sensor.IsHardwareEnable(HardwareType.Cpu),
            IsMemoryEnabled = settings.Sensor.IsHardwareEnable(HardwareType.Memory) ||
                              settings.Memory.IsEnable(),
            IsGpuEnabled = settings.Sensor.IsHardwareEnable(HardwareType.GpuAmd) ||
                           settings.Sensor.IsHardwareEnable(HardwareType.GpuNvidia),
            IsStorageEnabled = settings.Sensor.IsHardwareEnable(HardwareType.Storage),
            IsNetworkEnabled = settings.Sensor.IsHardwareEnable(HardwareType.Network)
        };
        computer.Open();
        var repository = new SensorRepository(computer, settings.Expire * 10000L);

        if (settings.Sensor?.Length > 0)
        {
            foreach (var entry in settings.Sensor)
            {
                services.AddSingleton<IPlugin>(new SensorPlugin(repository, entry));
            }
        }

        if (settings.Memory.IsEnable())
        {
            services.AddSingleton<IPlugin>(new MemoryPlugin(repository, settings.Memory));
        }
    }
}
