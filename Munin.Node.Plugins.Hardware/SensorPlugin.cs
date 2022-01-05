namespace Munin.Node.Plugins.Hardware;

using System.Text;

public sealed class SensorPlugin : IPlugin
{
    private readonly SensorEntry entry;

    public byte[] Name { get; }

    public SensorPlugin(SensorEntry entry)
    {
        Name = Encoding.ASCII.GetBytes(entry.Name);
        this.entry = entry;
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

        var subset = SensorValuePool.Default.Rent();
        subset.Clear();
        SensorValueHelper.Filter(HardwareContext.Snapshot.Value!, subset, entry);

        for (var i = 0; i < subset.Count; i++)
        {
            var value = subset[i];
            // label
            buffer.Add(value.HardwareType.ToString()); // TODO field
            buffer.Add("_");
            buffer.Add(value.SensorType.ToString());
            buffer.Add("_");
            buffer.Add(value.Index);
            buffer.Add(".label ");
            buffer.Add(value.SensorName);   // TODO name
            buffer.AddLineFeed();
            // draw
            if (!String.IsNullOrEmpty(entry.GraphDraw))
            {
                buffer.Add(value.HardwareType.ToString()); // TODO field
                buffer.Add("_");
                buffer.Add(value.SensorType.ToString());
                buffer.Add("_");
                buffer.Add(value.Index);
                buffer.Add(".draw ");
                buffer.Add(entry.GraphDraw);    // TODO custom
                buffer.AddLineFeed();
            }
            // TODO type
            // TODO color
            // TODO critical/warning
        }

        SensorValuePool.Default.Return(subset);

        buffer.AddEndLine();
    }

    public void BuildFetch(BufferSegment buffer)
    {
        var subset = SensorValuePool.Default.Rent();
        subset.Clear();
        SensorValueHelper.Filter(HardwareContext.Snapshot.Value!, subset, entry);

        for (var i = 0; i < subset.Count; i++)
        {
            var value = subset[i];
            if (value.Value.HasValue)
            {
                // value
                buffer.Add(value.HardwareType.ToString()); // TODO field
                buffer.Add("_");
                buffer.Add(value.SensorType.ToString());
                buffer.Add("_");
                buffer.Add(value.Index);
                buffer.Add(".value ");
                buffer.Add(value.Value.Value);
                buffer.AddLineFeed();
            }
        }

        SensorValuePool.Default.Return(subset);

        buffer.AddEndLine();
    }
}
