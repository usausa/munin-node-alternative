namespace Munin.Node.Plugins.Hardware;

using System.Buffers;
using System.Runtime.CompilerServices;

using LibreHardwareMonitor.Hardware;

public static class SensorValueHelper
{
    public static void Update(Computer computer)
    {
        for (var i = 0; i < computer.Hardware.Count; i++)
        {
            Update(computer.Hardware[i]);
        }
    }

    private static void Update(IHardware hardware)
    {
        hardware.Update();
        for (var i = 0; i < hardware.SubHardware.Length; i++)
        {
            Update(hardware.SubHardware[i]);
        }
    }

    public static void Gather(Computer computer, List<SensorValue> values)
    {
        for (var i = 0; i < computer.Hardware.Count; i++)
        {
            GatherValues(computer.Hardware[i], values);
        }
    }

    private static void GatherValues(IHardware hardware, List<SensorValue> values)
    {
        for (var i = 0; i < hardware.SubHardware.Length; i++)
        {
            GatherValues(hardware.SubHardware[i], values);
        }

        for (var i = 0; i < hardware.Sensors.Length; i++)
        {
            var sensor = hardware.Sensors[i];
            values.Add(new SensorValue
            {
                HardwareType = hardware.HardwareType,
                SensorType = sensor.SensorType,
                Index = sensor.Index,
                HardwareName = hardware.Name,
                SensorName = sensor.Name,
                Value = sensor.Value
            });
        }
    }

    public static void Filter(List<SensorValue> source, List<SensorValue> destination, SensorEntry entry)
    {
        for (var i = 0; i < source.Count; i++)
        {
            var value = source[i];
            if (value.SensorType != entry.Sensor)
            {
                continue;
            }

            if ((entry.Hardware?.Length > 0) && (Array.IndexOf(entry.Hardware, value.HardwareType) < 0))
            {
                continue;
            }

            if ((entry.Include?.Length > 0) && !IsMatch(value, entry.Include))
            {
                continue;
            }

            if ((entry.Exclude?.Length > 0) && IsMatch(value, entry.Exclude))
            {
                continue;
            }

            destination.Add(value);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsMatch(SensorValue value, FilterEntry[] filters)
    {
        for (var i = 0; i < filters.Length; i++)
        {
            var filter = filters[i];
            if ((!filter.Type.HasValue || (filter.Type == value.HardwareType)) &&
                (String.IsNullOrEmpty(filter.Name) || (filter.Name == value.SensorName)))
            {
                return true;
            }
        }

        return false;
    }

    public static unsafe void Order(List<SensorValue> source, List<SensorValue> destination, FilterEntry[] orders)
    {
        using var processed = source.Count > 2048
            ? new ValueBitMap(source.Count / 8)
            : new ValueBitMap(stackalloc byte[source.Count / 8]);

        for (var i = 0; i < source.Count; i++)
        {
            var value = source[i];
            if (IsMatch(value, orders))
            {
                destination.Add(value);
                processed.Set(i);
            }
        }

        for (var i = 0; i < source.Count; i++)
        {
            if (!processed.Get(i))
            {
                destination.Add(source[i]);
            }
        }
    }

    public ref struct ValueBitMap
    {
        private readonly byte[]? arrayReturnToPool;

        private readonly Span<byte> buffer;

        public ValueBitMap(Span<byte> buffer)
        {
            arrayReturnToPool = null;
            this.buffer = buffer;
        }

        public ValueBitMap(int capacity)
        {
            arrayReturnToPool = ArrayPool<byte>.Shared.Rent(capacity);
            buffer = arrayReturnToPool;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            var toReturn = arrayReturnToPool;
            this = default;
            if (toReturn != null)
            {
                ArrayPool<byte>.Shared.Return(toReturn);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(int index)
        {
            var i = index / 8;
            buffer[i] = (byte)(buffer[i] | (1 << (index % 8)));
        }

        public bool Get(int index)
        {
            var i = index / 8;
            return (buffer[i] & (1 << (index % 8))) > 0;
        }
    }
}
