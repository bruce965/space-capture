// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: MIT

using System.Buffers;
using System.Buffers.Text;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SpaceCapture.Shared.Utilities;

/// <summary>
/// Deterministic random numbers generator.
/// </summary>
[JsonConverter(typeof(JsonConverter))]
public struct DeterministicRandom
    : IEquatable<DeterministicRandom>,
        IParsable<DeterministicRandom>,
        IUtf8SpanParsable<DeterministicRandom>,
        IUtf8SpanFormattable
{
    [InlineArray(4)]
    internal struct UInt32Buffer4
    {
        uint _element;
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public class JsonConverter : JsonConverter<DeterministicRandom>
    {
        public override DeterministicRandom Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options
        )
        {
            Span<byte> buffer = stackalloc byte[22];
            int length = reader.CopyString(buffer);
            return Parse(buffer[..length], null);
        }

        public override void Write(Utf8JsonWriter writer, DeterministicRandom value, JsonSerializerOptions options)
        {
            Span<byte> buffer = stackalloc byte[22];
            bool ok = value.TryFormat(buffer, out int length, "", null);
            Debug.Assert(ok);
            writer.WriteStringValue(buffer[..length]);
        }
    }

    UInt32Buffer4 _buffer;

    /// <summary>
    /// Generate a new random seed.
    /// </summary>
    /// <returns></returns>
    public DeterministicRandom()
        : this(default)
    {
        Random.Shared.NextBytes(MemoryMarshal.AsBytes<uint>(_buffer));
    }

    internal DeterministicRandom(UInt32Buffer4 buffer) => _buffer = buffer;

    internal DeterministicRandom(uint a, uint b, uint c, uint d)
    {
        UInt32Buffer4 buffer = new();
        buffer[0] = a;
        buffer[1] = b;
        buffer[2] = c;
        buffer[3] = d;
        this = new(buffer);
    }

    /// <summary>
    /// Get the next deterministic random number between <see cref="int.MinValue"/> and <see cref="int.MaxValue"/>.
    /// </summary>
    /// <param name="rand"></param>
    /// <returns></returns>
    int NextRaw()
    {
        uint x = _buffer[0];
        uint w = _buffer[3];

        uint t = x ^ (x << 11);
        uint v = w ^ (w >> 19) ^ t ^ (t >> 8);

        _buffer[0] = _buffer[1];
        _buffer[1] = _buffer[2];
        _buffer[2] = w;
        _buffer[3] = v;

        return (int)v;
    }

    /// <inheritdoc cref="Random.NextBytes(Span{byte})"/>
    public void NextBytes(Span<byte> buffer)
    {
        Span<int> randInt32 = stackalloc int[1];
        ReadOnlySpan<byte> randBytes = MemoryMarshal.AsBytes(randInt32);

        for (int i = 0; i < buffer.Length; i += sizeof(int))
        {
            randInt32[0] = NextRaw();
            randBytes[0..Math.Min(sizeof(int), buffer.Length - i)].CopyTo(buffer[i..]);
        }
    }

    /// <inheritdoc/>
    public static DeterministicRandom Parse(ReadOnlySpan<byte> utf8Text, IFormatProvider? provider)
    {
        Span<byte> base64 = stackalloc byte[24];
        utf8Text[..Math.Min(utf8Text.Length, 22)].CopyTo(base64);
        base64[22] = (byte)'=';
        base64[23] = (byte)'=';
        foreach (ref byte b in base64[..22])
        {
            b = b switch
            {
                (>= (byte)'0' and <= (byte)'9') or (>= (byte)'a' and <= (byte)'z') or (>= (byte)'A' and <= (byte)'Z') =>
                    b,
                (byte)'-' => (byte)'+',
                (byte)'_' => (byte)'/',
                _ => (byte)'A',
            };
        }

        UInt32Buffer4 buffer = default;

        OperationStatus status = Base64.DecodeFromUtf8(
            base64,
            MemoryMarshal.AsBytes<uint>(buffer),
            out _,
            out int length
        );

        Debug.Assert(status is OperationStatus.Done);
        Debug.Assert(length is 16);

        return new(buffer);
    }

    /// <inheritdoc/>
    public static DeterministicRandom Parse(string s, IFormatProvider? provider)
    {
        Span<char> base64 = stackalloc char[24];
        s.AsSpan(0, Math.Min(s.Length, 22)).CopyTo(base64);
        base64[22] = '=';
        base64[23] = '=';
        foreach (ref char c in base64[..22])
        {
            c = c switch
            {
                (>= '0' and <= '9') or (>= 'a' and <= 'z') or (>= 'A' and <= 'Z') => c,
                '-' => '+',
                '_' => '/',
                _ => 'A',
            };
        }

        UInt32Buffer4 buffer = default;

        bool ok = Convert.TryFromBase64Chars(base64, MemoryMarshal.AsBytes<uint>(buffer), out int length);

        Debug.Assert(ok);
        Debug.Assert(length is 16);

        return new(buffer);
    }

    static bool IUtf8SpanParsable<DeterministicRandom>.TryParse(
        ReadOnlySpan<byte> utf8Text,
        IFormatProvider? provider,
        out DeterministicRandom result
    )
    {
        result = Parse(utf8Text, null);
        return true;
    }

    static bool IParsable<DeterministicRandom>.TryParse(
        [NotNullWhen(true)] string? s,
        IFormatProvider? provider,
        out DeterministicRandom result
    )
    {
        if (s is null)
        {
            result = default;
            return false;
        }

        result = Parse(s, null);
        return true;
    }

    /// <summary>
    /// Generate a new deterministic high quality seed from a seed of unknown quality.
    /// </summary>
    public readonly DeterministicRandom Sanitize()
    {
        DeterministicRandom sanitized = this;

        // The "zero" seed does not work property, swap it with another seed.
        UInt32Buffer4 zero = default;
        if (((ReadOnlySpan<uint>)sanitized._buffer).SequenceEqual(zero))
            sanitized = Parse("HighQualitySeedThisIsA"u8, null);

        for (int i = 0; i < 32; i++)
            sanitized.NextRaw();

        return sanitized;
    }

    /// <inheritdoc/>
    public bool Equals(DeterministicRandom other) => ((Span<uint>)_buffer).SequenceEqual(other._buffer);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is DeterministicRandom seed && Equals(seed);

    public static bool operator ==(DeterministicRandom left, DeterministicRandom right) => left.Equals(right);

    public static bool operator !=(DeterministicRandom left, DeterministicRandom right) => !(left == right);

    /// <inheritdoc/>
    public bool TryFormat(
        Span<byte> utf8Destination,
        out int bytesWritten,
        ReadOnlySpan<char> format,
        IFormatProvider? provider
    )
    {
        Span<byte> buffer = stackalloc byte[24];
        _ = Base64.EncodeToUtf8(MemoryMarshal.AsBytes<uint>(_buffer), buffer, out _, out _);
        buffer.Replace((byte)'+', (byte)'-');
        buffer.Replace((byte)'/', (byte)'_');

        bytesWritten = 23;
        while (bytesWritten > 1 && buffer[--bytesWritten - 1] is (byte)'A') { }
        return buffer[..bytesWritten].TryCopyTo(utf8Destination);
    }

    /// <inheritdoc/>
    public override readonly int GetHashCode()
    {
        HashCode hash = new();
        hash.AddBytes(MemoryMarshal.AsBytes((ReadOnlySpan<uint>)_buffer));
        return hash.ToHashCode();
    }

    /// <summary>
    /// Returns the string representation of the current value for this deterministic random generator.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        Span<char> chars = stackalloc char[24];
        _ = Convert.TryToBase64Chars(MemoryMarshal.AsBytes<uint>(_buffer), chars, out _);
        chars.Replace('+', '-');
        chars.Replace('/', '_');

        int length = 23;
        while (length > 1 && chars[--length - 1] is 'A') { }
        return new(chars[..length]);
    }
}

/// <summary>
/// Extension methods for <see cref="DeterministicRandom"/>.
/// </summary>
public static class DeterministicRandomExtensions
{
    /// <summary>
    /// Enumerable sequence of non-repeating integers.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public readonly ref struct SequenceEnumerable
    {
        readonly ref DeterministicRandom _rand;
        readonly int _offset;
        readonly int _length;

        internal SequenceEnumerable(ref DeterministicRandom rand, int offset, int length)
        {
            _rand = ref rand;
            _offset = offset;
            _length = length;
        }

        public SequenceEnumerator GetEnumerator() => new(ref _rand, _offset, _length);
    }

    /// <summary>
    /// Sequence of non-repeating integers.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public ref struct SequenceEnumerator : IEnumerator<int>
    {
        readonly int _offset;
        int[]? _buffer;
        int _i;

        internal SequenceEnumerator(ref DeterministicRandom rand, int offset, int length)
        {
            _offset = offset;
            _buffer = ArrayPool<int>.Shared.Rent(length);
            _i = length;

            for (int i = 0; i < length; i++)
                _buffer[i] = i;

            for (int i = 0; i < length - 1; i++)
            {
                int j = rand.Next(i, length);
                (_buffer[i], _buffer[j]) = (_buffer[j], _buffer[i]);
            }
        }

        public readonly int Current =>
            (_buffer ?? throw new ObjectDisposedException(nameof(SequenceEnumerator)))[_i] + _offset;

        readonly object IEnumerator.Current => Current;

        public bool MoveNext() => _i-- > 0;

        public void Dispose()
        {
            if (_buffer is not null)
            {
                ArrayPool<int>.Shared.Return(_buffer);
                _buffer = null;
            }
        }

        void IEnumerator.Reset() => throw new NotSupportedException();
    }

    /// <summary>
    /// Enumerable sequence of non-repeating items.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public readonly ref struct ShuffledEnumerable<T>
    {
        readonly Span<T> _values;
        readonly SequenceEnumerable _sequence;

        internal ShuffledEnumerable(ref DeterministicRandom rand, Span<T> values)
        {
            _values = values;
            _sequence = new(ref rand, 0, values.Length);
        }

        public ShuffledEnumerator<T> GetEnumerator() => new(_values, _sequence);
    }

    /// <summary>
    /// Sequence of non-repeating items.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public ref struct ShuffledEnumerator<T> : IEnumerator<T>
    {
        readonly Span<T> _values;
        SequenceEnumerator _sequence;

        internal ShuffledEnumerator(Span<T> values, SequenceEnumerable sequence)
        {
            _values = values;
            _sequence = sequence.GetEnumerator();
        }

        public readonly ref T Current => ref _values[_sequence.Current];

        readonly T IEnumerator<T>.Current => Current;

        readonly object? IEnumerator.Current => Current;

        public bool MoveNext() => _sequence.MoveNext();

        public void Dispose() => _sequence.Dispose();

        void IEnumerator.Reset() => throw new NotSupportedException();
    }

    #region From 'Random'

    /// <inheritdoc cref="Random.Next()"/>
    public static int Next(this ref DeterministicRandom rand)
    {
        Span<int> randInt32 = stackalloc int[1];
        Span<byte> randBytes = MemoryMarshal.AsBytes(randInt32);

        while (true)
        {
            rand.NextBytes(randBytes);
            randInt32[0] &= ~int.MinValue; // Remove negative bit.

            if (randInt32[0] is int.MaxValue)
                continue; // Extremely unlikely, but possible.

            return randInt32[0];
        }
    }

    /// <inheritdoc cref="Random.Next(int)"/>
    public static int Next(this ref DeterministicRandom rand, int maxValue) => rand.Next(stackalloc int[1], maxValue);

    /// <inheritdoc cref="Random.Next(int, int)"/>
    public static int Next(this ref DeterministicRandom rand, int minValue, int maxValue) =>
        rand.Next(stackalloc int[1], minValue, maxValue);

    /// <inheritdoc cref="Random.NextInt64()"/>
    public static long NextInt64(this ref DeterministicRandom rand)
    {
        Span<long> randInt64 = stackalloc long[1];
        Span<byte> randBytes = MemoryMarshal.AsBytes(randInt64);

        while (true)
        {
            rand.NextBytes(randBytes);
            randInt64[0] &= ~long.MinValue; // Remove negative bit.

            if (randInt64[0] is long.MaxValue)
                continue; // Extremely unlikely, but possible.

            return randInt64[0];
        }
    }

    static T Next<T>(this ref DeterministicRandom rand, Span<T> buffer, T minValue, T maxValue)
        where T : struct, INumberBase<T>, IBinaryInteger<T>, IComparisonOperators<T, T, bool>
    {
        if (minValue > maxValue)
            throw new ArgumentOutOfRangeException(
                nameof(minValue),
                $"'{nameof(minValue)}' cannot be greater than '{nameof(maxValue)}'."
            );

        return rand.Next(buffer, maxValue - minValue) + minValue;
    }

    static T Next<T>(this ref DeterministicRandom rand, Span<T> buffer, T maxValue)
        where T : struct, INumberBase<T>, IBinaryInteger<T>, IComparisonOperators<T, T, bool>
    {
        ArgumentOutOfRangeException.ThrowIfNegative(maxValue);

        if (maxValue == default)
            return default;

        Debug.Assert(buffer.Length is 1);

        Span<byte> bytes = MemoryMarshal.AsBytes(buffer);

        buffer[0] = default;

        int zeroBitsCount = int.CreateChecked(T.LeadingZeroCount(maxValue));

        // Skip the initial always-zero bytes and bits.
        bytes = bytes[..^(zeroBitsCount / 8)];
        byte zeroBitsMask = unchecked((byte)(0b11111111 >> (zeroBitsCount - zeroBitsCount / 8 * 8)));

        do
        {
            rand.NextBytes(bytes);
            bytes[^1] &= zeroBitsMask;
        } while (buffer[0] >= maxValue);

        return buffer[0];
    }

    #endregion

    #region Custom

    public static DeterministicRandom NextSeed(this ref DeterministicRandom rand)
    {
        rand.Next();
        return new(
            unchecked((uint)rand.Next()),
            unchecked((uint)rand.Next()),
            unchecked((uint)rand.Next()),
            unchecked((uint)rand.Next())
        );
    }

    /// <summary>
    /// Generate a sequence of non-repeating integers between <c>0</c> (inclusive) and <paramref name="maxValue"/> (exclusive).
    /// </summary>
    /// <param name="rand"></param>
    /// <param name="maxValue"></param>
    /// <returns></returns>
    public static SequenceEnumerable Sequence(this ref DeterministicRandom rand, int maxValue) =>
        new(ref rand, 0, maxValue);

    /// <summary>
    /// Generate a sequence of non-repeating integers between <paramref name="minValue"/> (inclusive) and <paramref name="maxValue"/> (exclusive).
    /// </summary>
    /// <param name="rand"></param>
    /// <param name="minValue"></param>
    /// <param name="maxValue"></param>
    /// <returns></returns>
    public static SequenceEnumerable Sequence(this ref DeterministicRandom rand, int minValue, int maxValue) =>
        new(ref rand, minValue, maxValue - minValue);

    /// <summary>
    /// Pick all items in a random order from a set of values, without repetitions.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="rand"></param>
    /// <param name="values"></param>
    /// <returns></returns>
    public static ShuffledEnumerable<T> Shuffled<T>(this ref DeterministicRandom rand, Span<T> values) =>
        new(ref rand, values);

    /// <summary>
    /// Pick an item at random from a set of values.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="rand"></param>
    /// <param name="values"></param>
    /// <returns></returns>
    public static T Pick<T>(this ref DeterministicRandom rand, ReadOnlySpan<T> values) =>
        values[rand.Next(values.Length)];

    #endregion
}
