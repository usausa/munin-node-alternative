namespace Munin.Node.Plugins.Hardware;

using System.Diagnostics;
using System.Text;

using LibreHardwareMonitor.Hardware;

internal sealed class SensorPlugin : IPlugin
{
#pragma warning disable CS8618
#pragma warning disable SA1401
    private sealed class SensorInfo
    {
        public byte[] Field;

        public byte[] Label;

        public ISensor Sensor;
    }
#pragma warning restore SA1401
#pragma warning restore CS8618

    private readonly SensorEntry entry;

    private readonly Computer computer;

    private readonly IHardware[] uniqHardware;

    private readonly SensorInfo[] sensors;

    public byte[] Name { get; }

    public SensorPlugin(Computer computer, SensorEntry entry)
    {
        this.computer = computer;
        this.entry = entry;
        Name = Encoding.ASCII.GetBytes(entry.Name);

        var target = computer.Hardware.SelectMany(Filter);
        if (entry.Order is not null)
        {
            target = target.OrderBy(x => IsMatch(x, entry.Order));
        }

        // TODO label
        sensors = target
            .Select(x => new SensorInfo
            {
                Field = Encoding.ASCII.GetBytes(x.Identifier.ToString()[1..].Replace('/', '_')),
                Label = Encoding.ASCII.GetBytes(x.Name),
                Sensor = x
            })
            .ToArray();
        var set = new HashSet<string>();
        uniqHardware = sensors
            .Where(x => set.Add(x.Sensor.Hardware.Identifier.ToString()))
            .Select(x => x.Sensor.Hardware)
            .ToArray();

        // Debug
#if DEBUG
        Debug.WriteLine($"[{entry.Name}]");
        foreach (var hardware in uniqHardware)
        {
            Debug.WriteLine($"Hardware: {hardware.Name}");
            hardware.Update();
        }

        foreach (var sensor in sensors)
        {
            Debug.WriteLine($"Sensor: {sensor.Sensor.Hardware.Name}/{sensor.Sensor.Name} : {sensor.Sensor.Value}");
        }
#endif
    }

    public IEnumerable<ISensor> Filter(IHardware hardware)
    {
        foreach (var subHardware in hardware.SubHardware)
        {
            foreach (var sensor in Filter(subHardware))
            {
                yield return sensor;
            }
        }

        foreach (var sensor in hardware.Sensors)
        {
            if (sensor.SensorType != entry.Sensor)
            {
                continue;
            }

            if ((entry.Hardware?.Length > 0) && (Array.IndexOf(entry.Hardware, sensor.Hardware.HardwareType) < 0))
            {
                continue;
            }

            if ((entry.Include?.Length > 0) && !IsMatch(sensor, entry.Include))
            {
                continue;
            }

            if ((entry.Exclude?.Length > 0) && IsMatch(sensor, entry.Exclude))
            {
                continue;
            }

            yield return sensor;
        }
    }

    public static bool IsMatch(ISensor sensor, FilterEntry[] filters)
    {
        return filters.Any(x => (!x.Type.HasValue || (x.Type == sensor.Hardware.HardwareType)) &&
                                (String.IsNullOrEmpty(x.Name) || (x.Name == sensor.Name)));
    }

    public void BuildConfig(BufferSegment buffer)
    {
        // graph_category
        buffer.Add("graph_category ");
        buffer.Add(entry.GraphCategory);
        buffer.AddLineFeed();
        // graph_title
        buffer.Add("graph_title ");
        buffer.Add(entry.GraphTitle);
        buffer.AddLineFeed();
        // graph_vlabel
        buffer.Add("graph_vlabel ");
        buffer.Add(entry.GraphVLabel);
        buffer.AddLineFeed();
        // graph_category
        buffer.Add("graph_args ");
        buffer.Add(entry.GraphArgs);
        buffer.AddLineFeed();
        // graph_scale
        if (entry.GraphScale.HasValue)
        {
            buffer.Add("graph_scale ");
            buffer.Add(entry.GraphScale.Value ? "yes" : "no");
            buffer.AddLineFeed();
        }

        foreach (var sensor in sensors)
        {
            // label
            buffer.Add(sensor.Field);
            buffer.Add(".label ");
            buffer.Add(sensor.Label);
            buffer.AddLineFeed();
            // draw
            if (!String.IsNullOrEmpty(entry.GraphDraw))
            {
                buffer.Add(sensor.Field);
                buffer.Add(".draw ");
                buffer.Add(entry.GraphDraw);    // TODO custom
                buffer.AddLineFeed();
            }
        }

        buffer.AddEndLine();
    }

    public void BuildFetch(BufferSegment buffer)
    {
        lock (computer)
        {
            foreach (var hardware in uniqHardware)
            {
                hardware.Update();
            }

            foreach (var sensor in sensors)
            {
                if (sensor.Sensor.Value.HasValue)
                {
                    // value
                    buffer.Add(sensor.Field);
                    buffer.Add(".value ");
                    buffer.Add(sensor.Sensor.Value.Value);
                    buffer.AddLineFeed();
                }
            }
        }

        buffer.AddEndLine();
    }
}
