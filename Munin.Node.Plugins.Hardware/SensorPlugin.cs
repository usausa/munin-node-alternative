namespace Munin.Node.Plugins.Hardware;

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

    private readonly IHardware[] updateHardware;

    private readonly SensorInfo[] sensors;

    public byte[] Name { get; }

    public SensorPlugin(Computer computer, SensorEntry entry)
    {
        this.computer = computer;
        this.entry = entry;
        Name = Encoding.ASCII.GetBytes(entry.Name);

        var target = computer.Hardware
            .SelectMany(Filter)
            .OrderBy(x => entry.Order is null || IsMatch(x, entry.Order))
            .ThenBy(x => x.Hardware.HardwareType)
            .ThenBy(x => x.Hardware.Identifier.ToString())
            .ToList();
        var grouping = target
            .GroupBy(x => x.Hardware.Identifier)
            .ToDictionary(x => x.Key.ToString(), x => new
            {
                x.First().Hardware,
                SensorCount = x.Count()
            });

        updateHardware = grouping
            .Select(x => x.Value.Hardware)
            .ToArray();

        var singleHardware = grouping.Count == 1;
        var singleSensor = grouping.All(x => x.Value.SensorCount == 1);
        sensors = target
            .Select(x => new SensorInfo
            {
                Field = Encoding.ASCII.GetBytes(MakeFieldName(x)),
                Label = Encoding.ASCII.GetBytes(MakeLabelName(singleHardware, singleSensor, x)),
                Sensor = x
            })
            .ToArray();

        // Debug
#if DEBUG
        System.Diagnostics.Debug.WriteLine($"[{entry.Name}]");
        foreach (var hardware in updateHardware)
        {
            System.Diagnostics.Debug.WriteLine($"Hardware: {hardware.Name}");
            hardware.Update();
        }

        foreach (var sensor in sensors)
        {
            System.Diagnostics.Debug.WriteLine($"Sensor: {sensor.Sensor.Hardware.Name}/{sensor.Sensor.Name} : {sensor.Sensor.Value}");
        }
#endif
    }

    private static string MakeFieldName(ISensor sensor)
    {
        if (sensor.Hardware.HardwareType == HardwareType.Network)
        {
            return sensor.Identifier.ToString()[1..].Replace('/', '_')
                .Replace("-", string.Empty, StringComparison.Ordinal)
                .Replace("{", string.Empty, StringComparison.Ordinal)
                .Replace("}", string.Empty, StringComparison.Ordinal);
        }

        return sensor.Identifier.ToString()[1..].Replace('/', '_');
    }

    private static string MakeLabelName(bool singleHardware, bool singleSensor, ISensor sensor)
    {
        if (singleHardware)
        {
            return sensor.Name;
        }

        if (singleSensor)
        {
            return sensor.Hardware.Name;
        }

        return $"{sensor.Hardware.Name} {sensor.Name}";
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

    public void BuildConfig(ResponseBuilder response)
    {
        // graph_category
        response.Add("graph_category ");
        response.Add(entry.GraphCategory);
        response.AddLineFeed();
        // graph_title
        response.Add("graph_title ");
        response.Add(entry.GraphTitle);
        response.AddLineFeed();
        // graph_vlabel
        response.Add("graph_vlabel ");
        response.Add(entry.GraphVLabel);
        response.AddLineFeed();
        // graph_category
        response.Add("graph_args ");
        response.Add(entry.GraphArgs);
        response.AddLineFeed();
        // graph_scale
        if (entry.GraphScale.HasValue)
        {
            response.Add("graph_scale ");
            response.Add(entry.GraphScale.Value ? "yes" : "no");
            response.AddLineFeed();
        }

        foreach (var sensor in sensors)
        {
            // TODO custom per field: draw, type, color
            // label
            response.Add(sensor.Field);
            response.Add(".label ");
            response.Add(sensor.Label);
            response.AddLineFeed();
            // draw
            if (!String.IsNullOrEmpty(entry.GraphDraw))
            {
                response.Add(sensor.Field);
                response.Add(".draw ");
                response.Add(entry.GraphDraw);
                response.AddLineFeed();
            }
        }

        response.AddEndLine();
    }

    public void BuildFetch(ResponseBuilder response)
    {
        lock (computer)
        {
            foreach (var hardware in updateHardware)
            {
                hardware.Update();
            }

            foreach (var sensor in sensors)
            {
                if (sensor.Sensor.Value.HasValue)
                {
                    // value
                    response.Add(sensor.Field);
                    response.Add(".value ");
                    response.Add(entry.Multiply.HasValue
                        ? sensor.Sensor.Value.Value * entry.Multiply.Value
                        : sensor.Sensor.Value.Value);
                    response.AddLineFeed();
                }
            }
        }

        response.AddEndLine();
    }
}
