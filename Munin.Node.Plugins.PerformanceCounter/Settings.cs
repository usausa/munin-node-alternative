namespace Munin.Node.Plugins.PerformanceCounter;

using System.Diagnostics.CodeAnalysis;

public sealed class PerformanceCounterEntry
{
    [AllowNull]
    public string Name { get; set; }

    [AllowNull]
    public string Object { get; set; }

    [AllowNull]
    public string Counter { get; set; }

    public string? Instance { get; set; }

    public float? CounterMultiply { get; set; } = 1f;

    [AllowNull]
    public string GraphCategory { get; set; }

    [AllowNull]
    public string GraphTitle { get; set; }

    [AllowNull]
    public string GraphArgs { get; set; }

    [AllowNull]
    public string GraphDraw { get; set; }
}

public sealed class Settings
{
    public PerformanceCounterEntry[]? Counter { get; set; }
}
