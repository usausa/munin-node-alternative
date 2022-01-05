namespace Munin.Node.Plugins.Hardware;

using System.Text;

using LibreHardwareMonitor.Hardware;

internal static class NameExtensions
{
    private static readonly byte[] Motherboard = Convert(HardwareType.Motherboard);
    private static readonly byte[] SuperIO = Convert(HardwareType.SuperIO);
    private static readonly byte[] Cpu = Convert(HardwareType.Cpu);
    private static readonly byte[] Memory = Convert(HardwareType.Memory);
    private static readonly byte[] GpuNvidia = Convert(HardwareType.GpuNvidia);
    private static readonly byte[] GpuAmd = Convert(HardwareType.GpuAmd);
    private static readonly byte[] Storage = Convert(HardwareType.Storage);
    private static readonly byte[] Network = Convert(HardwareType.Network);
    private static readonly byte[] Cooler = Convert(HardwareType.Cooler);
    private static readonly byte[] EmbeddedController = Convert(HardwareType.EmbeddedController);
    private static readonly byte[] Psu = Convert(HardwareType.Psu);

    private static readonly byte[] Voltage = Convert(SensorType.Voltage);
    private static readonly byte[] Current = Convert(SensorType.Current);
    private static readonly byte[] Power = Convert(SensorType.Power);
    private static readonly byte[] Clock = Convert(SensorType.Clock);
    private static readonly byte[] Temperature = Convert(SensorType.Temperature);
    private static readonly byte[] Load = Convert(SensorType.Load);
    private static readonly byte[] Frequency = Convert(SensorType.Frequency);
    private static readonly byte[] Fan = Convert(SensorType.Fan);
    private static readonly byte[] Flow = Convert(SensorType.Flow);
    private static readonly byte[] Control = Convert(SensorType.Control);
    private static readonly byte[] Level = Convert(SensorType.Level);
    private static readonly byte[] Factor = Convert(SensorType.Factor);
    private static readonly byte[] Data = Convert(SensorType.Data);
    private static readonly byte[] SmallData = Convert(SensorType.SmallData);
    private static readonly byte[] Throughput = Convert(SensorType.Throughput);
    private static readonly byte[] TimeSpan = Convert(SensorType.TimeSpan);

#pragma warning disable CA1308
    private static byte[] Convert<T>(T value)
        where T : Enum
    {
        return Encoding.ASCII.GetBytes(value.ToString().ToLowerInvariant());
    }
#pragma warning restore CA1308

    public static byte[] ToNameBytes(this HardwareType type)
    {
        return type switch
        {
            HardwareType.Motherboard => Motherboard,
            HardwareType.SuperIO => SuperIO,
            HardwareType.Cpu => Cpu,
            HardwareType.Memory => Memory,
            HardwareType.GpuNvidia => GpuNvidia,
            HardwareType.GpuAmd => GpuAmd,
            HardwareType.Storage => Storage,
            HardwareType.Network => Network,
            HardwareType.Cooler => Cooler,
            HardwareType.EmbeddedController => EmbeddedController,
            HardwareType.Psu => Psu,
            _ => throw new ArgumentException("Unsupported type.")
        };
    }

    public static byte[] ToNameBytes(this SensorType type)
    {
        return type switch
        {
            SensorType.Voltage => Voltage,
            SensorType.Current => Current,
            SensorType.Power => Power,
            SensorType.Clock => Clock,
            SensorType.Temperature => Temperature,
            SensorType.Load => Load,
            SensorType.Frequency => Frequency,
            SensorType.Fan => Fan,
            SensorType.Flow => Flow,
            SensorType.Control => Control,
            SensorType.Level => Level,
            SensorType.Factor => Factor,
            SensorType.Data => Data,
            SensorType.SmallData => SmallData,
            SensorType.Throughput => Throughput,
            SensorType.TimeSpan => TimeSpan,
            _ => throw new ArgumentException("Unsupported type.")
        };
    }
}
