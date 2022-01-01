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

    [JsonConverter(typeof(ComplexHardwareConverter))]
    public Hardware[]? Hardware { get; set; }

    public SensorType Sensor { get; set; }

    [JsonConverter(typeof(ComplexFilterConverter))]
    public object[]? Include { get; set; }

    [JsonConverter(typeof(ComplexFilterConverter))]
    public object[]? Exclude { get; set; }

    [JsonConverter(typeof(ComplexFilterConverter))]
    public object[]? Order { get; set; }
}

public sealed class CustomEntry
{
    public string? Name { get; set; }

    public bool Enable { get; set; }
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
            if ((sensor.Hardware is null) ||
                sensor.Hardware.Any(hardware => hardware.HardwareType == type))
            {
                return true;
            }
        }

        return false;
    }

    public static bool IsEnable([NotNullWhen(true)] this CustomEntry? entry) => entry?.Enable ?? false;
}
