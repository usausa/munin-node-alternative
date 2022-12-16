namespace Munin.Node.Plugins.SensorOmron;

using System.Buffers.Binary;
using System.Diagnostics;
using System.IO.Ports;

internal sealed class SensorRepository
{
    private static readonly byte[] Command =
    {
        0x52, 0x42,         // Header
        0x05, 0x00,         // Length 5
        0x01, 0x21, 0x50,   // Read 0x5021
        0x00, 0x00          // CRC Area
    };

    private readonly object sync = new();

    private readonly string portName;

    private readonly long expire;

    private long lastTick;

    private readonly byte[] buffer = new byte[256];

    private float? temperature;
    private float? humidity;
    private float? light;
    private float? pressure;
    private float? noise;
    private float? discomfort;
    private float? heat;
    private float? etvoc;
    private float? eco2;
    private float? seismic;

    public float? Temperature
    {
        get
        {
            lock (sync)
            {
                return temperature;
            }
        }
    }

    public float? Humidity
    {
        get
        {
            lock (sync)
            {
                return humidity;
            }
        }
    }

    public float? Light
    {
        get
        {
            lock (sync)
            {
                return light;
            }
        }
    }

    public float? Pressure
    {
        get
        {
            lock (sync)
            {
                return pressure;
            }
        }
    }

    public float? Noise
    {
        get
        {
            lock (sync)
            {
                return noise;
            }
        }
    }

    public float? Discomfort
    {
        get
        {
            lock (sync)
            {
                return discomfort;
            }
        }
    }

    public float? Heat
    {
        get
        {
            lock (sync)
            {
                return heat;
            }
        }
    }

    public float? Etvoc
    {
        get
        {
            lock (sync)
            {
                return etvoc;
            }
        }
    }

    public float? Eco2
    {
        get
        {
            lock (sync)
            {
                return eco2;
            }
        }
    }

    public float? Seismic
    {
        get
        {
            lock (sync)
            {
                return seismic;
            }
        }
    }

    static SensorRepository()
    {
        var crc = CalcCrc(Command.AsSpan(0, Command.Length - 2));
        BinaryPrimitives.WriteUInt16LittleEndian(Command.AsSpan(Command.Length - 2, 2), crc);
    }

    public SensorRepository(string portName, long expire)
    {
        this.portName = portName;
        this.expire = expire;
    }

    public void Update()
    {
        lock (sync)
        {
            var timestamp = Stopwatch.GetTimestamp();
            if ((timestamp - lastTick) < expire)
            {
                return;
            }

            try
            {
                using var port = new SerialPort(portName)
                {
                    BaudRate = 115200,
                    DataBits = 8,
                    StopBits = StopBits.One,
                    Parity = Parity.None,
                    Handshake = Handshake.None
                };

                port.ReadTimeout = 5000;

                port.Open();
                port.DiscardOutBuffer();
                port.DiscardInBuffer();

                port.Write(Command, 0, Command.Length);

                var read = 0;
                var length = 4;
                while (true)
                {
                    read += port.Read(buffer, read, length - read);
                    if (read == length)
                    {
                        if (read == 4)
                        {
                            length += BinaryPrimitives.ReadUInt16LittleEndian(buffer.AsSpan(2, 2));
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                temperature = (float)BinaryPrimitives.ReadInt16LittleEndian(buffer.AsSpan(8, 2)) / 100;
                humidity = (float)BinaryPrimitives.ReadInt16LittleEndian(buffer.AsSpan(10, 2)) / 100;
                light = BinaryPrimitives.ReadInt16LittleEndian(buffer.AsSpan(12, 2));
                pressure = (float)BinaryPrimitives.ReadInt32LittleEndian(buffer.AsSpan(14, 4)) / 1000;
                noise = (float)BinaryPrimitives.ReadInt16LittleEndian(buffer.AsSpan(18, 2)) / 100;

                discomfort = (float)BinaryPrimitives.ReadInt16LittleEndian(buffer.AsSpan(24, 2)) / 100;
                heat = (float)BinaryPrimitives.ReadInt16LittleEndian(buffer.AsSpan(26, 2)) / 100;

                etvoc = BinaryPrimitives.ReadInt16LittleEndian(buffer.AsSpan(20, 2));
                eco2 = BinaryPrimitives.ReadInt16LittleEndian(buffer.AsSpan(22, 2));

                seismic = (float)BinaryPrimitives.ReadInt16LittleEndian(buffer.AsSpan(33, 2)) / 1000;

                lastTick = timestamp;
            }
            catch (IOException)
            {
                temperature = null;
                humidity = null;
                light = null;
                pressure = null;
                noise = null;
                discomfort = null;
                heat = null;
                etvoc = null;
                eco2 = null;
                seismic = null;
            }
        }
    }

    private static ushort CalcCrc(Span<byte> span)
    {
        var crc = (ushort)0xFFFF;
        for (var i = 0; i < span.Length; i++)
        {
            crc = (ushort)(crc ^ span[i]);
            for (var j = 0; j < 8; j++)
            {
                var carry = crc & 1;
                if (carry != 0)
                {
                    crc = (ushort)((crc >> 1) ^ 0xA001);
                }
                else
                {
                    crc >>= 1;
                }
            }
        }

        return crc;
    }
}
