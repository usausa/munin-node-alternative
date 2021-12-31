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

        // TODO
        counters = Array.Empty<PerformanceCounter>();
    }

    public void Dispose()
    {
        foreach (var counter in counters)
        {
            counter.Dispose();
        }
    }

    public void BuildConfig(BufferSegment buffer)
    {
        // graph_title
        buffer.Add("graph_title ");
        buffer.Add(entry.GraphTitle);
        buffer.AddLineFeed();
        // graph_category
        buffer.Add("graph_category ");
        buffer.Add(entry.GraphCategory);
        buffer.AddLineFeed();
        // graph_category
        buffer.Add("graph_args ");
        buffer.Add(entry.GraphArgs);
        buffer.AddLineFeed();
        // graph_vlabel
        // TODO

        // label
        // TODO
        buffer.Add(Name);
        buffer.Add(".label ");
        buffer.Add("TODO");
        buffer.AddLineFeed();
        // draw
        // TODO
        buffer.Add(Name);
        buffer.Add(".draw ");
        buffer.Add(entry.GraphDraw);
        buffer.AddLineFeed();
        // type
        //buffer.Add(Name);
        //buffer.Add(".type ");
        //buffer.Add("TODO");
        //buffer.AddLineFeed();
        // info
        //buffer.Add(Name);
        //buffer.Add(".info ");
        //buffer.Add("TODO");
        //buffer.AddLineFeed();

        buffer.AddEndLine();
    }

    public void BuildFetch(BufferSegment buffer)
    {
        buffer.Add(Name);
        buffer.Add(".value ");
        buffer.Add(" 100");
        buffer.AddLineFeed();

        buffer.AddEndLine();
    }
}
