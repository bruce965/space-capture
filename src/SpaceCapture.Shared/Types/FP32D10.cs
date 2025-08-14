using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;
using SpaceCapture.Shared.Abstractions;

namespace SpaceCapture.Shared.Types;

// Note: why not just use floats or doubles? Because they are not deterministic.
// We want the simulation to be exactly the same on the server and on each of
// the clients, which might be running on different hardware architectures.

/// <summary>
/// 32-bit signed fixed point number with 10 bits of decimal precision.
/// </summary>
[JsonConverter(typeof(JsonConverter))]
public readonly struct FP32D10
    : IBinaryInteger<FP32D10>,
        ITrigonometricFunctions<FP32D10>,
        IConvertible,
        IImmutable
{
    public class JsonConverter : JsonConverter<FP32D10>
    {
        public override FP32D10 Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options
        ) => new(Convert.ToInt32(reader.GetDecimal() * ValueOne));

        public override void Write(
            Utf8JsonWriter writer,
            FP32D10 value,
            JsonSerializerOptions options
        ) => writer.WriteNumberValue((decimal)value._v / ValueOne);
    }

    const int BinaryDecimalDigits = 10;
    const int ValueOne = 1 << BinaryDecimalDigits;
    const int DecimalBitsMask = ValueOne - 1;
    const int IntegerBitsMask = ~DecimalBitsMask;
    const int ValueE = (int)(Math.E * ValueOne + .5);
    const int ValuePi = (int)(Math.PI * ValueOne + .5);
    const int ValueHalfPi = (int)(Math.PI / 2 * ValueOne + .5);
    const int ValueTau = (int)(Math.Tau * ValueOne + .5);

    #region Sin LUT

    /*
    for (int i = 0; i <= ValueHalfPi; i++)
        Console.WriteLine($"{(int)(Math.Round(Math.Sin((double)i / ValueOne) * ValueOne))},");
    */

    // csharpier-ignore
    static readonly int[] s_sinLut =
    [
        0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29,
        30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57,
        58, 59, 60, 61, 62, 63, 64, 65, 66, 67, 68, 69, 70, 71, 72, 73, 74, 75, 76, 77, 78, 79, 80, 81, 82, 83, 84, 85,
        86, 87, 88, 89, 90, 91, 92, 93, 94, 95, 96, 97, 98, 99, 100, 101, 102, 103, 104, 105, 106, 107, 108, 109, 110,
        111, 112, 113, 114, 115, 116, 117, 118, 119, 120, 121, 122, 123, 124, 125, 126, 127, 128, 129, 130, 131, 132,
        133, 134, 135, 136, 137, 138, 139, 140, 141, 142, 143, 144, 145, 146, 146, 147, 148, 149, 150, 151, 152, 153,
        154, 155, 156, 157, 158, 159, 160, 161, 162, 163, 164, 165, 166, 167, 168, 169, 170, 171, 172, 173, 174, 175,
        176, 177, 178, 179, 180, 181, 182, 183, 184, 185, 186, 187, 188, 189, 190, 191, 192, 193, 194, 195, 196, 197,
        198, 199, 200, 201, 202, 203, 204, 205, 206, 207, 208, 209, 210, 210, 211, 212, 213, 214, 215, 216, 217, 218,
        219, 220, 221, 222, 223, 224, 225, 226, 227, 228, 229, 230, 231, 232, 233, 234, 235, 236, 237, 238, 239, 240,
        241, 242, 243, 244, 245, 246, 247, 248, 248, 249, 250, 251, 252, 253, 254, 255, 256, 257, 258, 259, 260, 261,
        262, 263, 264, 265, 266, 267, 268, 269, 270, 271, 272, 273, 274, 275, 276, 277, 277, 278, 279, 280, 281, 282,
        283, 284, 285, 286, 287, 288, 289, 290, 291, 292, 293, 294, 295, 296, 297, 298, 299, 300, 301, 301, 302, 303,
        304, 305, 306, 307, 308, 309, 310, 311, 312, 313, 314, 315, 316, 317, 318, 319, 320, 321, 321, 322, 323, 324,
        325, 326, 327, 328, 329, 330, 331, 332, 333, 334, 335, 336, 337, 338, 339, 339, 340, 341, 342, 343, 344, 345,
        346, 347, 348, 349, 350, 351, 352, 353, 354, 355, 355, 356, 357, 358, 359, 360, 361, 362, 363, 364, 365, 366,
        367, 368, 369, 369, 370, 371, 372, 373, 374, 375, 376, 377, 378, 379, 380, 381, 382, 382, 383, 384, 385, 386,
        387, 388, 389, 390, 391, 392, 393, 394, 395, 395, 396, 397, 398, 399, 400, 401, 402, 403, 404, 405, 406, 406,
        407, 408, 409, 410, 411, 412, 413, 414, 415, 416, 417, 417, 418, 419, 420, 421, 422, 423, 424, 425, 426, 427,
        427, 428, 429, 430, 431, 432, 433, 434, 435, 436, 437, 437, 438, 439, 440, 441, 442, 443, 444, 445, 446, 446,
        447, 448, 449, 450, 451, 452, 453, 454, 455, 455, 456, 457, 458, 459, 460, 461, 462, 463, 464, 464, 465, 466,
        467, 468, 469, 470, 471, 472, 472, 473, 474, 475, 476, 477, 478, 479, 479, 480, 481, 482, 483, 484, 485, 486,
        487, 487, 488, 489, 490, 491, 492, 493, 494, 494, 495, 496, 497, 498, 499, 500, 501, 501, 502, 503, 504, 505,
        506, 507, 508, 508, 509, 510, 511, 512, 513, 514, 514, 515, 516, 517, 518, 519, 520, 520, 521, 522, 523, 524,
        525, 526, 527, 527, 528, 529, 530, 531, 532, 533, 533, 534, 535, 536, 537, 538, 538, 539, 540, 541, 542, 543,
        544, 544, 545, 546, 547, 548, 549, 549, 550, 551, 552, 553, 554, 555, 555, 556, 557, 558, 559, 560, 560, 561,
        562, 563, 564, 565, 565, 566, 567, 568, 569, 570, 570, 571, 572, 573, 574, 575, 575, 576, 577, 578, 579, 580,
        580, 581, 582, 583, 584, 584, 585, 586, 587, 588, 589, 589, 590, 591, 592, 593, 593, 594, 595, 596, 597, 598,
        598, 599, 600, 601, 602, 602, 603, 604, 605, 606, 606, 607, 608, 609, 610, 610, 611, 612, 613, 614, 614, 615,
        616, 617, 618, 618, 619, 620, 621, 622, 622, 623, 624, 625, 626, 626, 627, 628, 629, 630, 630, 631, 632, 633,
        633, 634, 635, 636, 637, 637, 638, 639, 640, 641, 641, 642, 643, 644, 644, 645, 646, 647, 648, 648, 649, 650,
        651, 651, 652, 653, 654, 654, 655, 656, 657, 658, 658, 659, 660, 661, 661, 662, 663, 664, 664, 665, 666, 667,
        667, 668, 669, 670, 670, 671, 672, 673, 673, 674, 675, 676, 677, 677, 678, 679, 679, 680, 681, 682, 682, 683,
        684, 685, 685, 686, 687, 688, 688, 689, 690, 691, 691, 692, 693, 694, 694, 695, 696, 697, 697, 698, 699, 699,
        700, 701, 702, 702, 703, 704, 705, 705, 706, 707, 707, 708, 709, 710, 710, 711, 712, 712, 713, 714, 715, 715,
        716, 717, 718, 718, 719, 720, 720, 721, 722, 722, 723, 724, 725, 725, 726, 727, 727, 728, 729, 730, 730, 731,
        732, 732, 733, 734, 734, 735, 736, 737, 737, 738, 739, 739, 740, 741, 741, 742, 743, 743, 744, 745, 745, 746,
        747, 748, 748, 749, 750, 750, 751, 752, 752, 753, 754, 754, 755, 756, 756, 757, 758, 758, 759, 760, 760, 761,
        762, 762, 763, 764, 764, 765, 766, 766, 767, 768, 768, 769, 770, 770, 771, 772, 772, 773, 774, 774, 775, 776,
        776, 777, 778, 778, 779, 780, 780, 781, 781, 782, 783, 783, 784, 785, 785, 786, 787, 787, 788, 789, 789, 790,
        790, 791, 792, 792, 793, 794, 794, 795, 795, 796, 797, 797, 798, 799, 799, 800, 801, 801, 802, 802, 803, 804,
        804, 805, 805, 806, 807, 807, 808, 809, 809, 810, 810, 811, 812, 812, 813, 813, 814, 815, 815, 816, 816, 817,
        818, 818, 819, 819, 820, 821, 821, 822, 822, 823, 824, 824, 825, 825, 826, 827, 827, 828, 828, 829, 830, 830,
        831, 831, 832, 832, 833, 834, 834, 835, 835, 836, 837, 837, 838, 838, 839, 839, 840, 841, 841, 842, 842, 843,
        843, 844, 845, 845, 846, 846, 847, 847, 848, 848, 849, 850, 850, 851, 851, 852, 852, 853, 853, 854, 855, 855,
        856, 856, 857, 857, 858, 858, 859, 859, 860, 861, 861, 862, 862, 863, 863, 864, 864, 865, 865, 866, 866, 867,
        868, 868, 869, 869, 870, 870, 871, 871, 872, 872, 873, 873, 874, 874, 875, 875, 876, 876, 877, 878, 878, 879,
        879, 880, 880, 881, 881, 882, 882, 883, 883, 884, 884, 885, 885, 886, 886, 887, 887, 888, 888, 889, 889, 890,
        890, 891, 891, 892, 892, 893, 893, 894, 894, 895, 895, 896, 896, 896, 897, 897, 898, 898, 899, 899, 900, 900,
        901, 901, 902, 902, 903, 903, 904, 904, 905, 905, 906, 906, 906, 907, 907, 908, 908, 909, 909, 910, 910, 911,
        911, 912, 912, 912, 913, 913, 914, 914, 915, 915, 916, 916, 916, 917, 917, 918, 918, 919, 919, 920, 920, 920,
        921, 921, 922, 922, 923, 923, 923, 924, 924, 925, 925, 926, 926, 926, 927, 927, 928, 928, 929, 929, 929, 930,
        930, 931, 931, 932, 932, 932, 933, 933, 934, 934, 934, 935, 935, 936, 936, 936, 937, 937, 938, 938, 938, 939,
        939, 940, 940, 940, 941, 941, 942, 942, 942, 943, 943, 944, 944, 944, 945, 945, 946, 946, 946, 947, 947, 947,
        948, 948, 949, 949, 949, 950, 950, 950, 951, 951, 952, 952, 952, 953, 953, 953, 954, 954, 954, 955, 955, 956,
        956, 956, 957, 957, 957, 958, 958, 958, 959, 959, 959, 960, 960, 961, 961, 961, 962, 962, 962, 963, 963, 963,
        964, 964, 964, 965, 965, 965, 966, 966, 966, 967, 967, 967, 968, 968, 968, 969, 969, 969, 970, 970, 970, 970,
        971, 971, 971, 972, 972, 972, 973, 973, 973, 974, 974, 974, 975, 975, 975, 975, 976, 976, 976, 977, 977, 977,
        978, 978, 978, 978, 979, 979, 979, 980, 980, 980, 981, 981, 981, 981, 982, 982, 982, 983, 983, 983, 983, 984,
        984, 984, 984, 985, 985, 985, 986, 986, 986, 986, 987, 987, 987, 987, 988, 988, 988, 988, 989, 989, 989, 990,
        990, 990, 990, 991, 991, 991, 991, 992, 992, 992, 992, 993, 993, 993, 993, 994, 994, 994, 994, 994, 995, 995,
        995, 995, 996, 996, 996, 996, 997, 997, 997, 997, 997, 998, 998, 998, 998, 999, 999, 999, 999, 999, 1000, 1000,
        1000, 1000, 1001, 1001, 1001, 1001, 1001, 1002, 1002, 1002, 1002, 1002, 1003, 1003, 1003, 1003, 1003, 1004,
        1004, 1004, 1004, 1004, 1005, 1005, 1005, 1005, 1005, 1006, 1006, 1006, 1006, 1006, 1007, 1007, 1007, 1007,
        1007, 1007, 1008, 1008, 1008, 1008, 1008, 1008, 1009, 1009, 1009, 1009, 1009, 1010, 1010, 1010, 1010, 1010,
        1010, 1010, 1011, 1011, 1011, 1011, 1011, 1011, 1012, 1012, 1012, 1012, 1012, 1012, 1013, 1013, 1013, 1013,
        1013, 1013, 1013, 1014, 1014, 1014, 1014, 1014, 1014, 1014, 1015, 1015, 1015, 1015, 1015, 1015, 1015, 1015,
        1016, 1016, 1016, 1016, 1016, 1016, 1016, 1016, 1017, 1017, 1017, 1017, 1017, 1017, 1017, 1017, 1017, 1018,
        1018, 1018, 1018, 1018, 1018, 1018, 1018, 1018, 1019, 1019, 1019, 1019, 1019, 1019, 1019, 1019, 1019, 1019,
        1020, 1020, 1020, 1020, 1020, 1020, 1020, 1020, 1020, 1020, 1020, 1021, 1021, 1021, 1021, 1021, 1021, 1021,
        1021, 1021, 1021, 1021, 1021, 1021, 1022, 1022, 1022, 1022, 1022, 1022, 1022, 1022, 1022, 1022, 1022, 1022,
        1022, 1022, 1022, 1022, 1022, 1023, 1023, 1023, 1023, 1023, 1023, 1023, 1023, 1023, 1023, 1023, 1023, 1023,
        1023, 1023, 1023, 1023, 1023, 1023, 1023, 1023, 1023, 1023, 1024, 1024, 1024, 1024, 1024, 1024, 1024, 1024,
        1024, 1024, 1024, 1024, 1024, 1024, 1024, 1024, 1024, 1024, 1024, 1024, 1024, 1024, 1024, 1024, 1024, 1024,
        1024, 1024, 1024, 1024, 1024, 1024,
    ];

    #endregion

    readonly int _v;

    private FP32D10(int value) => _v = value;

    public static FP32D10 AdditiveIdentity => new(0);

    public static FP32D10 MultiplicativeIdentity => new(ValueOne);

    public static FP32D10 One => new(ValueOne);

    public static int Radix => 10;

    public static FP32D10 Zero => new(0);

    public static FP32D10 E => new(ValueE);

    public static FP32D10 Pi => new(ValuePi);

    public static FP32D10 Tau => new(ValueTau);

    public static FP32D10 Abs(FP32D10 value) => new(int.Abs(value._v));

    public static bool IsCanonical(FP32D10 value) => true;

    public static bool IsComplexNumber(FP32D10 value) => false;

    public static bool IsEvenInteger(FP32D10 value) =>
        (value._v & (ValueOne | DecimalBitsMask)) is ValueOne;

    public static bool IsFinite(FP32D10 value) => true;

    public static bool IsImaginaryNumber(FP32D10 value) => false;

    public static bool IsInfinity(FP32D10 value) => false;

    public static bool IsInteger(FP32D10 value) => true;

    public static bool IsNaN(FP32D10 value) => false;

    public static bool IsNegative(FP32D10 value) => value._v < 0;

    public static bool IsNegativeInfinity(FP32D10 value) => false;

    public static bool IsNormal(FP32D10 value) => value._v is not 0;

    public static bool IsOddInteger(FP32D10 value) =>
        (value._v & (ValueOne | DecimalBitsMask)) is 0;

    public static bool IsPositive(FP32D10 value) => value._v >= 0;

    public static bool IsPositiveInfinity(FP32D10 value) => false;

    public static bool IsPow2(FP32D10 value) =>
        BitOperations.PopCount(unchecked((ulong)(value._v >> BinaryDecimalDigits))) is 1;

    public static bool IsRealNumber(FP32D10 value) => true;

    public static bool IsSubnormal(FP32D10 value) => value._v is not 0;

    public static bool IsZero(FP32D10 value) => value._v is 0;

    public static FP32D10 Log2(FP32D10 value) => throw new NotImplementedException();

    public static FP32D10 MaxMagnitude(FP32D10 x, FP32D10 y) => Abs(x) > Abs(y) ? x : y;

    public static FP32D10 MaxMagnitudeNumber(FP32D10 x, FP32D10 y) => MaxMagnitude(x, y);

    public static FP32D10 MinMagnitude(FP32D10 x, FP32D10 y) => Abs(x) < Abs(y) ? x : y;

    public static FP32D10 MinMagnitudeNumber(FP32D10 x, FP32D10 y) => MinMagnitudeNumber(x, y);

    public static FP32D10 Parse(
        ReadOnlySpan<char> s,
        NumberStyles style,
        IFormatProvider? provider
    ) => throw new NotImplementedException();

    public static FP32D10 Parse(string s, NumberStyles style, IFormatProvider? provider) =>
        throw new NotImplementedException();

    public static FP32D10 Parse(ReadOnlySpan<char> s, IFormatProvider? provider) =>
        throw new NotImplementedException();

    public static FP32D10 Parse(string s, IFormatProvider? provider) =>
        throw new NotImplementedException();

    public static FP32D10 PopCount(FP32D10 value) => int.PopCount(value._v);

    public static FP32D10 TrailingZeroCount(FP32D10 value) => int.TrailingZeroCount(value._v);

    public static bool TryConvertFromChecked<TOther>(
        TOther value,
        [MaybeNullWhen(false)] out FP32D10 result
    )
        where TOther : INumberBase<TOther> => throw new NotImplementedException();

    public static bool TryConvertFromSaturating<TOther>(
        TOther value,
        [MaybeNullWhen(false)] out FP32D10 result
    )
        where TOther : INumberBase<TOther> => throw new NotImplementedException();

    public static bool TryConvertFromTruncating<TOther>(
        TOther value,
        [MaybeNullWhen(false)] out FP32D10 result
    )
        where TOther : INumberBase<TOther> => throw new NotImplementedException();

    public static bool TryConvertToChecked<TOther>(
        FP32D10 value,
        [MaybeNullWhen(false)] out TOther result
    )
        where TOther : INumberBase<TOther> => throw new NotImplementedException();

    public static bool TryConvertToSaturating<TOther>(
        FP32D10 value,
        [MaybeNullWhen(false)] out TOther result
    )
        where TOther : INumberBase<TOther> => throw new NotImplementedException();

    public static bool TryConvertToTruncating<TOther>(
        FP32D10 value,
        [MaybeNullWhen(false)] out TOther result
    )
        where TOther : INumberBase<TOther> => throw new NotImplementedException();

    public static bool TryParse(
        ReadOnlySpan<char> s,
        NumberStyles style,
        IFormatProvider? provider,
        [MaybeNullWhen(false)] out FP32D10 result
    ) => throw new NotImplementedException();

    public static bool TryParse(
        [NotNullWhen(true)] string? s,
        NumberStyles style,
        IFormatProvider? provider,
        [MaybeNullWhen(false)] out FP32D10 result
    ) => throw new NotImplementedException();

    public static bool TryParse(
        ReadOnlySpan<char> s,
        IFormatProvider? provider,
        [MaybeNullWhen(false)] out FP32D10 result
    ) => throw new NotImplementedException();

    public static bool TryParse(
        [NotNullWhen(true)] string? s,
        IFormatProvider? provider,
        [MaybeNullWhen(false)] out FP32D10 result
    ) => throw new NotImplementedException();

    public static bool TryReadBigEndian(
        ReadOnlySpan<byte> source,
        bool isUnsigned,
        out FP32D10 value
    ) => throw new NotImplementedException();

    public static bool TryReadLittleEndian(
        ReadOnlySpan<byte> source,
        bool isUnsigned,
        out FP32D10 value
    ) => throw new NotImplementedException();

    public int CompareTo(object? obj) => throw new NotImplementedException();

    public int CompareTo(FP32D10 other) => _v.CompareTo(other._v);

    public override bool Equals(object? obj) => obj is FP32D10 v && Equals(v);

    public bool Equals(FP32D10 other) => _v.Equals(other._v);

    public override int GetHashCode() => _v.GetHashCode();

    public int GetByteCount() => sizeof(int);

    public int GetShortestBitLength() => throw new NotImplementedException();

    public string ToString(string? format, IFormatProvider? formatProvider) =>
        throw new NotImplementedException();

    public bool TryFormat(
        Span<char> destination,
        out int charsWritten,
        ReadOnlySpan<char> format,
        IFormatProvider? provider
    ) => throw new NotImplementedException();

    public bool TryWriteBigEndian(Span<byte> destination, out int bytesWritten) =>
        throw new NotImplementedException();

    public bool TryWriteLittleEndian(Span<byte> destination, out int bytesWritten) =>
        throw new NotImplementedException();

    public TypeCode GetTypeCode() => TypeCode.Object;

    public bool ToBoolean(IFormatProvider? provider) => (bool)this;

    public byte ToByte(IFormatProvider? provider) => (byte)this;

    public char ToChar(IFormatProvider? provider) => (char)this;

    public DateTime ToDateTime(IFormatProvider? provider) => throw new NotImplementedException();

    public decimal ToDecimal(IFormatProvider? provider) => throw new NotImplementedException();

    public double ToDouble(IFormatProvider? provider) => throw new NotImplementedException();

    public short ToInt16(IFormatProvider? provider) => (short)(_v >> BinaryDecimalDigits);

    public int ToInt32(IFormatProvider? provider) => _v >> BinaryDecimalDigits;

    public long ToInt64(IFormatProvider? provider) => _v >> BinaryDecimalDigits;

    public sbyte ToSByte(IFormatProvider? provider) => (sbyte)(_v >> BinaryDecimalDigits);

    public float ToSingle(IFormatProvider? provider) => throw new NotImplementedException();

    public string ToString(IFormatProvider? provider) => throw new NotImplementedException();

    public object ToType(Type conversionType, IFormatProvider? provider) =>
        throw new NotImplementedException();

    public ushort ToUInt16(IFormatProvider? provider) => (ushort)(_v >> BinaryDecimalDigits);

    public uint ToUInt32(IFormatProvider? provider) => (uint)(_v >> BinaryDecimalDigits);

    public ulong ToUInt64(IFormatProvider? provider) => (ulong)(_v >> BinaryDecimalDigits);

    public static FP32D10 Acos(FP32D10 x) => throw new NotImplementedException();

    public static FP32D10 AcosPi(FP32D10 x) => throw new NotImplementedException();

    public static FP32D10 Asin(FP32D10 x) => throw new NotImplementedException();

    public static FP32D10 AsinPi(FP32D10 x) => throw new NotImplementedException();

    public static FP32D10 Atan(FP32D10 x) => throw new NotImplementedException();

    public static FP32D10 AtanPi(FP32D10 x) => throw new NotImplementedException();

    public static FP32D10 Cos(FP32D10 x) => Sin(x - ValueHalfPi);

    public static FP32D10 CosPi(FP32D10 x) => throw new NotImplementedException();

    public static FP32D10 Sin(FP32D10 x)
    {
        int i = (x._v % ValueTau) is var r && r < 0 ? r + ValueTau : r;
        return i <= ValuePi ? SinUpToHalfPi(i) : -SinUpToHalfPi(i - ValuePi);

        static FP32D10 SinUpToHalfPi(int i)
        {
            Debug.Assert(i <= ValuePi);

            if (i <= ValueHalfPi)
                return s_sinLut[i];

            return s_sinLut[ValuePi - i];
        }
    }

    public static (FP32D10 Sin, FP32D10 Cos) SinCos(FP32D10 x) => (Sin(x), Cos(x));

    public static (FP32D10 SinPi, FP32D10 CosPi) SinCosPi(FP32D10 x) =>
        throw new NotImplementedException();

    public static FP32D10 SinPi(FP32D10 x) => throw new NotImplementedException();

    public static FP32D10 Tan(FP32D10 x) => throw new NotImplementedException();

    public static FP32D10 TanPi(FP32D10 x) => throw new NotImplementedException();

    public static FP32D10 operator +(FP32D10 value) => new(+value._v);

    public static FP32D10 operator +(FP32D10 left, FP32D10 right) => new(left._v + right._v);

    public static FP32D10 operator -(FP32D10 value) => new(-value._v);

    public static FP32D10 operator -(FP32D10 left, FP32D10 right) => new(left._v - right._v);

    public static FP32D10 operator ~(FP32D10 value) => new(~value._v);

    public static FP32D10 operator ++(FP32D10 value) => new(value._v + ValueOne);

    public static FP32D10 operator --(FP32D10 value) => new(value._v - ValueOne);

    public static FP32D10 operator *(FP32D10 left, FP32D10 right) =>
        new((int)(((long)left._v * right._v) >> BinaryDecimalDigits));

    public static FP32D10 operator /(FP32D10 left, FP32D10 right) =>
        throw new NotImplementedException();

    public static FP32D10 operator %(FP32D10 left, FP32D10 right) => new(left._v % right._v);

    public static FP32D10 operator &(FP32D10 left, FP32D10 right) => new(left._v & right._v);

    public static FP32D10 operator |(FP32D10 left, FP32D10 right) => new(left._v | right._v);

    public static FP32D10 operator ^(FP32D10 left, FP32D10 right) =>
        throw new NotImplementedException();

    public static FP32D10 operator <<(FP32D10 value, int shiftAmount) =>
        new(value._v << shiftAmount);

    public static FP32D10 operator >>(FP32D10 value, int shiftAmount) =>
        new(value._v >> shiftAmount);

    public static bool operator ==(FP32D10 left, FP32D10 right) => left._v == right._v;

    public static bool operator !=(FP32D10 left, FP32D10 right) => left._v != right._v;

    public static bool operator <(FP32D10 left, FP32D10 right) => left._v < right._v;

    public static bool operator >(FP32D10 left, FP32D10 right) => left._v > right._v;

    public static bool operator <=(FP32D10 left, FP32D10 right) => left._v <= right._v;

    public static bool operator >=(FP32D10 left, FP32D10 right) => left._v >= right._v;

    public static FP32D10 operator >>>(FP32D10 value, int shiftAmount) =>
        new(value._v >>> shiftAmount);

    public static explicit operator bool(FP32D10 value) => value._v is not 0;

    public static explicit operator byte(FP32D10 value) => (byte)(value._v >> BinaryDecimalDigits);

    public static explicit operator char(FP32D10 value) => (char)(value._v >> BinaryDecimalDigits);

    public static explicit operator short(FP32D10 value) =>
        (short)(value._v >> BinaryDecimalDigits);

    public static explicit operator int(FP32D10 value) => value._v >> BinaryDecimalDigits;

    public static explicit operator long(FP32D10 value) => value._v >> BinaryDecimalDigits;

    public static explicit operator sbyte(FP32D10 value) =>
        (sbyte)(value._v >> BinaryDecimalDigits);

    public static explicit operator ushort(FP32D10 value) =>
        (ushort)(value._v >> BinaryDecimalDigits);

    public static explicit operator uint(FP32D10 value) => (uint)(value._v >> BinaryDecimalDigits);

    public static explicit operator ulong(FP32D10 value) =>
        (ulong)(value._v >> BinaryDecimalDigits);

    public static implicit operator FP32D10(byte value) => new(value << BinaryDecimalDigits);

    public static implicit operator FP32D10(char value) => new(value << BinaryDecimalDigits);

    public static implicit operator FP32D10(short value) => new(value << BinaryDecimalDigits);

    public static implicit operator FP32D10(int value) => new(value << BinaryDecimalDigits);

    public static explicit operator FP32D10(long value) => new((int)(value << BinaryDecimalDigits));

    public static implicit operator FP32D10(sbyte value) => new(value << BinaryDecimalDigits);

    public static implicit operator FP32D10(ushort value) => new(value << BinaryDecimalDigits);

    public static explicit operator FP32D10(uint value) => new((int)(value << BinaryDecimalDigits));

    public static explicit operator FP32D10(ulong value) =>
        new((int)(value << BinaryDecimalDigits));
}
