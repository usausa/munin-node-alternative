namespace Munin.Node.Plugins.Hardware;

using System.Diagnostics;

using LibreHardwareMonitor.Hardware;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public sealed class PluginInitializer : IPluginInitializer
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Ignore")]
    public void Setup(IConfiguration config, IServiceCollection services)
    {
        var settings = config.GetSection("Hardware").Get<Settings>();
        if ((settings.Sensor?.Length == 0) && !settings.MemoryLoad.IsEnable())
        {
            return;
        }

        var computer = new Computer
        {
            IsMotherboardEnabled = settings.Sensor.IsHardwareEnable(HardwareType.SuperIO),
            IsCpuEnabled = settings.Sensor.IsHardwareEnable(HardwareType.Cpu),
            IsMemoryEnabled = settings.Sensor.IsHardwareEnable(HardwareType.Memory) ||
                              settings.MemoryLoad.IsEnable(),
            IsGpuEnabled = settings.Sensor.IsHardwareEnable(HardwareType.GpuAmd) ||
                           settings.Sensor.IsHardwareEnable(HardwareType.GpuNvidia),
            IsStorageEnabled = settings.Sensor.IsHardwareEnable(HardwareType.Storage),
            IsNetworkEnabled = settings.Sensor.IsHardwareEnable(HardwareType.Network)
        };
        computer.Open();
        services.AddSingleton<ISessionInitializer>(new SessionInitializer(computer));

        if (settings.Sensor?.Length > 0)
        {
            foreach (var entry in settings.Sensor)
            {
                services.AddSingleton<IPlugin>(new SensorPlugin(entry));
            }
        }

        if (settings.MemoryLoad.IsEnable())
        {
            services.AddSingleton<IPlugin>(new MemoryPlugin(settings.MemoryLoad.Name ?? "memory"));
        }

        // TODO
        SensorValueHelper.Update(computer);
        var values = SensorValuePool.Default.Rent();
        SensorValueHelper.Gather(computer, values);

        var subset = SensorValuePool.Default.Rent();
        foreach (var entry in settings.Sensor!)
        {
            subset.Clear();
            SensorValueHelper.Filter(values, subset, entry);
            System.Diagnostics.Debug.WriteLine("----" + entry.Name);
            foreach (var value in subset)
            {
                Debug.WriteLine($"{value.HardwareType}/{value.SensorType} : {value.HardwareName} / {value.SensorName} : {value.Value}");
            }
        }
    }
}
