namespace Munin.Node;

using System.Buffers;
using System.Buffers.Text;
using System.Runtime.CompilerServices;
using System.Text;

public sealed class ResponseBuilder : IDisposable
{
    private static readonly Encoding Ascii = Encoding.ASCII;

    private static readonly byte[] Eol = [(byte)'.', (byte)'\n'];

    private byte[] buffer;

    private int length;

    public bool IsEmpty
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => length == 0;
    }

    public ResponseBuilder(int bufferSize)
    {
        buffer = ArrayPool<byte>.Shared.Rent(bufferSize);
        length = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose()
    {
        ArrayPool<byte>.Shared.Return(buffer);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear() => length = 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlyMemory<byte> AsSendMemory() => new(buffer, 0, length);

    private void Grow(int size)
    {
        var newSize = buffer.Length;
        do
        {
            newSize *= 2;
        }
        while (newSize < length + size);

        var newBuffer = ArrayPool<byte>.Shared.Rent(newSize);
        buffer.CopyTo(newBuffer, 0);
        ArrayPool<byte>.Shared.Return(buffer);
        buffer = newBuffer;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(ReadOnlySpan<byte> value)
    {
        if ((buffer.Length - length) < value.Length)
        {
            Grow(value.Length);
        }

        value.CopyTo(buffer.AsSpan(length));
        length += value.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(ReadOnlySpan<char> value)
    {
        if ((buffer.Length - length) < value.Length)
        {
            Grow(value.Length);
        }

        length += Ascii.GetBytes(value, buffer.AsSpan(length));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(byte value)
    {
        if ((buffer.Length - length) < 1)
        {
            Grow(1);
        }

        buffer[length++] = value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(int value)
    {
        if (!Utf8Formatter.TryFormat(value, buffer.AsSpan(length), out var written))
        {
            Grow(written);
            if (!Utf8Formatter.TryFormat(value, buffer.AsSpan(length), out written))
            {
                throw new InvalidOperationException();
            }
        }

        length += written;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(float value)
    {
        if (!Utf8Formatter.TryFormat(value, buffer.AsSpan(length), out var written))
        {
            Grow(written);
            if (!Utf8Formatter.TryFormat(value, buffer.AsSpan(length), out written))
            {
                throw new InvalidOperationException();
            }
        }

        length += written;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddLineFeed()
    {
        Add((byte)'\n');
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddEndLine()
    {
        Add(Eol);
    }
}
