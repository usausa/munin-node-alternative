namespace Munin.Node.Plugins.Hardware;

using System.Text;

using LibreHardwareMonitor.Hardware;

internal sealed class MemoryPlugin : IPlugin
{
    public byte[] Name { get; }

    private readonly CustomEntry entry;

    private readonly SensorRepository repository;

    private readonly ISensor physicalMemoryUsed;
    private readonly ISensor physicalMemoryAvailable;
    private readonly ISensor virtualMemoryUsed;

    public MemoryPlugin(SensorRepository repository, CustomEntry entry)
    {
        this.repository = repository;
        this.entry = entry;
        var name = entry.Name ?? "memory";
        Name = Encoding.ASCII.GetBytes(name);

        foreach (var sensor in repository.EnumerableSensor().Where(x => (x.Hardware.HardwareType == HardwareType.Memory) && (x.SensorType == SensorType.Data)))
        {
            switch (sensor.Name)
            {
                case "Memory Used":
                    physicalMemoryUsed = sensor;
                    break;
                case "Memory Available":
                    physicalMemoryAvailable = sensor;
                    break;
                case "Virtual Memory Used":
                    virtualMemoryUsed = sensor;
                    break;
            }
        }

        if ((physicalMemoryUsed is null) || (physicalMemoryAvailable is null) || (virtualMemoryUsed is null))
        {
            throw new InvalidOperationException("Sensor not found.");
        }

        // Debug
#if DEBUG
        System.Diagnostics.Debug.WriteLine($"[{name}]");
        System.Diagnostics.Debug.WriteLine($"Sensor: {physicalMemoryUsed.Name} : {physicalMemoryUsed.Value}");
        System.Diagnostics.Debug.WriteLine($"Sensor: {physicalMemoryAvailable.Name} : {physicalMemoryAvailable.Value}");
        System.Diagnostics.Debug.WriteLine($"Sensor: {virtualMemoryUsed.Name} : {virtualMemoryUsed.Value}");
#endif
    }

    public void BuildConfig(ResponseBuilder response)
    {
        // graph_category
        response.Add("graph_category ");
        response.Add(entry.GraphCategory ?? "memory");
        response.AddLineFeed();
        // graph_title
        response.Add("graph_title ");
        response.Add(entry.GraphTitle ?? "Memory");
        response.AddLineFeed();
        // graph_vlabel
        response.Add("graph_vlabel ");
        response.Add(entry.GraphVLabel ?? "GB");
        response.AddLineFeed();
        // graph_category
        response.Add("graph_args ");
        response.Add(entry.GraphArgs ?? "-b 1024 -l 0");
        response.AddLineFeed();
        // graph_order
        response.Add("graph_order apps free swap");
        response.AddLineFeed();

        response.Add("apps.label apps");
        response.AddLineFeed();
        response.Add("apps.draw AREA");
        response.AddLineFeed();
        response.Add("free.label free");
        response.AddLineFeed();
        response.Add("free.draw STACK");
        response.AddLineFeed();
        response.Add("swap.label swap");
        response.AddLineFeed();
        response.Add("swap.draw STACK");
        response.AddLineFeed();

        response.AddEndLine();
    }

    public void BuildFetch(ResponseBuilder response)
    {
        lock (repository.Sync)
        {
            repository.Update();

            response.Add("apps.value ");
            response.Add(physicalMemoryUsed.Value!.Value);
            response.AddLineFeed();
            response.Add("free.value ");
            response.Add(physicalMemoryAvailable.Value!.Value);
            response.AddLineFeed();
            response.Add("swap.value ");
            response.Add(virtualMemoryUsed.Value!.Value);
            response.AddLineFeed();
        }

        response.AddEndLine();
    }
}
