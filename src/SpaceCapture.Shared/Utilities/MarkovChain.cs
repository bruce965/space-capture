// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: MIT

//#define SERIALIZATION

using System.Buffers;
#if SERIALIZATION
using System.Text;
#endif

namespace SpaceCapture.Shared.Utilities;

public class MarkovChain
{
    readonly int _minLength;
    readonly int _maxLength;
    readonly (string Sequence, uint Weight)[] _starters;
    readonly Dictionary<string, (char Next, uint Weight)[]> _weights;

    public MarkovChain(ReadOnlySpan<char> weights)
    {
        Deserialize(
            weights,
            out _minLength,
            out _maxLength,
            out (string Sequence, int Count)[]? starters,
            out Dictionary<string, (char Next, int Count)[]>? sequences
        );

        _starters = CountsToWeights(starters);

        _weights = sequences
            .Select(kvp => KeyValuePair.Create(kvp.Key, CountsToWeights(kvp.Value)))
            .OrderBy(kvp => kvp.Key)
            .ToDictionary();
    }

#if SERIALIZATION
    static string Serialize(
        int minLength,
        int maxLength,
        (string Sequence, int Count)[] starters,
        Dictionary<string, (char Next, int Count)[]> sequences
    )
    {
        StringBuilder sb = new();

        WriteInt(sb, starters[0].Sequence.Length);

        #region minLength

        WriteInt(sb, minLength);

        #endregion

        #region maxLength

        WriteInt(sb, maxLength);

        #endregion

        #region starters

        WriteIntLong(sb, starters.Length);

        foreach ((string sequence, int weight) in starters)
        {
            WriteString(sb, sequence);
            WriteInt(sb, weight);
        }

        #endregion

        #region sequences

        WriteIntLong(sb, sequences.Count);

        foreach ((string sequence, (char Next, int Count)[] values) in sequences)
        {
            WriteString(sb, sequence);
            WriteInt(sb, values.Length);

            foreach ((char next, int count) in values)
            {
                WriteString(sb, [next]);
                WriteInt(sb, count);
            }
        }

        #endregion

        return sb.ToString();
    }
#endif

    static void Deserialize(
        ReadOnlySpan<char> serialized,
        out int minLength,
        out int maxLength,
        out (string Sequence, int Count)[] starters,
        out Dictionary<string, (char Next, int Count)[]> sequences
    )
    {
        int p = 0;

        int sequenceLength = ReadInt(serialized, ref p);

        #region minLength

        minLength = ReadInt(serialized, ref p);

        #endregion

        #region maxLength

        maxLength = ReadInt(serialized, ref p);

        #endregion

        #region starters

        int startersCount = ReadIntLong(serialized, ref p);

        starters = new (string Sequence, int Count)[startersCount];

        for (int i = 0; i < starters.Length; i++)
        {
            string sequence = ReadString(serialized, sequenceLength, ref p);
            int weight = ReadInt(serialized, ref p);
            starters[i] = (sequence, weight);
        }

        #endregion

        #region sequences

        int weightsCount = ReadIntLong(serialized, ref p);

        sequences = new(weightsCount);

        for (int i = 0; i < weightsCount; i++)
        {
            string sequence = ReadString(serialized, sequenceLength, ref p);
            int valuesCount = ReadInt(serialized, ref p);

            (char Next, int Count)[] values = new (char Next, int Count)[valuesCount];
            for (int j = 0; j < values.Length; j++)
            {
                char next = ReadString(serialized, 1, ref p)[0];
                int count = ReadInt(serialized, ref p);
                values[j] = (next, count);
            }

            sequences[sequence] = values;
        }

        #endregion
    }

    public string Generate(ref DeterministicRandom rand)
    {
        char[] result = ArrayPool<char>.Shared.Rent(_maxLength + 1);
        try
        {
            while (true)
            {
                string current = Get(ref rand, _starters);
                current.CopyTo(result);
                int resultIndex = current.Length;

                while (resultIndex <= _maxLength)
                {
                    char chr = Get(ref rand, _weights[current]);
                    if (chr == '\0')
                    {
                        if (resultIndex >= _minLength)
                            return new(result[..resultIndex]);

                        break;
                    }

                    result[resultIndex++] = chr;
                    current = new(result, resultIndex - current.Length, current.Length);
                }
            }
        }
        finally
        {
            ArrayPool<char>.Shared.Return(result);
        }
    }

#if SERIALIZATION
    public static string BuildWeights(IEnumerable<string> samples, int window)
    {
        Dictionary<string, int> rawStarters = [];
        Dictionary<string, Dictionary<char, int>> rawCounts = [];

        foreach (string sample in samples)
        {
            if (sample.Length < window)
                continue; // Ignore samples shorter than the window.

            string start = sample[..window];
            rawStarters[start] = rawStarters.TryGetValue(start, out int sc) ? sc + 1 : 1;

            for (int i = 0; i <= sample.Length - window; i++)
            {
                string seq = sample.Substring(i, window);
                char next = sample.Length > i + window ? sample[i + window] : '\0';
                if (!rawCounts.TryGetValue(seq, out Dictionary<char, int>? counts))
                    rawCounts[seq] = counts = [];

                counts[next] = counts.TryGetValue(next, out int c) ? c + 1 : 1;
            }
        }

        int minLength = samples.Min(s => s.Length);
        int maxLength = samples.Max(s => s.Length);

        (string Sequence, int Count)[] starters = [.. rawStarters.Select(kvp => (kvp.Key, kvp.Value))];

        Dictionary<string, (char Next, int Count)[]> sequences = rawCounts
            .Select(kvp => KeyValuePair.Create(kvp.Key, kvp.Value.Select(kvp2 => (kvp2.Key, kvp2.Value)).ToArray()))
            .ToDictionary();

        return Serialize(minLength, maxLength, starters, sequences);
    }
#endif

    static (T Value, uint Weight)[] CountsToWeights<T>((T Value, int Count)[] counts)
        where T : notnull
    {
        (T Value, uint Weight)[] weights = new (T, uint)[counts.Length];

        int total = counts.Sum(c => c.Count);
        decimal share = (decimal)uint.MaxValue / total;

        int i = 0;
        uint distributed = 0;
        foreach ((T value, int count) in counts.OrderByDescending(c => c.Count))
        {
            uint weight = (uint)(share * count);
            weights[i++] = (value, weight);
            distributed += weight;
        }

        uint remainder = uint.MaxValue - distributed;

        // TODO: not very clearn; should be redistributed by weight instead of all to the first.
        weights[0].Weight += remainder;

        return weights;
    }

    static T Get<T>(ref DeterministicRandom rand, (T Value, uint Weight)[] weights)
    {
        uint random = (uint)rand.Next();

        for (int i = 0; ; i++)
        {
            if (weights[i].Weight >= random)
                return weights[i].Value;

            random -= weights[i].Weight;
        }
    }

#if SERIALIZATION
    static void WriteIntLong(StringBuilder sb, int value)
    {
        int lengthIndex = sb.Length;
        sb.Append('\0');

        int length = 0;
        while (value > 0)
        {
            value = Math.DivRem(value, 62, out int remainder);
            sb.Append(ToBase62(remainder));
            length++;
        }

        sb[lengthIndex] = ToBase62(length);
    }

    static void WriteInt(StringBuilder sb, int value)
    {
        char chr = value < 62 ? ToBase62(value) : '-';

        sb.Append(chr);

        if (chr is '-')
            WriteIntLong(sb, value);
    }

    static char ToBase62(int value)
    {
        int chr = value switch
        {
            < 10 => '0' + value,
            < 36 => 'a' + value - 10,
            < 62 => 'A' + value - 36,
            _ => throw new ArgumentException(null, nameof(value)),
        };

        return (char)chr;
    }

    static void WriteString(StringBuilder sb, ReadOnlySpan<char> value)
    {
        int p = 0;
        Span<char> buff = stackalloc char[value.Length * 2];

        foreach (char c in value)
        {
            switch (c)
            {
                case '\0':
                    buff[p++] = '$';
                    buff[p++] = '0';
                    break;

                case '$':
                    buff[p++] = '$';
                    buff[p++] = '$';
                    break;

                default:
                    buff[p++] = c;
                    break;
            }
        }

        sb.Append(buff[..p]);
    }
#endif

    static int ReadIntLong(ReadOnlySpan<char> buffer, ref int p)
    {
        int value = 0;
        int multiplier = 1;
        int length = FromBase62(buffer.Slice(p++, 1)[0]);
        for (int i = 0; i < length; i++)
        {
            value += FromBase62(buffer.Slice(p++, 1)[0]) * multiplier;
            multiplier *= 62;
        }

        return value;
    }

    static int ReadInt(ReadOnlySpan<char> buffer, ref int p)
    {
        char chr = buffer[p++];

        if (chr is '-')
            return ReadIntLong(buffer, ref p);

        return FromBase62(chr);
    }

    static int FromBase62(char chr)
    {
        return chr switch
        {
            >= '0' and <= '9' => chr - '0',
            >= 'a' and <= 'z' => chr - 'a' + 10,
            >= 'A' and <= 'Z' => chr - 'A' + 36,
            _ => throw new FormatException(),
        };
    }

    static string ReadString(ReadOnlySpan<char> buffer, int length, ref int p)
    {
        Span<char> chars = stackalloc char[length];

        while (length > 0)
        {
            char chr = buffer[p++];
            if (chr is '$')
            {
                char escaped = buffer[p++];
                chr = escaped switch
                {
                    '0' => '\0',
                    '$' => '$',
                    _ => throw new FormatException(),
                };
            }

            chars[^(length--)] = chr;
        }

        return new(chars);
    }
}
