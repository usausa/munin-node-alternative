namespace Munin.Node.Plugins.Hardware;

public sealed class HardwareEntry
{
    public bool Enable { get; set; }

    public string[]? Include { get; set; }

    public string[]? Exclude { get; set; }

    public string[]? Order { get; set; }
}

public sealed class Settings
{
    public HardwareEntry? SensorControl { get; set; }

    public HardwareEntry? SensorFan { get; set; }

    public HardwareEntry? SensorTemperature { get; set; }

    public HardwareEntry? SensorVoltage { get; set; }

    public HardwareEntry? CpuClock { get; set; }

    public HardwareEntry? CpuLoad { get; set; }

    public HardwareEntry? CpuPower { get; set; }

    public HardwareEntry? CpuTemperature { get; set; }

    public HardwareEntry? CpuVoltage { get; set; }

    public HardwareEntry? MemoryLoad { get; set; }

    public HardwareEntry? MemoryData { get; set; }

    public HardwareEntry? GpuClock { get; set; }

    public HardwareEntry? GpuControl { get; set; }

    public HardwareEntry? GpuFan { get; set; }

    public HardwareEntry? GpuLoad { get; set; }

    public HardwareEntry? GpuPower { get; set; }

    public HardwareEntry? GpuTemperature { get; set; }

    public HardwareEntry? GpuVoltage { get; set; }

    public HardwareEntry? StorageData { get; set; }

    public HardwareEntry? StorageLevel { get; set; }

    public HardwareEntry? StorageTemperature { get; set; }

    public HardwareEntry? StorageThroughput { get; set; }

    public HardwareEntry? StorageUsed { get; set; }

    public HardwareEntry? NetworkData { get; set; }

    public HardwareEntry? NetworkLoad { get; set; }

    public HardwareEntry? NetworkThroughput { get; set; }
}
