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

    private readonly SensorRepository repository;

    private readonly SensorInfo[] sensors;

    public byte[] Name { get; }

    public SensorPlugin(SensorRepository repository, SensorEntry entry)
    {
        this.repository = repository;
        this.entry = entry;
        Name = Encoding.ASCII.GetBytes(entry.Name);

        var target = repository.EnumerableSensor()
            .Where(Filter)
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
            return sensor.Identifier.ToString()![1..].Replace('/', '_')
                .Replace("-", string.Empty, StringComparison.Ordinal)
                .Replace("{", string.Empty, StringComparison.Ordinal)
                .Replace("}", string.Empty, StringComparison.Ordinal);
        }

        return sensor.Identifier.ToString()![1..].Replace('/', '_');
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

    private bool Filter(ISensor sensor)
    {
        if (sensor.SensorType != entry.Sensor)
        {
            return false;
        }

        if ((entry.Hardware?.Length > 0) && (Array.IndexOf(entry.Hardware, sensor.Hardware.HardwareType) < 0))
        {
            return false;
        }

        if ((entry.Include?.Length > 0) && !IsMatch(sensor, entry.Include))
        {
            return false;
        }

        if ((entry.Exclude?.Length > 0) && IsMatch(sensor, entry.Exclude))
        {
            return false;
        }

        return true;
    }

    private static bool IsMatch(ISensor sensor, IEnumerable<FilterEntry> filters)
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
            // type
            if (!String.IsNullOrEmpty(entry.GraphType))
            {
                response.Add(sensor.Field);
                response.Add(".type ");
                response.Add(entry.GraphType);
                response.AddLineFeed();
            }
        }

        response.AddEndLine();
    }

    public void BuildFetch(ResponseBuilder response)
    {
        lock (repository.Sync)
        {
            repository.Update();

            foreach (var sensor in sensors)
            {
                if (sensor.Sensor.Value.HasValue)
                {
                    // value
                    response.Add(sensor.Field);
                    response.Add(".value ");
                    if (entry.GraphType is "DERIVE" or "COUNTER")
                    {
                        response.Add((int)(entry.Multiply.HasValue
                            ? sensor.Sensor.Value.Value * entry.Multiply.Value
                            : sensor.Sensor.Value.Value));
                    }
                    else
                    {
                        response.Add(entry.Multiply.HasValue
                            ? sensor.Sensor.Value.Value * entry.Multiply.Value
                            : sensor.Sensor.Value.Value);
                    }
                    response.AddLineFeed();
                }
            }
        }

        response.AddEndLine();
    }
}
