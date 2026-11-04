using System.Buffers;

namespace YamlDotNet.System.Text.Json;

/// <summary>
/// A minimal <see cref="IBufferWriter{T}"/> implementation backed by <see cref="ArrayPool{T}"/> shared buffers.
/// </summary>
/// <remarks>Used to stage intermediate JSON payloads without allocating a single large array.</remarks>
internal sealed class PooledBufferWriter : IBufferWriter<byte>, IDisposable
{
    private const int InitialSize = 1024;

    private byte[] _buffer;
    private int _written;
    private bool _disposed;

    public PooledBufferWriter()
    {
        _buffer = ArrayPool<byte>.Shared.Rent(InitialSize);
    }

    public Span<byte> WrittenSpan => _buffer.AsSpan(0, _written);

    public void Advance(int count)
    {
        if (_written + count > _buffer.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(count));
        }

        _written += count;
    }

    public Memory<byte> GetMemory(int sizeHint = 0) => GetBuffer(sizeHint).AsMemory(_written);

    public Span<byte> GetSpan(int sizeHint = 0) => GetBuffer(sizeHint).AsSpan(_written);

    private byte[] GetBuffer(int sizeHint)
    {
#if NETSTANDARD2_0
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(PooledBufferWriter));
        }

        if (sizeHint < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sizeHint));
        }
#else
        ObjectDisposedException.ThrowIf(_disposed, nameof(PooledBufferWriter));
        ArgumentOutOfRangeException.ThrowIfNegative(sizeHint, nameof(sizeHint));
#endif

        if (sizeHint == 0)
        {
            sizeHint = 1;
        }

        if (_written + sizeHint <= _buffer.Length)
        {
            return _buffer;
        }

        var newSize = Math.Max(_written + sizeHint, _buffer.Length * 2);
        var newBuffer = ArrayPool<byte>.Shared.Rent(newSize);

        Buffer.BlockCopy(_buffer, 0, newBuffer, 0, _written);

        ArrayPool<byte>.Shared.Return(_buffer);
        _buffer = newBuffer;

        return _buffer;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            ArrayPool<byte>.Shared.Return(_buffer);
            _buffer = [];
            _written = 0;
            _disposed = true;
        }
    }
}
