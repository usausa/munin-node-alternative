namespace Munin.Node.Plugins.PerformanceCounter;

internal sealed class ObjectEntry
{
    public string Category { get; set; } = default!;

    public string Counter { get; set; } = default!;

    public string? Instance { get; set; }

    public float? Multiply { get; set; }

    public string? Label { get; set; }
}

internal sealed class PerformanceCounterEntry
{
    public string Name { get; set; } = default!;

    public ObjectEntry[] Object { get; set; } = default!;

    public string GraphCategory { get; set; } = default!;

    public string GraphTitle { get; set; } = default!;

    public string GraphVLabel { get; set; } = default!;

    public string GraphArgs { get; set; } = default!;

    public bool? GraphScale { get; set; }

    public string? GraphDraw { get; set; }

    public string? GraphType { get; set; }
}

internal sealed class Settings
{
    public PerformanceCounterEntry[]? Counter { get; set; }
}
