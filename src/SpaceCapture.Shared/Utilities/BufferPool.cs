// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: MIT

using System.Buffers;

namespace SpaceCapture.Shared.Utilities;

/// <summary>
/// Utility to rent buffers from <see cref="ArrayPool{T}.Shared"/>.
/// </summary>
public static class BufferPool
{
    /// <summary>
    /// Rent a buffer of the requested size.
    /// </summary>
    /// <remarks>
    /// The returned lease should be disposed through <see langword="using"/>.
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    /// <param name="size"></param>
    /// <param name="clearOnReturn"></param>
    /// <returns></returns>
    public static BufferLease<T> Rent<T>(int size, bool clearOnReturn = false) =>
        new(ArrayPool<T>.Shared, size, clearOnReturn);
}

/// <summary>
/// Disposable lease from <see cref="ArrayPool{T}.Shared"/>.
/// </summary>
/// <typeparam name="T"></typeparam>
public ref struct BufferLease<T> : IDisposable
{
    readonly ArrayPool<T> _pool;
    T[]? _buffer;
    readonly bool _clearOnReturn;
    readonly Memory<T> _memory;

    /// <inheritdoc cref="Span{T}.this"/>
    public readonly ref T this[int index] => ref Span[index];

    /// <summary>
    /// Buffer as <see cref="Span{T}"/>.
    /// </summary>
    public readonly Span<T> Span => _memory.Span;

    /// <summary>
    /// Buffer as <see cref="Memory{T}"/>.
    /// </summary>
    public readonly Memory<T> Memory => _memory;

    /// <summary>
    /// Buffer as an array of <typeparamref name="T"/>.
    /// </summary>
    /// <remarks>
    /// This array may be larger than the original requested size.
    /// </remarks>
    public readonly T[] Array => _buffer!;

    public BufferLease(ArrayPool<T> pool, int size, bool clearOnReturn)
    {
        _pool = pool;
        _buffer = _pool.Rent(size);
        _clearOnReturn = clearOnReturn;
        _memory = _buffer.AsMemory(0, size);
    }

    public void Dispose()
    {
        if (_buffer is null)
            throw new ObjectDisposedException(nameof(BufferLease<T>));

        _pool.Return(_buffer, _clearOnReturn);
        _buffer = null;
    }

    public static implicit operator Span<T>(BufferLease<T> lease) => lease.Span;

    public static implicit operator ReadOnlySpan<T>(BufferLease<T> lease) => lease.Span;

    public static implicit operator Memory<T>(BufferLease<T> lease) => lease.Memory;

    public static implicit operator ReadOnlyMemory<T>(BufferLease<T> lease) => lease.Memory;

    public static implicit operator T[](BufferLease<T> lease) => lease.Array;
}
