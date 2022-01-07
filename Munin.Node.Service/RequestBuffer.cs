namespace Munin.Node.Service;

using System;
using System.Buffers;
using System.Runtime.CompilerServices;

internal struct RequestBuffer : IDisposable
{
    private readonly byte[] buffer;

    private int start;

    private int length;

    public bool HasRemaining
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => (buffer.Length - length) > 0;
    }

    public RequestBuffer(int bufferSize)
    {
        buffer = ArrayPool<byte>.Shared.Rent(bufferSize);
        start = 0;
        length = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose()
    {
        ArrayPool<byte>.Shared.Return(buffer);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Memory<byte> GetReceiveMemory() => buffer.AsMemory(length, buffer.Length - length);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetLine(out Memory<byte> line)
    {
        var index = buffer.AsSpan(start, length - start).IndexOf((byte)'\n');
        if (index < 0)
        {
            line = default;
            return false;
        }

        line = buffer.AsMemory(0, start + index + 1);
        return true;
    }

    public void Advance(int size)
    {
        start = length;
        length += size;
    }

    public void Flip(int offset)
    {
        if (offset < length)
        {
            var nextSize = length - offset;
            buffer.AsSpan(offset, nextSize).CopyTo(buffer.AsSpan());
            length = nextSize;
        }
        else
        {
            length = 0;
        }

        start = 0;
    }
}
