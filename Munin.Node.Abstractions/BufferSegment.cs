namespace Munin.Node;

using System.Buffers;
using System.Buffers.Text;
using System.Runtime.CompilerServices;
using System.Text;

// TODO Grow support ?
public sealed class BufferSegment : IDisposable
{
    public byte[] Buffer { get; }

    public int Length { get; set; }

    public int Remaining
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Buffer.Length - Length;
    }

    public bool IsEmpty
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Length == 0;
    }

    public BufferSegment(int bufferSize)
    {
        Buffer = ArrayPool<byte>.Shared.Rent(bufferSize);
        Length = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose()
    {
        ArrayPool<byte>.Shared.Return(Buffer);
    }

    public void Clear() => Length = 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(ReadOnlySpan<byte> values)
    {
        values.CopyTo(Buffer.AsSpan(Length));
        Length += values.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(ReadOnlySpan<char> values)
    {
        var length = Encoding.ASCII.GetBytes(values, Buffer.AsSpan(Length));
        if (length > 0)
        {
            Length += length;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(byte value)
    {
        Buffer[Length++] = value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(int value)
    {
        if (Utf8Formatter.TryFormat(value, Buffer.AsSpan(Length), out var written))
        {
            Length += written;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(float value)
    {
        if (Utf8Formatter.TryFormat(value, Buffer.AsSpan(Length), out var written))
        {
            Length += written;
        }
    }
}

public static class BufferSegmentExtensions
{
    public static void AddLineFeed(this BufferSegment buffer)
    {
        buffer.Add((byte)'\n');
    }

    public static void AddEndLine(this BufferSegment buffer)
    {
        buffer.Add((byte)'.');
        buffer.Add((byte)'\n');
    }
}
