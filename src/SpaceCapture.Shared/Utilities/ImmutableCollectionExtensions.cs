using SpaceCapture.Shared.Abstractions;

namespace SpaceCapture.Shared.Utilities;

public static class ImmutableCollectionExtensions
{
    /// <inheritdoc cref="ICloneable{T}.Clone"/>
    public static T[] DeepClone<T>(this T[] value)
        where T : IImmutable => [.. value];

    /// <inheritdoc cref="ICloneable{T}.Clone"/>
    public static List<T> DeepClone<T>(this List<T> value)
        where T : IImmutable => [.. value];

    /// <inheritdoc cref="ITransferable{T}.CopyFrom(T)"/>
    public static void CopyFrom<T>(this T[] value, T[] other)
        where T : IImmutable
    {
        ArgumentOutOfRangeException.ThrowIfNotEqual(value.Length, other.Length);
        other.AsSpan().CopyTo(value);
    }

    /// <inheritdoc cref="ICloneable{T}.Clone"/>
    public static void CopyFrom<T>(this List<T> value, List<T> other)
        where T : IImmutable
    {
        for (int i = 0; i < value.Count && i < other.Count; i++)
            value[i] = other[i] is { } o ? o : default!;

        if (value.Count > other.Count)
        {
            value.RemoveRange(other.Count, value.Count - other.Count);
        }
        else if (value.Count < other.Count)
        {
            value.EnsureCapacity(other.Count);

            for (int i = value.Count; i < other.Count; i++)
                value.Add(other[i] is { } o ? o : default!);
        }
    }
}
