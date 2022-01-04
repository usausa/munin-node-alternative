namespace Munin.Node.Plugins.Hardware;

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

using LibreHardwareMonitor.Hardware;

public class FilterEntry
{
    public HardwareType? Type { get; set; }

    public string? Name { get; set; }
}

public sealed class SensorEntry
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
    public string GraphLabel { get; set; }

    [AllowNull]
    public string GraphArgs { get; set; }

    public bool GraphScale { get; set; } = true;
}

public sealed class CustomEntry
{
    public string? Name { get; set; }

    public bool Enable { get; set; }

    [AllowNull]
    public string GraphCategory { get; set; }

    [AllowNull]
    public string GraphTitle { get; set; }

    [AllowNull]
    public string GraphLabel { get; set; }

    [AllowNull]
    public string GraphArgs { get; set; }
}

public sealed class Settings
{
    public SensorEntry[]? Sensor { get; set; }

    public CustomEntry? MemoryLoad { get; set; }
}

public static class SettingsExtensions
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
