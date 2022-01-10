namespace Munin.Node.Plugins.Hardware;

using System.Diagnostics.CodeAnalysis;

using LibreHardwareMonitor.Hardware;

internal class FilterEntry
{
    public HardwareType? Type { get; set; }

    public string? Name { get; set; }
}

internal sealed class SensorEntry
{
    [AllowNull]
    public string Name { get; set; }

    public HardwareType[]? Hardware { get; set; }

    public SensorType Sensor { get; set; }

    public FilterEntry[]? Include { get; set; }

    public FilterEntry[]? Exclude { get; set; }

    public FilterEntry[]? Order { get; set; }

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

    public string? GraphType { get; set; }

    public float? Multiply { get; set; }
}

internal sealed class CustomEntry
{
    public string? Name { get; set; }

    public bool Enable { get; set; }

    public string? GraphCategory { get; set; }

    public string? GraphTitle { get; set; }

    public string? GraphVLabel { get; set; }

    public string? GraphArgs { get; set; }

    public bool? GraphScale { get; set; }
}

internal sealed class Settings
{
    public int Expire { get; set; } = 30000;

    public SensorEntry[]? Sensor { get; set; }

    public CustomEntry? Memory { get; set; }
}

internal static class SettingsExtensions
{
    public static bool IsHardwareEnable([NotNullWhen(true)] this SensorEntry[]? sensors, HardwareType type)
    {
        if (sensors is null)
        {
            return false;
        }

        foreach (var sensor in sensors)
        {
            if ((sensor.Hardware is null) || sensor.Hardware.Any(x => x == type))
            {
                return true;
            }
        }

        return false;
    }

    public static bool IsEnable([NotNullWhen(true)] this CustomEntry? entry) => entry?.Enable ?? false;
}
