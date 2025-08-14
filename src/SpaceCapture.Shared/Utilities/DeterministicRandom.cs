// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: MIT

using System.Buffers;
using System.Buffers.Text;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SpaceCapture.Shared.Utilities;

/// <summary>
/// Deterministic random number generator.
/// </summary>
public static class DeterministicRandom
{
    [InlineArray(4)]
    internal struct UInt32Buffer4
    {
        uint _element;
    }

    /// <summary>
    /// Seed.
    /// </summary>
    [JsonConverter(typeof(JsonConverter))]
    public struct Seed
        : IEquatable<Seed>,
            IParsable<Seed>,
            IUtf8SpanParsable<Seed>,
            IUtf8SpanFormattable
    {
        [EditorBrowsable(EditorBrowsableState.Never)]
        public class JsonConverter : JsonConverter<Seed>
        {
            public override Seed Read(
                ref Utf8JsonReader reader,
                Type typeToConvert,
                JsonSerializerOptions options
            )
            {
                Span<byte> buffer = stackalloc byte[22];
                int length = reader.CopyString(buffer);
                return Parse(buffer[..length], null);
            }

            public override void Write(
                Utf8JsonWriter writer,
                Seed value,
                JsonSerializerOptions options
            )
            {
                Span<byte> buffer = stackalloc byte[22];
                bool ok = value.TryFormat(buffer, out int length, "", null);
                Debug.Assert(ok);
                writer.WriteStringValue(buffer[..length]);
            }
        }

        internal UInt32Buffer4 _buffer;

        internal Seed(UInt32Buffer4 buffer) => _buffer = buffer;

        /// <inheritdoc/>
        public static Seed Parse(ReadOnlySpan<byte> utf8Text, IFormatProvider? provider)
        {
            Span<byte> base64 = stackalloc byte[24];
            utf8Text[..Math.Min(utf8Text.Length, 22)].CopyTo(base64);
            base64[22] = (byte)'=';
            base64[23] = (byte)'=';
            foreach (ref byte b in base64[..22])
            {
                b = b switch
                {
                    (>= (byte)'0' and <= (byte)'9')
                    or (>= (byte)'a' and <= (byte)'z')
                    or (>= (byte)'A' and <= (byte)'Z') => b,
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
        public static Seed Parse(string s, IFormatProvider? provider)
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

            bool ok = Convert.TryFromBase64Chars(
                base64,
                MemoryMarshal.AsBytes<uint>(buffer),
                out int length
            );

            Debug.Assert(ok);
            Debug.Assert(length is 16);

            return new(buffer);
        }

        static bool IUtf8SpanParsable<Seed>.TryParse(
            ReadOnlySpan<byte> utf8Text,
            IFormatProvider? provider,
            out Seed result
        )
        {
            result = Parse(utf8Text, null);
            return true;
        }

        static bool IParsable<Seed>.TryParse(
            [NotNullWhen(true)] string? s,
            IFormatProvider? provider,
            out Seed result
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
        public readonly Seed Sanitize()
        {
            Seed sanitized = this;

            // The "zero" seed does not work property, swap it with another seed.
            UInt32Buffer4 zero = default;
            if (((ReadOnlySpan<uint>)sanitized._buffer).SequenceEqual(zero))
                sanitized = Parse("HighQualitySeedThisIsA"u8, null);

            for (int i = 0; i < 32; i++)
                Next(ref sanitized);

            return sanitized;
        }

        /// <inheritdoc/>
        public bool Equals(Seed other) => ((Span<uint>)_buffer).SequenceEqual(other._buffer);

        /// <inheritdoc/>
        public override bool Equals(object? obj) => obj is Seed seed && Equals(seed);

        public static bool operator ==(Seed left, Seed right) => left.Equals(right);

        public static bool operator !=(Seed left, Seed right) => !(left == right);

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

        /// <inheritdoc/>
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
    /// Generate a new random seed.
    /// </summary>
    /// <returns></returns>
    public static Seed NewSeed()
    {
        UInt32Buffer4 buffer = default;
        Random.Shared.NextBytes(MemoryMarshal.AsBytes<uint>(buffer));
        return new(buffer);
    }

    /// <summary>
    /// Generate a random number between <see cref="int.MinValue"/> and <see cref="int.MaxValue"/>;
    /// </summary>
    /// <param name="seed"></param>
    /// <returns></returns>
    public static int Next(ref Seed seed)
    {
        uint x = seed._buffer[0];
        uint w = seed._buffer[3];

        uint t = x ^ (x << 11);
        uint v = w ^ (w >> 19) ^ t ^ (t >> 8);

        seed._buffer[0] = seed._buffer[1];
        seed._buffer[1] = seed._buffer[2];
        seed._buffer[2] = w;
        seed._buffer[3] = v;

        return (int)v;
    }

    /// <summary>
    /// Generate a random number between <see cref="int.MinValue"/> and <see cref="int.MaxValue"/>;
    /// </summary>
    /// <param name="seed"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static Seed Next(Seed seed, out int value)
    {
        Seed nextSeed = seed;
        value = Next(ref nextSeed);
        return nextSeed;
    }
}
