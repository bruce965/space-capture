// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: MIT

using System.Runtime.CompilerServices;

namespace SpaceCapture.Shared.Utilities;

public static class EnumerableExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TResult[] ToArray<TSource, TResult>(
        this IReadOnlyList<TSource> source,
        Func<TSource, TResult> converter
    )
    {
        TResult[] result = new TResult[source.Count];
        for (int i = 0; i < result.Length && i < source.Count; i++)
            result[i] = converter(source[i]);

        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TResult[] ToArray<TSource, TResult>(
        this ReadOnlySpan<TSource> source,
        Func<TSource, TResult> converter
    )
    {
        TResult[] result = new TResult[source.Length];
        for (int i = 0; i < result.Length && i < source.Length; i++)
            result[i] = converter(source[i]);

        return result;
    }
}
