using System.Diagnostics.CodeAnalysis;
using SpaceCapture.Shared.Abstractions;

namespace SpaceCapture.Shared.Utilities;

public static class TransferHelper
{
    public static void Copy<T>([NotNullIfNotNull(nameof(from))] ref T[]? to, T[]? from)
        where T : ICloneable<T>, ITransferable<T>
    {
        if (from is null)
        {
            to = null;
        }
        else if (to?.Length == from.Length)
        {
            to.CopyFrom(from);
        }
        else
        {
            to = new T[from.Length];
            for (int i = 0; i < to.Length; i++)
                to[i] = from[i].Clone();
        }
    }

    public static void Copy<T>([NotNullIfNotNull(nameof(from))] ref List<T>? to, List<T>? from)
        where T : ICloneable<T>, ITransferable<T>
    {
        if (from is null)
        {
            to = null;
        }
        else
        {
            to ??= new(from.Count);
            to.CopyFrom(from);
        }
    }

    public static void Copy<TKey, TValue>(
        [NotNullIfNotNull(nameof(from))] ref Dictionary<TKey, TValue>? to,
        Dictionary<TKey, TValue>? from
    )
        where TKey : notnull
        where TValue : ICloneable<TValue>, ITransferable<TValue>
    {
        if (from is null)
        {
            to = null;
        }
        else
        {
            to ??= new(from.Count);
            to.Clear();

            foreach ((TKey k, TValue v) in from)
                to[k] = v.Clone();
        }
    }

    public static void CopyImmutable<T>([NotNullIfNotNull(nameof(from))] ref T[]? to, T[]? from)
        where T : IImmutable
    {
        if (from is null)
            to = null;
        else if (to?.Length == from.Length)
            from.AsSpan().CopyTo(to);
        else
            to = [.. from];
    }

    public static void CopyImmutable<T>(
        [NotNullIfNotNull(nameof(from))] ref List<T>? to,
        List<T>? from
    )
        where T : IImmutable
    {
        if (from is null)
        {
            to = null;
        }
        else
        {
            to ??= new(from.Count);
            to.CopyFrom(from);
        }
    }

    public static void CopyImmutable<TKey, TValue>(
        [NotNullIfNotNull(nameof(from))] ref Dictionary<TKey, TValue>? to,
        Dictionary<TKey, TValue>? from
    )
        where TKey : notnull
        where TValue : IImmutable
    {
        if (from is null)
        {
            to = null;
        }
        else
        {
            to ??= new(from.Count);
            to.Clear();

            foreach ((TKey k, TValue v) in from)
                to[k] = v;
        }
    }
}
