namespace Munin.Node.Plugins.PerformanceCounter;

using System.Diagnostics.CodeAnalysis;

internal sealed class ObjectEntry
{
    [AllowNull]
    public string Category { get; set; }

    [AllowNull]
    public string Counter { get; set; }

    public string? Instance { get; set; }

    public float? Multiply { get; set; }

    public string? Label { get; set; }
}

internal sealed class PerformanceCounterEntry
{
    [AllowNull]
    public string Name { get; set; }

    [AllowNull]
    public ObjectEntry[] Object { get; set; }

    [AllowNull]
    public string GraphCategory { get; set; }

    [AllowNull]
    public string GraphTitle { get; set; }

    [AllowNull]
    public string GraphVLabel { get; set; }

    [AllowNull]
    public string GraphArgs { get; set; }

    public bool? GraphScale { get; set; }

    public string? GraphDraw { get; set; }
}

internal sealed class Settings
{
    public PerformanceCounterEntry[]? Counter { get; set; }
}
