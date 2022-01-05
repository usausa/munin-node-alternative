namespace Munin.Node.Plugins.PerformanceCounter;

using System.Diagnostics;
using System.Runtime.Versioning;
using System.Text;

[SupportedOSPlatform("windows")]
public sealed class PerformanceCounterPlugin : IPlugin, IDisposable
{
    private readonly PerformanceCounterEntry entry;

    private readonly PerformanceCounter[] counters;

    public byte[] Name { get; }

    public PerformanceCounterPlugin(PerformanceCounterEntry entry)
    {
        this.entry = entry;
        Name = Encoding.ASCII.GetBytes(entry.Name);

        counters = entry.Object
            .SelectMany(x => Create(x.Category, x.Counter, x.Instance))
            .ToArray();
        foreach (var counter in counters)
        {
            counter.NextValue();
        }
    }

    public void Dispose()
    {
        foreach (var counter in counters)
        {
            counter.Dispose();
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

        for (var i = 0; i < counters.Length; i++)
        {
            var counter = counters[i];
            // label
            // TODO field
            buffer.Add(Name);
            buffer.Add(i);
            buffer.Add(".label ");
            // TODO name
            if (String.IsNullOrEmpty(counter.InstanceName))
            {
                buffer.Add(i);
            }
            else
            {
                buffer.Add(counter.InstanceName);
            }
            // draw
            if (!String.IsNullOrEmpty(entry.GraphDraw))
            {
                // TODO field
                buffer.Add(Name);
                buffer.Add(i);
                buffer.Add(".draw ");
                buffer.Add(entry.GraphDraw);    // TODO custom
                buffer.AddLineFeed();
            }
            // TODO type
            // TODO color
            // TODO critical/warning
        }

        buffer.AddEndLine();
    }

    public void BuildFetch(BufferSegment buffer)
    {
        for (var i = 0; i < counters.Length; i++)
        {
            var counter = counters[i];
            // value
            // TODO field
            buffer.Add(Name);
            buffer.Add(i);
            buffer.Add(".value ");
            // TODO name
            buffer.Add(counter.NextValue());
        }

        buffer.AddEndLine();
    }
}
