namespace Munin.Node;

using System.Buffers;
using System.Text;

public sealed class BufferSegment : IDisposable
{
    public byte[] Buffer { get; }

    public int Length { get; set; }

    public int Remaining => Buffer.Length - Length;

    public bool IsEmpty => Length == 0;

    public BufferSegment(int bufferSize)
    {
        Buffer = ArrayPool<byte>.Shared.Rent(bufferSize);
        Length = 0;
    }

    public void Dispose()
    {
        ArrayPool<byte>.Shared.Return(Buffer);
    }

    public void Clear() => Length = 0;

    public void Add(ReadOnlySpan<byte> values)
    {
        values.CopyTo(Buffer.AsSpan(Length));
        Length += values.Length;
    }

    public void Add(ReadOnlySpan<char> values)
    {
        var length = Encoding.ASCII.GetBytes(values, Buffer.AsSpan(Length));
        if (length > 0)
        {
            Length += length;
        }
    }

    public void AddSpace()
    {
        Buffer[Length++] = Bytes.Space;
    }

    public void AddLineFeed()
    {
        Buffer[Length++] = Bytes.LineFeed;
    }

    public void AddEndLine()
    {
        Buffer[Length++] = Bytes.Dot;
        Buffer[Length++] = Bytes.LineFeed;
    }
}
