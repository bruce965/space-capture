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
/// 48-bit signed fixed point number with 16 bits of decimal precision.
/// </summary>
/// <remarks>
/// 1 sign bit, 15 bits unused, 32 bits of integer precision, 16 bits of decimal precision.
/// </remarks>
[JsonConverter(typeof(JsonConverter))]
[DebuggerDisplay($"{{{nameof(ToString)}(),nq}}")]
public readonly partial struct FP48D16
    : IBinaryInteger<FP48D16>,
        ITrigonometricFunctions<FP48D16>,
        IMinMaxValue<FP48D16>,
        IConvertible
{
    public class JsonConverter : JsonConverter<FP48D16>
    {
        public override FP48D16 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            (FP48D16)reader.GetDecimal();

        public override void Write(Utf8JsonWriter writer, FP48D16 value, JsonSerializerOptions options) =>
            writer.WriteNumberValue(
                Math.Round((decimal)value, s_approximateDecimalPrecision) / 1.000000000000000000000000000000000m
            );
    }

    const int TotalBits = sizeof(long) * 8;
    const int DecimalBits = 16;
    const int IntegerBits = 32;
    const int UnusedBitsExceptSignBit = TotalBits - IntegerBits - DecimalBits;
    const long ValueOne = 1L << DecimalBits;
    const long DecimalBitsMask = ValueOne - 1;
    const long IntegerBitsMask = ~DecimalBitsMask & (~0 >> UnusedBitsExceptSignBit);
    const long SignBitMask = long.MinValue;
    const long UnusedBitsMask = ~(DecimalBitsMask | IntegerBitsMask | SignBitMask);
    const long ValueE = (long)(Math.E * ValueOne + .5);
    const long ValuePi = (long)(Math.PI * ValueOne + .5);
    const long ValueHalfPi = (long)(Math.PI / 2 * ValueOne + .5);
    const long ValueTau = (long)(Math.Tau * ValueOne + .5);
    const long ValueMax = DecimalBitsMask | IntegerBitsMask;
    const long ValueMin = SignBitMask;
    const double EpsilonDouble = 1d / ValueOne;
    const decimal EpsilonDecimal = 1m / ValueOne;

    static readonly int s_approximateDecimalPrecision =
        (1m / ValueOne).ToString(CultureInfo.InvariantCulture).AsSpan(2).IndexOfAnyExcept('0') + 1;

    readonly long _v;

    private FP48D16(long raw) => _v = raw;

    /// <inheritdoc cref="IMinMaxValue{TSelf}.MinValue"/>
    public static readonly FP48D16 MinValue = new(ValueMin);

    static FP48D16 IMinMaxValue<FP48D16>.MinValue => MinValue;

    /// <inheritdoc cref="IMinMaxValue{TSelf}.MaxValue"/>
    public static readonly FP48D16 MaxValue = new(ValueMax);

    static FP48D16 IMinMaxValue<FP48D16>.MaxValue => MaxValue;

    public static FP48D16 AdditiveIdentity => new(0);

    public static FP48D16 MultiplicativeIdentity => new(ValueOne);

    public static FP48D16 One => new(ValueOne);

    public static int Radix => 10;

    public static FP48D16 Zero => new(0);

    public static FP48D16 E => new(ValueE);

    public static FP48D16 Pi => new(ValuePi);

    public static FP48D16 Tau => new(ValueTau);

    public static FP48D16 Abs(FP48D16 value) => new(long.Abs(value._v));

    public static bool IsCanonical(FP48D16 value) => true;

    public static bool IsComplexNumber(FP48D16 value) => false;

    public static bool IsEvenInteger(FP48D16 value) => (value._v & (ValueOne | DecimalBitsMask)) is ValueOne;

    public static bool IsFinite(FP48D16 value) => true;

    public static bool IsImaginaryNumber(FP48D16 value) => false;

    public static bool IsInfinity(FP48D16 value) => false;

    public static bool IsInteger(FP48D16 value) => true;

    public static bool IsNaN(FP48D16 value) => false;

    public static bool IsNegative(FP48D16 value) => value._v < 0;

    public static bool IsNegativeInfinity(FP48D16 value) => false;

    public static bool IsNormal(FP48D16 value) => value._v is not 0;

    public static bool IsOddInteger(FP48D16 value) => (value._v & (ValueOne | DecimalBitsMask)) is 0;

    public static bool IsPositive(FP48D16 value) => value._v >= 0;

    public static bool IsPositiveInfinity(FP48D16 value) => false;

    public static bool IsPow2(FP48D16 value) =>
        BitOperations.PopCount(unchecked((ulong)(value._v >> DecimalBits))) is 1;

    public static bool IsRealNumber(FP48D16 value) => true;

    public static bool IsSubnormal(FP48D16 value) => value._v is not 0;

    public static bool IsZero(FP48D16 value) => value._v is 0;

    public static FP48D16 Log2(FP48D16 value) => throw new NotImplementedException();

    public static FP48D16 MaxMagnitude(FP48D16 x, FP48D16 y) => Abs(x) > Abs(y) ? x : y;

    public static FP48D16 MaxMagnitudeNumber(FP48D16 x, FP48D16 y) => MaxMagnitude(x, y);

    public static FP48D16 MinMagnitude(FP48D16 x, FP48D16 y) => Abs(x) < Abs(y) ? x : y;

    public static FP48D16 MinMagnitudeNumber(FP48D16 x, FP48D16 y) => MinMagnitudeNumber(x, y);

    public static FP48D16 Parse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider) =>
        throw new NotImplementedException();

    public static FP48D16 Parse(string s, NumberStyles style, IFormatProvider? provider) =>
        throw new NotImplementedException();

    public static FP48D16 Parse(ReadOnlySpan<char> s, IFormatProvider? provider) => throw new NotImplementedException();

    public static FP48D16 Parse(string s, IFormatProvider? provider) => throw new NotImplementedException();

    public static FP48D16 PopCount(FP48D16 value) => (FP48D16)long.PopCount(value._v);

    public static FP48D16 TrailingZeroCount(FP48D16 value) => (FP48D16)long.TrailingZeroCount(value._v);

    public static bool TryConvertFromChecked<TOther>(TOther value, [MaybeNullWhen(false)] out FP48D16 result)
        where TOther : INumberBase<TOther> => throw new NotImplementedException();

    public static bool TryConvertFromSaturating<TOther>(TOther value, [MaybeNullWhen(false)] out FP48D16 result)
        where TOther : INumberBase<TOther> => throw new NotImplementedException();

    public static bool TryConvertFromTruncating<TOther>(TOther value, [MaybeNullWhen(false)] out FP48D16 result)
        where TOther : INumberBase<TOther> => throw new NotImplementedException();

    public static bool TryConvertToChecked<TOther>(FP48D16 value, [MaybeNullWhen(false)] out TOther result)
        where TOther : INumberBase<TOther> => throw new NotImplementedException();

    public static bool TryConvertToSaturating<TOther>(FP48D16 value, [MaybeNullWhen(false)] out TOther result)
        where TOther : INumberBase<TOther> => throw new NotImplementedException();

    public static bool TryConvertToTruncating<TOther>(FP48D16 value, [MaybeNullWhen(false)] out TOther result)
        where TOther : INumberBase<TOther> => throw new NotImplementedException();

    public static bool TryParse(
        ReadOnlySpan<char> s,
        NumberStyles style,
        IFormatProvider? provider,
        [MaybeNullWhen(false)] out FP48D16 result
    ) => throw new NotImplementedException();

    public static bool TryParse(
        [NotNullWhen(true)] string? s,
        NumberStyles style,
        IFormatProvider? provider,
        [MaybeNullWhen(false)] out FP48D16 result
    ) => throw new NotImplementedException();

    public static bool TryParse(
        ReadOnlySpan<char> s,
        IFormatProvider? provider,
        [MaybeNullWhen(false)] out FP48D16 result
    ) => throw new NotImplementedException();

    public static bool TryParse(
        [NotNullWhen(true)] string? s,
        IFormatProvider? provider,
        [MaybeNullWhen(false)] out FP48D16 result
    ) => throw new NotImplementedException();

    public static bool TryReadBigEndian(ReadOnlySpan<byte> source, bool isUnsigned, out FP48D16 value) =>
        throw new NotImplementedException();

    public static bool TryReadLittleEndian(ReadOnlySpan<byte> source, bool isUnsigned, out FP48D16 value) =>
        throw new NotImplementedException();

    public int CompareTo(object? obj) => throw new NotImplementedException();

    public int CompareTo(FP48D16 other) => _v.CompareTo(other._v);

    public override bool Equals(object? obj) => obj is FP48D16 v && Equals(v);

    public bool Equals(FP48D16 other) => _v.Equals(other._v);

    public override int GetHashCode() => _v.GetHashCode();

    public int GetByteCount() => sizeof(long);

    public int GetShortestBitLength() => throw new NotImplementedException();

    public string ToString(string? format, IFormatProvider? formatProvider) =>
        (Math.Round((decimal)this, s_approximateDecimalPrecision) / 1.000000000000000000000000000000000m).ToString(
            format,
            formatProvider
        );

    public bool TryFormat(
        Span<char> destination,
        out int charsWritten,
        ReadOnlySpan<char> format,
        IFormatProvider? provider
    ) =>
        (Math.Round((decimal)this, s_approximateDecimalPrecision) / 1.000000000000000000000000000000000m).TryFormat(
            destination,
            out charsWritten,
            format,
            provider
        );

    public bool TryWriteBigEndian(Span<byte> destination, out int bytesWritten) => throw new NotImplementedException();

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
        (Math.Round((decimal)this, s_approximateDecimalPrecision) / 1.000000000000000000000000000000000m).ToString(
            provider
        );

    public object ToType(Type conversionType, IFormatProvider? provider) => throw new NotImplementedException();

    public ushort ToUInt16(IFormatProvider? provider) => (ushort)this;

    public uint ToUInt32(IFormatProvider? provider) => (uint)this;

    public ulong ToUInt64(IFormatProvider? provider) => (ulong)this;

    public static FP48D16 Acos(FP48D16 x) => throw new NotImplementedException();

    public static FP48D16 AcosPi(FP48D16 x) => throw new NotImplementedException();

    public static FP48D16 Asin(FP48D16 x) => throw new NotImplementedException();

    public static FP48D16 AsinPi(FP48D16 x) => throw new NotImplementedException();

    public static FP48D16 Atan(FP48D16 x) => throw new NotImplementedException();

    public static FP48D16 AtanPi(FP48D16 x) => throw new NotImplementedException();

    public static FP48D16 Cos(FP48D16 x) => Sin(x - new FP48D16(ValueHalfPi));

    public static FP48D16 CosPi(FP48D16 x) => throw new NotImplementedException();

    public static FP48D16 Sin(FP48D16 x)
    {
        long i = (x._v % ValueTau) is var r && r < 0 ? r + ValueTau : r;
        return i <= ValuePi ? SinUpToHalfPi(i) : -SinUpToHalfPi(i - ValuePi);

        static FP48D16 SinUpToHalfPi(long i)
        {
            Debug.Assert(i <= ValuePi);

            if (i <= ValueHalfPi)
                return StaticLutBuffer.s_sinLut[(int)i];

            return StaticLutBuffer.s_sinLut[(int)(ValuePi - i)];
        }
    }

    public static (FP48D16 Sin, FP48D16 Cos) SinCos(FP48D16 x) => (Sin(x), Cos(x));

    public static (FP48D16 SinPi, FP48D16 CosPi) SinCosPi(FP48D16 x) => throw new NotImplementedException();

    public static FP48D16 SinPi(FP48D16 x) => throw new NotImplementedException();

    public static FP48D16 Tan(FP48D16 x) => throw new NotImplementedException();

    public static FP48D16 TanPi(FP48D16 x) => throw new NotImplementedException();

    public static FP48D16 Min(FP48D16 val1, FP48D16 val2) => val1 < val2 ? val1 : val2;

    public static FP48D16 Max(FP48D16 val1, FP48D16 val2) => val1 > val2 ? val1 : val2;

    public override string ToString() => ToString(CultureInfo.InvariantCulture);

    public static FP48D16 operator +(FP48D16 value) => new(+value._v);

    public static FP48D16 operator +(FP48D16 left, FP48D16 right) => new(left._v + right._v);

    public static FP48D16 operator -(FP48D16 value) => new(-value._v);

    public static FP48D16 operator -(FP48D16 left, FP48D16 right) => new(left._v - right._v);

    public static FP48D16 operator ~(FP48D16 value) => new(~value._v);

    public static FP48D16 operator ++(FP48D16 value) => new(value._v + ValueOne);

    public static FP48D16 operator --(FP48D16 value) => new(value._v - ValueOne);

    public static FP48D16 operator *(FP48D16 left, FP48D16 right) => new((left._v * right._v) >> DecimalBits);

    public static FP48D16 operator /(FP48D16 left, FP48D16 right)
    {
        // https://stackoverflow.com/a/18067292/1135019
        // TODO: is the sign preserved correctly? Probably not for numbers < 2^31. To be tested and fixed if necessary.
        long n = (left._v << DecimalBits) | (left._v & SignBitMask);
        long d = right._v;
        long v = ((n < 0) == (d < 0)) ? ((n + d / 2) / d) : ((n - d / 2) / d);

        if ((n & UnusedBitsMask) is not 0)
            throw new OverflowException();

        return new(v);
    }

    public static FP48D16 operator %(FP48D16 left, FP48D16 right) => new(left._v % right._v);

    public static FP48D16 operator &(FP48D16 left, FP48D16 right) => new(left._v & right._v);

    public static FP48D16 operator |(FP48D16 left, FP48D16 right) => new(left._v | right._v);

    public static FP48D16 operator ^(FP48D16 left, FP48D16 right) => throw new NotImplementedException();

    public static FP48D16 operator <<(FP48D16 value, int shiftAmount) => new(value._v << shiftAmount);

    public static FP48D16 operator >>(FP48D16 value, int shiftAmount) => new(value._v >> shiftAmount);

    public static bool operator ==(FP48D16 left, FP48D16 right) => left._v == right._v;

    public static bool operator !=(FP48D16 left, FP48D16 right) => left._v != right._v;

    public static bool operator <(FP48D16 left, FP48D16 right) => left._v < right._v;

    public static bool operator >(FP48D16 left, FP48D16 right) => left._v > right._v;

    public static bool operator <=(FP48D16 left, FP48D16 right) => left._v <= right._v;

    public static bool operator >=(FP48D16 left, FP48D16 right) => left._v >= right._v;

    public static FP48D16 operator >>>(FP48D16 value, int shiftAmount) => new(value._v >>> shiftAmount);

    public static explicit operator bool(FP48D16 value) => value._v is not 0;

    public static explicit operator byte(FP48D16 value) => (byte)(value._v >> DecimalBits);

    public static explicit operator char(FP48D16 value) => (char)(value._v >> DecimalBits);

    public static explicit operator short(FP48D16 value) => (short)(value._v >> DecimalBits);

    public static explicit operator int(FP48D16 value) => (int)(value._v >> DecimalBits);

    public static explicit operator long(FP48D16 value) => value._v >> DecimalBits;

    public static explicit operator sbyte(FP48D16 value) => (sbyte)(value._v >> DecimalBits);

    public static explicit operator ushort(FP48D16 value) => (ushort)(value._v >> DecimalBits);

    public static explicit operator uint(FP48D16 value) => (uint)(value._v >> DecimalBits);

    public static explicit operator ulong(FP48D16 value) => (ulong)(value._v >> DecimalBits);

    public static implicit operator float(FP48D16 value) => (float)(value._v * EpsilonDouble);

    public static implicit operator double(FP48D16 value) => value._v * EpsilonDouble;

    public static implicit operator decimal(FP48D16 value) => value._v * EpsilonDecimal;

    public static implicit operator FP48D16(byte value) => new(value << DecimalBits);

    public static implicit operator FP48D16(char value) => new(value << DecimalBits);

    public static implicit operator FP48D16(short value) => new(value << DecimalBits);

    public static implicit operator FP48D16(int value) => new(value << DecimalBits);

    public static explicit operator FP48D16(long value)
    {
        FP48D16 v = new(value << DecimalBits);
        if ((v._v & UnusedBitsMask) is not 0)
            throw new OverflowException();

        return v;
    }

    public static implicit operator FP48D16(sbyte value) => new(value << DecimalBits);

    public static implicit operator FP48D16(ushort value) => new(value << DecimalBits);

    public static explicit operator FP48D16(uint value) => new(value << DecimalBits);

    public static explicit operator FP48D16(ulong value)
    {
        FP48D16 v = new((long)value << DecimalBits);
        if ((v._v & UnusedBitsMask) is not 0)
            throw new OverflowException();

        return v;
    }

    public static explicit operator FP48D16(float value) => new(Convert.ToInt32(value * ValueOne));

    public static explicit operator FP48D16(double value) => new(Convert.ToInt32(value * ValueOne));

    public static explicit operator FP48D16(decimal value) => new(Convert.ToInt32(value * ValueOne));
}
