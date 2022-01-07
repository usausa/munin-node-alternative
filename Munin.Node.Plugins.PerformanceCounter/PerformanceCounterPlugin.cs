namespace Munin.Node.Plugins.PerformanceCounter;

using System.Diagnostics;
using System.Runtime.Versioning;
using System.Text;

[SupportedOSPlatform("windows")]
internal sealed class PerformanceCounterPlugin : IPlugin, IDisposable
{
#pragma warning disable CS8618
#pragma warning disable SA1401
    private sealed class CounterInfo
    {
        public byte[] Field;

        public byte[] Label;

        public PerformanceCounter Counter;

        public float? Multiply;
    }
#pragma warning restore SA1401
#pragma warning restore CS8618

    private readonly PerformanceCounterEntry entry;

    private readonly CounterInfo[] counters;

    public byte[] Name { get; }

    public PerformanceCounterPlugin(PerformanceCounterEntry entry)
    {
        this.entry = entry;
        Name = Encoding.ASCII.GetBytes(entry.Name);

        var list = entry.Object
            .Select(x => new { Object = x, Counters = Create(x.Category, x.Counter, x.Instance).ToList() })
            .ToList();
        var singleCounter = list.Count == 1;
        var singleInstance = list.All(x => x.Counters.Count == 1);
        counters = list
            .SelectMany((x, i) => x.Counters
                .Select((y, j) => new CounterInfo
                {
                    Field = Encoding.ASCII.GetBytes(MakeFieldName(singleCounter, singleInstance, i, j, entry.Name)),
                    Label = Encoding.ASCII.GetBytes(MakeLabelName(x.Counters.Count == 1, j, y, x.Object)),
                    Counter = y,
                    Multiply = x.Object.Multiply
                }))
            .ToArray();

        // Dummy call
        foreach (var counter in counters)
        {
            counter.Counter.NextValue();
        }

        // Debug
#if DEBUG
        System.Diagnostics.Debug.WriteLine($"[{entry.Name}]");
        foreach (var counter in list.SelectMany(x => x.Counters))
        {
            System.Diagnostics.Debug.WriteLine($"Counter: ({counter.CategoryName})({counter.CounterName})({counter.InstanceName})");
        }
#endif
    }

    private static string MakeFieldName(bool singleCounter, bool singleInstance, int counterIndex, int instanceIndex, string name)
    {
        if (singleCounter)
        {
            return singleInstance ? name : $"{name}_{instanceIndex}";
        }

        return singleInstance ? $"{name}_{counterIndex}" : $"{name}_{counterIndex}_{instanceIndex}";
    }

    private static string MakeLabelName(bool singleInstance, int instanceIndex, PerformanceCounter counter, ObjectEntry entry)
    {
        if (String.IsNullOrEmpty(entry.Label))
        {
            return String.IsNullOrEmpty(counter.InstanceName) ? counter.CounterName : counter.InstanceName;
        }

        return singleInstance ? entry.Label : $"{entry.Label} {instanceIndex}";
    }

    public void Dispose()
    {
        foreach (var counter in counters)
        {
            counter.Counter.Dispose();
        }
    }

    private static IEnumerable<PerformanceCounter> Create(string category, string counter, string? instance = null)
    {
        if (!String.IsNullOrEmpty(instance))
        {
            yield return new PerformanceCounter(category, counter, instance);
        }
        else
        {
            var pcc = new PerformanceCounterCategory(category);
            if (pcc.CategoryType == PerformanceCounterCategoryType.SingleInstance)
            {
                yield return new PerformanceCounter(category, counter);
            }
            else
            {
                var names = pcc.GetInstanceNames();
                Array.Sort(names);
                foreach (var name in names)
                {
                    yield return new PerformanceCounter(category, counter, name);
                }
            }
        }
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

        foreach (var counter in counters)
        {
            // TODO custom per field: draw, type, color
            // label
            response.Add(counter.Field);
            response.Add(".label ");
            response.Add(counter.Label);
            response.AddLineFeed();
            // draw
            if (!String.IsNullOrEmpty(entry.GraphDraw))
            {
                response.Add(counter.Field);
                response.Add(".draw ");
                response.Add(entry.GraphDraw);
                response.AddLineFeed();
            }
        }

        response.AddEndLine();
    }

    public void BuildFetch(ResponseBuilder response)
    {
        foreach (var counter in counters)
        {
            // value
            response.Add(counter.Field);
            response.Add(".value ");
            response.Add(counter.Multiply.HasValue
                ? counter.Counter.NextValue() * counter.Multiply.Value
                : counter.Counter.NextValue());
            response.AddLineFeed();
        }

        response.AddEndLine();
    }
}
