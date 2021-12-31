namespace Munin.Node.Plugins.Hardware;

using System.Diagnostics.CodeAnalysis;

public sealed class SensorValue
{
    [AllowNull]
    public static AsyncLocal<SensorValue> Current { get; } = new();

    [AllowNull]
    public float?[] MotherboardControl { get; set; }

    [AllowNull]
    public float?[] MotherboardFan { get; set; }

    [AllowNull]
    public float?[] MotherboardTemperature { get; set; }

    [AllowNull]
    public float?[] MotherboardVoltage { get; set; }

    [AllowNull]
    public float?[] CpuClock { get; set; }

    [AllowNull]
    public float?[] CpuLoad { get; set; }

    [AllowNull]
    public float?[] CpuPower { get; set; }

    [AllowNull]
    public float?[] CpuTemperature { get; set; }

    [AllowNull]
    public float?[] CpuVoltage { get; set; }

    [AllowNull]
    public float?[] MemoryLoad { get; set; }

    [AllowNull]
    public float?[] MemoryData { get; set; }

    [AllowNull]
    public float?[] GpuClock { get; set; }

    [AllowNull]
    public float?[] GpuControl { get; set; }

    [AllowNull]
    public float?[] GpuFan { get; set; }

    [AllowNull]
    public float?[] GpuLoad { get; set; }

    [AllowNull]
    public float?[] GpuPower { get; set; }

    [AllowNull]
    public float?[] GpuTemperature { get; set; }

    [AllowNull]
    public float?[] GpuVoltage { get; set; }

    [AllowNull]
    public float?[] StorageData { get; set; }

    [AllowNull]
    public float?[] StorageLevel { get; set; }

    [AllowNull]
    public float?[] StorageTemperature { get; set; }

    [AllowNull]
    public float?[] StorageThroughput { get; set; }

    [AllowNull]
    public float?[] StorageUsed { get; set; }

    [AllowNull]
    public float?[] NetworkData { get; set; }

    [AllowNull]
    public float?[] NetworkLoad { get; set; }

    [AllowNull]
    public float?[] NetworkThroughput { get; set; }
}
