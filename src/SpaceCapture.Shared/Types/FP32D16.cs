// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: MIT

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SpaceCapture.Shared.Types;

// Note: why not just use floats or doubles? Because they are not deterministic.
// We want the simulation to be exactly the same on the server and on each of
// the clients, which might be running on different hardware architectures.

/// <summary>
/// 32-bit signed fixed point number with 16 bits of decimal precision.
/// </summary>
[JsonConverter(typeof(JsonConverter))]
[DebuggerDisplay($"{{{nameof(ToString)}(),nq}}")]
public readonly partial struct FP32D16
    : IBinaryInteger<FP32D16>,
        ITrigonometricFunctions<FP32D16>,
        IConvertible
{
    public class JsonConverter : JsonConverter<FP32D16>
    {
        public override FP32D16 Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options
        ) => (FP32D16)reader.GetDecimal();

        public override void Write(
            Utf8JsonWriter writer,
            FP32D16 value,
            JsonSerializerOptions options
        ) =>
            writer.WriteNumberValue(
                Math.Round((decimal)value, s_approximateDecimalPrecision)
                    / 1.000000000000000000000000000000000m
            );
    }

    const int BinaryDecimalDigits = 16;
    const int ValueOne = 1 << BinaryDecimalDigits;
    const int DecimalBitsMask = ValueOne - 1;
    const int IntegerBitsMask = ~DecimalBitsMask;
    const int ValueE = (int)(Math.E * ValueOne + .5);
    const int ValuePi = (int)(Math.PI * ValueOne + .5);
    const int ValueHalfPi = (int)(Math.PI / 2 * ValueOne + .5);
    const int ValueTau = (int)(Math.Tau * ValueOne + .5);
    const double EpsilonDouble = 1d / ValueOne;
    const decimal EpsilonDecimal = 1m / ValueOne;

    static readonly int s_approximateDecimalPrecision =
        (1m / ValueOne).ToString(CultureInfo.InvariantCulture).AsSpan(2).IndexOfAnyExcept('0') + 1;

    readonly int _v;

    private FP32D16(int raw) => _v = raw;

    public static FP32D16 AdditiveIdentity => new(0);

    public static FP32D16 MultiplicativeIdentity => new(ValueOne);

    public static FP32D16 One => new(ValueOne);

    public static int Radix => 10;

    public static FP32D16 Zero => new(0);

    public static FP32D16 E => new(ValueE);

    public static FP32D16 Pi => new(ValuePi);

    public static FP32D16 Tau => new(ValueTau);

    public static FP32D16 Abs(FP32D16 value) => new(int.Abs(value._v));

    public static bool IsCanonical(FP32D16 value) => true;

    public static bool IsComplexNumber(FP32D16 value) => false;

    public static bool IsEvenInteger(FP32D16 value) =>
        (value._v & (ValueOne | DecimalBitsMask)) is ValueOne;

    public static bool IsFinite(FP32D16 value) => true;

    public static bool IsImaginaryNumber(FP32D16 value) => false;

    public static bool IsInfinity(FP32D16 value) => false;

    public static bool IsInteger(FP32D16 value) => true;

    public static bool IsNaN(FP32D16 value) => false;

    public static bool IsNegative(FP32D16 value) => value._v < 0;

    public static bool IsNegativeInfinity(FP32D16 value) => false;

    public static bool IsNormal(FP32D16 value) => value._v is not 0;

    public static bool IsOddInteger(FP32D16 value) =>
        (value._v & (ValueOne | DecimalBitsMask)) is 0;

    public static bool IsPositive(FP32D16 value) => value._v >= 0;

    public static bool IsPositiveInfinity(FP32D16 value) => false;

    public static bool IsPow2(FP32D16 value) =>
        BitOperations.PopCount(unchecked((ulong)(value._v >> BinaryDecimalDigits))) is 1;

    public static bool IsRealNumber(FP32D16 value) => true;

    public static bool IsSubnormal(FP32D16 value) => value._v is not 0;

    public static bool IsZero(FP32D16 value) => value._v is 0;

    public static FP32D16 Log2(FP32D16 value) => throw new NotImplementedException();

    public static FP32D16 MaxMagnitude(FP32D16 x, FP32D16 y) => Abs(x) > Abs(y) ? x : y;

    public static FP32D16 MaxMagnitudeNumber(FP32D16 x, FP32D16 y) => MaxMagnitude(x, y);

    public static FP32D16 MinMagnitude(FP32D16 x, FP32D16 y) => Abs(x) < Abs(y) ? x : y;

    public static FP32D16 MinMagnitudeNumber(FP32D16 x, FP32D16 y) => MinMagnitudeNumber(x, y);

    public static FP32D16 Parse(
        ReadOnlySpan<char> s,
        NumberStyles style,
        IFormatProvider? provider
    ) => throw new NotImplementedException();

    public static FP32D16 Parse(string s, NumberStyles style, IFormatProvider? provider) =>
        throw new NotImplementedException();

    public static FP32D16 Parse(ReadOnlySpan<char> s, IFormatProvider? provider) =>
        throw new NotImplementedException();

    public static FP32D16 Parse(string s, IFormatProvider? provider) =>
        throw new NotImplementedException();

    public static FP32D16 PopCount(FP32D16 value) => int.PopCount(value._v);

    public static FP32D16 TrailingZeroCount(FP32D16 value) => int.TrailingZeroCount(value._v);

    public static bool TryConvertFromChecked<TOther>(
        TOther value,
        [MaybeNullWhen(false)] out FP32D16 result
    )
        where TOther : INumberBase<TOther> => throw new NotImplementedException();

    public static bool TryConvertFromSaturating<TOther>(
        TOther value,
        [MaybeNullWhen(false)] out FP32D16 result
    )
        where TOther : INumberBase<TOther> => throw new NotImplementedException();

    public static bool TryConvertFromTruncating<TOther>(
        TOther value,
        [MaybeNullWhen(false)] out FP32D16 result
    )
        where TOther : INumberBase<TOther> => throw new NotImplementedException();

    public static bool TryConvertToChecked<TOther>(
        FP32D16 value,
        [MaybeNullWhen(false)] out TOther result
    )
        where TOther : INumberBase<TOther> => throw new NotImplementedException();

    public static bool TryConvertToSaturating<TOther>(
        FP32D16 value,
        [MaybeNullWhen(false)] out TOther result
    )
        where TOther : INumberBase<TOther> => throw new NotImplementedException();

    public static bool TryConvertToTruncating<TOther>(
        FP32D16 value,
        [MaybeNullWhen(false)] out TOther result
    )
        where TOther : INumberBase<TOther> => throw new NotImplementedException();

    public static bool TryParse(
        ReadOnlySpan<char> s,
        NumberStyles style,
        IFormatProvider? provider,
        [MaybeNullWhen(false)] out FP32D16 result
    ) => throw new NotImplementedException();

    public static bool TryParse(
        [NotNullWhen(true)] string? s,
        NumberStyles style,
        IFormatProvider? provider,
        [MaybeNullWhen(false)] out FP32D16 result
    ) => throw new NotImplementedException();

    public static bool TryParse(
        ReadOnlySpan<char> s,
        IFormatProvider? provider,
        [MaybeNullWhen(false)] out FP32D16 result
    ) => throw new NotImplementedException();

    public static bool TryParse(
        [NotNullWhen(true)] string? s,
        IFormatProvider? provider,
        [MaybeNullWhen(false)] out FP32D16 result
    ) => throw new NotImplementedException();

    public static bool TryReadBigEndian(
        ReadOnlySpan<byte> source,
        bool isUnsigned,
        out FP32D16 value
    ) => throw new NotImplementedException();

    public static bool TryReadLittleEndian(
        ReadOnlySpan<byte> source,
        bool isUnsigned,
        out FP32D16 value
    ) => throw new NotImplementedException();

    public int CompareTo(object? obj) => throw new NotImplementedException();

    public int CompareTo(FP32D16 other) => _v.CompareTo(other._v);

    public override bool Equals(object? obj) => obj is FP32D16 v && Equals(v);

    public bool Equals(FP32D16 other) => _v.Equals(other._v);

    public override int GetHashCode() => _v.GetHashCode();

    public int GetByteCount() => sizeof(int);

    public int GetShortestBitLength() => throw new NotImplementedException();

    public string ToString(string? format, IFormatProvider? formatProvider) =>
        (
            Math.Round((decimal)this, s_approximateDecimalPrecision)
            / 1.000000000000000000000000000000000m
        ).ToString(format, formatProvider);

    public bool TryFormat(
        Span<char> destination,
        out int charsWritten,
        ReadOnlySpan<char> format,
        IFormatProvider? provider
    ) =>
        (
            Math.Round((decimal)this, s_approximateDecimalPrecision)
            / 1.000000000000000000000000000000000m
        ).TryFormat(destination, out charsWritten, format, provider);

    public bool TryWriteBigEndian(Span<byte> destination, out int bytesWritten) =>
        throw new NotImplementedException();

    public bool TryWriteLittleEndian(Span<byte> destination, out int bytesWritten) =>
        throw new NotImplementedException();

    public TypeCode GetTypeCode() => TypeCode.Object;

    public bool ToBoolean(IFormatProvider? provider) => (bool)this;

    public byte ToByte(IFormatProvider? provider) => (byte)this;

    public char ToChar(IFormatProvider? provider) => (char)this;

    public DateTime ToDateTime(IFormatProvider? provider) => throw new NotImplementedException();

    public decimal ToDecimal(IFormatProvider? provider) => (decimal)this;

    public double ToDouble(IFormatProvider? provider) => (double)this;

    public short ToInt16(IFormatProvider? provider) => (short)this;

    public int ToInt32(IFormatProvider? provider) => (int)this;

    public long ToInt64(IFormatProvider? provider) => (long)this;

    public sbyte ToSByte(IFormatProvider? provider) => (sbyte)this;

    public float ToSingle(IFormatProvider? provider) => (float)this;

    public string ToString(IFormatProvider? provider) =>
        (
            Math.Round((decimal)this, s_approximateDecimalPrecision)
            / 1.000000000000000000000000000000000m
        ).ToString(provider);

    public object ToType(Type conversionType, IFormatProvider? provider) =>
        throw new NotImplementedException();

    public ushort ToUInt16(IFormatProvider? provider) => (ushort)this;

    public uint ToUInt32(IFormatProvider? provider) => (uint)this;

    public ulong ToUInt64(IFormatProvider? provider) => (ulong)this;

    public static FP32D16 Acos(FP32D16 x) => throw new NotImplementedException();

    public static FP32D16 AcosPi(FP32D16 x) => throw new NotImplementedException();

    public static FP32D16 Asin(FP32D16 x) => throw new NotImplementedException();

    public static FP32D16 AsinPi(FP32D16 x) => throw new NotImplementedException();

    public static FP32D16 Atan(FP32D16 x) => throw new NotImplementedException();

    public static FP32D16 AtanPi(FP32D16 x) => throw new NotImplementedException();

    public static FP32D16 Cos(FP32D16 x) => Sin(x - ValueHalfPi);

    public static FP32D16 CosPi(FP32D16 x) => throw new NotImplementedException();

    public static FP32D16 Sin(FP32D16 x)
    {
        int i = (x._v % ValueTau) is var r && r < 0 ? r + ValueTau : r;
        return i <= ValuePi ? SinUpToHalfPi(i) : -SinUpToHalfPi(i - ValuePi);

        static FP32D16 SinUpToHalfPi(int i)
        {
            Debug.Assert(i <= ValuePi);

            if (i <= ValueHalfPi)
                return StaticLutBuffer.s_sinLut[i];

            return StaticLutBuffer.s_sinLut[ValuePi - i];
        }
    }

    public static (FP32D16 Sin, FP32D16 Cos) SinCos(FP32D16 x) => (Sin(x), Cos(x));

    public static (FP32D16 SinPi, FP32D16 CosPi) SinCosPi(FP32D16 x) =>
        throw new NotImplementedException();

    public static FP32D16 SinPi(FP32D16 x) => throw new NotImplementedException();

    public static FP32D16 Tan(FP32D16 x) => throw new NotImplementedException();

    public static FP32D16 TanPi(FP32D16 x) => throw new NotImplementedException();

    public override string ToString() => ToString(CultureInfo.InvariantCulture);

    public static FP32D16 operator +(FP32D16 value) => new(+value._v);

    public static FP32D16 operator +(FP32D16 left, FP32D16 right) => new(left._v + right._v);

    public static FP32D16 operator -(FP32D16 value) => new(-value._v);

    public static FP32D16 operator -(FP32D16 left, FP32D16 right) => new(left._v - right._v);

    public static FP32D16 operator ~(FP32D16 value) => new(~value._v);

    public static FP32D16 operator ++(FP32D16 value) => new(value._v + ValueOne);

    public static FP32D16 operator --(FP32D16 value) => new(value._v - ValueOne);

    public static FP32D16 operator *(FP32D16 left, FP32D16 right) =>
        new(checked((int)(((long)left._v * right._v) >> BinaryDecimalDigits)));

    public static FP32D16 operator /(FP32D16 left, FP32D16 right)
    {
        // https://stackoverflow.com/a/18067292/1135019
        long n = (long)left._v << BinaryDecimalDigits;
        long d = right._v;
        long v = ((n < 0) == (d < 0)) ? ((n + d / 2) / d) : ((n - d / 2) / d);
        return new(checked((int)v));
    }

    public static FP32D16 operator %(FP32D16 left, FP32D16 right) => new(left._v % right._v);

    public static FP32D16 operator &(FP32D16 left, FP32D16 right) => new(left._v & right._v);

    public static FP32D16 operator |(FP32D16 left, FP32D16 right) => new(left._v | right._v);

    public static FP32D16 operator ^(FP32D16 left, FP32D16 right) =>
        throw new NotImplementedException();

    public static FP32D16 operator <<(FP32D16 value, int shiftAmount) =>
        new(value._v << shiftAmount);

    public static FP32D16 operator >>(FP32D16 value, int shiftAmount) =>
        new(value._v >> shiftAmount);

    public static bool operator ==(FP32D16 left, FP32D16 right) => left._v == right._v;

    public static bool operator !=(FP32D16 left, FP32D16 right) => left._v != right._v;

    public static bool operator <(FP32D16 left, FP32D16 right) => left._v < right._v;

    public static bool operator >(FP32D16 left, FP32D16 right) => left._v > right._v;

    public static bool operator <=(FP32D16 left, FP32D16 right) => left._v <= right._v;

    public static bool operator >=(FP32D16 left, FP32D16 right) => left._v >= right._v;

    public static FP32D16 operator >>>(FP32D16 value, int shiftAmount) =>
        new(value._v >>> shiftAmount);

    public static explicit operator bool(FP32D16 value) => value._v is not 0;

    public static explicit operator byte(FP32D16 value) => (byte)(value._v >> BinaryDecimalDigits);

    public static explicit operator char(FP32D16 value) => (char)(value._v >> BinaryDecimalDigits);

    public static explicit operator short(FP32D16 value) =>
        (short)(value._v >> BinaryDecimalDigits);

    public static explicit operator int(FP32D16 value) => value._v >> BinaryDecimalDigits;

    public static explicit operator long(FP32D16 value) => value._v >> BinaryDecimalDigits;

    public static explicit operator sbyte(FP32D16 value) =>
        (sbyte)(value._v >> BinaryDecimalDigits);

    public static explicit operator ushort(FP32D16 value) =>
        (ushort)(value._v >> BinaryDecimalDigits);

    public static explicit operator uint(FP32D16 value) => (uint)(value._v >> BinaryDecimalDigits);

    public static explicit operator ulong(FP32D16 value) =>
        (ulong)(value._v >> BinaryDecimalDigits);

    public static implicit operator float(FP32D16 value) => (float)(value._v * EpsilonDouble);

    public static implicit operator double(FP32D16 value) => value._v * EpsilonDouble;

    public static implicit operator decimal(FP32D16 value) => value._v * EpsilonDecimal;

    public static implicit operator FP32D16(byte value) => new(value << BinaryDecimalDigits);

    public static implicit operator FP32D16(char value) => new(value << BinaryDecimalDigits);

    public static implicit operator FP32D16(short value) => new(value << BinaryDecimalDigits);

    public static implicit operator FP32D16(int value) => new(value << BinaryDecimalDigits);

    public static explicit operator FP32D16(long value) =>
        new(checked((int)(value << BinaryDecimalDigits)));

    public static implicit operator FP32D16(sbyte value) => new(value << BinaryDecimalDigits);

    public static implicit operator FP32D16(ushort value) => new(value << BinaryDecimalDigits);

    public static explicit operator FP32D16(uint value) =>
        new(checked((int)(value << BinaryDecimalDigits)));

    public static explicit operator FP32D16(ulong value) =>
        new(checked((int)(value << BinaryDecimalDigits)));

    public static explicit operator FP32D16(float value) => new(Convert.ToInt32(value * ValueOne));

    public static explicit operator FP32D16(double value) => new(Convert.ToInt32(value * ValueOne));

    public static explicit operator FP32D16(decimal value) =>
        new(Convert.ToInt32(value * ValueOne));
}
