namespace Munin.Node.Plugins.SensorOmron;

using System.Text;

internal sealed class SensorField
{
    public string Field { get; }

    public string Label { get; }

    public Func<SensorRepository, float?> Resolver { get; }

    public SensorField(string field, string label, Func<SensorRepository, float?> resolver)
    {
        Field = field;
        Label = label;
        Resolver = resolver;
    }
}

internal sealed class SeismicPlugin : IPlugin
{
    private readonly SensorRepository repository;

    private readonly string category;

    private readonly string title;

    private readonly string vlavel;

    private readonly string graph;

    private readonly bool? scale;

    private readonly SensorField[] fields;

    public byte[] Name { get; }

    public SeismicPlugin(
        SensorRepository repository,
        string name,
        string category,
        string title,
        string vlavel,
        string graph,
        bool? scale,
        SensorField[] fields)
    {
        this.repository = repository;
        Name = Encoding.ASCII.GetBytes(name);
        this.category = category;
        this.title = title;
        this.vlavel = vlavel;
        this.graph = graph;
        this.scale = scale;
        this.fields = fields;
    }

    public void BuildConfig(ResponseBuilder response)
    {
        // graph_category
        response.Add("graph_category ");
        response.Add(category);
        response.AddLineFeed();
        // graph_title
        response.Add("graph_title ");
        response.Add(title);
        response.AddLineFeed();
        // graph_vlabel
        response.Add("graph_vlabel ");
        response.Add(vlavel);
        response.AddLineFeed();
        // graph_category
        response.Add("graph_args ");
        response.Add(graph);
        response.AddLineFeed();
        // graph_scale
        if (scale.HasValue)
        {
            response.Add("graph_scale ");
            response.Add(scale.Value ? "yes" : "no");
            response.AddLineFeed();
        }
        foreach (var field in fields)
        {
            // label
            response.Add(field.Field);
            response.Add(".label ");
            response.Add(field.Label);

            response.AddLineFeed();
        }

        response.AddEndLine();
    }

    public void BuildFetch(ResponseBuilder response)
    {
        repository.Update();

        foreach (var field in fields)
        {
            // value
            var value = field.Resolver(repository);
            if (value.HasValue)
            {
                response.Add(field.Field);
                response.Add(".value ");
                response.Add(value.Value);
            }

            response.AddLineFeed();
        }

        response.AddEndLine();
    }
}
