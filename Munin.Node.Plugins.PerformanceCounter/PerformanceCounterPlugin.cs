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
        var isSingle = list.All(x => x.Counters.Count == 1);
        if (list.Count == 1)
        {
            // TODO label
            counters = list[0].Counters
                .Select((x, i) => new CounterInfo
                {
                    Field = isSingle ? Name : Encoding.ASCII.GetBytes($"{entry.Name}_{i}"),
                    Label = Encoding.ASCII.GetBytes(String.IsNullOrEmpty(x.InstanceName) ? $"{i}" : x.InstanceName),
                    Counter = x,
                    Multiply = list[0].Object.Multiply
                })
                .ToArray();
        }
        else
        {
            // TODO label
            counters = list
                .SelectMany((x, i) => x.Counters
                    .Select((y, j) => new CounterInfo
                    {
                        Field = Encoding.ASCII.GetBytes(isSingle ? $"{entry.Name}_{i}" : $"{entry.Name}_{i}_{j}"),
                        Label = Encoding.ASCII.GetBytes(String.IsNullOrEmpty(y.InstanceName) ? (isSingle ? $"{i}" : $"{i}_{j}") : y.InstanceName),
                        Counter = y,
                        Multiply = x.Object.Multiply
                    }))
                .ToArray();
        }

        // Dummy
        foreach (var counter in counters)
        {
            counter.Counter.NextValue();
        }

        // Debug
#if DEBUG
        Debug.WriteLine($"[{entry.Name}]");
        foreach (var item in list)
        {
            foreach (var counter in item.Counters)
            {
                Debug.WriteLine($"Counter: ({counter.CategoryName})({counter.CounterName})({counter.InstanceName})");
            }
        }
#endif
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
                foreach (var name in pcc.GetInstanceNames())
                {
                    yield return new PerformanceCounter(category, counter, name);
                }
            }
        }
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

        foreach (var counter in counters)
        {
            // label
            buffer.Add(counter.Field);
            buffer.Add(".label ");
            buffer.Add(counter.Label);
            buffer.AddLineFeed();
            // draw
            if (!String.IsNullOrEmpty(entry.GraphDraw))
            {
                buffer.Add(counter.Field);
                buffer.Add(".draw ");
                buffer.Add(entry.GraphDraw);    // TODO custom
                buffer.AddLineFeed();
            }
        }

        buffer.AddEndLine();
    }

    public void BuildFetch(BufferSegment buffer)
    {
        foreach (var counter in counters)
        {
            // value
            buffer.Add(counter.Field);
            buffer.Add(".value ");
            buffer.Add(counter.Multiply.HasValue
                ? counter.Counter.NextValue() * counter.Multiply.Value
                : counter.Counter.NextValue());
            buffer.AddLineFeed();
        }

        buffer.AddEndLine();
    }
}
