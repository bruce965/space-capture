// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: MIT

using SpaceCapture.Shared.Abstractions;

namespace SpaceCapture.Shared.Utilities;

public static class CloneableCollectionExtensions
{
    /// <inheritdoc cref="ICloneable{T}.Clone"/>
    public static T[] DeepClone<T>(this T[] value)
        where T : ICloneable<T>
    {
        if (typeof(IImmutable).IsAssignableFrom(typeof(T)))
            return [.. value];

        T[] clone = new T[value.Length];
        for (int i = 0; i < clone.Length; i++)
            clone[i] = value[i] is { } v ? v.Clone() : default!;

        return clone;
    }

    /// <inheritdoc cref="ICloneable{T}.Clone"/>
    public static List<T> DeepClone<T>(this List<T> value)
        where T : ICloneable<T>
    {
        if (typeof(IImmutable).IsAssignableFrom(typeof(T)))
            return [.. value];

        List<T> clone = new(value.Count);
        foreach (T el in value)
            clone.Add(el is { } e ? e.Clone() : default!);

        return clone;
    }
}
