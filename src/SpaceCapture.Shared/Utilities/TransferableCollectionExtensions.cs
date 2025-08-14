using SpaceCapture.Shared.Abstractions;

namespace SpaceCapture.Shared.Utilities;

public static class TransferableCollectionExtensions
{
    /// <inheritdoc cref="ITransferable{T}.CopyFrom(T)"/>
    public static void CopyFrom<T>(this T[] value, T[] other)
        where T : ICloneable<T>, ITransferable<T>
    {
        ArgumentOutOfRangeException.ThrowIfNotEqual(value.Length, other.Length);

        for (int i = 0; i < value.Length; i++)
        {
            if (value[i] is { } v && other[i] is { } o)
                v.CopyFrom(o);
            else
                value[i] = other[i] is { } o2 ? o2.Clone() : default!;
        }
    }

    /// <inheritdoc cref="ITransferable{T}.CopyFrom(T)"/>
    public static void CopyFrom<T>(this List<T> value, List<T> other)
        where T : ICloneable<T>, ITransferable<T>
    {
        for (int i = 0; i < value.Count && i < other.Count; i++)
        {
            if (value[i] is { } v && other[i] is { } o)
                v.CopyFrom(o);
            else
                value[i] = other[i] is { } o2 ? o2.Clone() : default!;
        }

        if (value.Count > other.Count)
        {
            value.RemoveRange(other.Count, value.Count - other.Count);
        }
        else if (value.Count < other.Count)
        {
            value.EnsureCapacity(other.Count);

            for (int i = value.Count; i < other.Count; i++)
                value.Add(other[i] is { } o ? o.Clone() : default!);
        }
    }
}
