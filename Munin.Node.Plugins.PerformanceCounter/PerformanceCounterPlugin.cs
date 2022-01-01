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

        // TODO 複数対応
        //Dump("cpu", Prepare("Processor", "% Processor Time", "_Total"));
        //// Memory Usage, GAUGE, AREA, -l 0 -u 100
        //Dump("memory", Prepare("Memory", "% Committed Bytes In Use"));
        //// Disk Usage(逆)
        //Dump("disk", Prepare("LogicalDisk", "% Free Space"));
        //// Uptime, GAUGE, AREA, -b 1000 -l 0
        //Dump("uptime", Prepare("System", "System Up Time"), Single.Parse("1.1574074074074073e-005"));
        //// Processor Queue Length, system, --base 1000 -l 0, -base 1000 -l 0, LINE
        //Dump("processorqueue", Prepare("System", "Processor Queue Length"));
        //// Disk Time, disk, --base 1000 -l 0, LINE
        //Dump("disktime", Prepare("LogicalDisk", "% Disk Time"));
        //// Disk Queue Length, disk, --base 1000 -l 0, LINE
        //Dump("diskqueue", Prepare("PhysicalDisk", "Current Disk Queue Length"));
        //// Ex
        //Dump("memory_page", Prepare("Memory", "Pages/sec"));
        //Dump("handle", Prepare("Process V2", "Handle Count", "_Total"));
        //Dump("process", Prepare("System", "Processes"));
        //Dump("thread", Prepare("Process V2", "Thread Count", "_Total"));
        //Dump("tcp4_connections_established", Prepare("TCPv4", "Connections Established"));

        // TODO 一度ダミーNextValue()の必要あり

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

    //private static PerformanceCounter[] Create(string categoryName, string counterName, string? instanceName = null)
    //{
    //    if (!String.IsNullOrEmpty(instanceName))
    //    {
    //        return new[] { new PerformanceCounter(categoryName, counterName, instanceName) };
    //    }

    //    var pcc = new PerformanceCounterCategory(categoryName);
    //    if (pcc.CategoryType == PerformanceCounterCategoryType.SingleInstance)
    //    {
    //        return new[] { new PerformanceCounter(categoryName, counterName) };
    //    }

    //    var instanceNames = pcc.GetInstanceNames().OrderBy(x => x).ToArray();
    //    return instanceNames.Select(x => new PerformanceCounter(categoryName, counterName, x)).ToArray();
    //}

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
