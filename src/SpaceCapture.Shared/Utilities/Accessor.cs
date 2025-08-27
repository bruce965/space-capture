// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: MIT

using System.Collections.Immutable;
using SpaceCapture.Shared.Abstractions;

namespace SpaceCapture.Shared.Utilities;

public readonly ref struct Accessor<T, TIndex>(Span<T> span)
    where TIndex : IIndex
{
    public Span<T> Span { get; } = span;

    public readonly ref T this[TIndex index] => ref Span[index.Index];

    public Span<T>.Enumerator GetEnumerator() => Span.GetEnumerator();

    public static implicit operator Accessor<T, TIndex>(Span<T> span) => new(span);

    public static implicit operator Accessor<T, TIndex>(T[] array) => new(array);

    public static implicit operator Span<T>(Accessor<T, TIndex> accessor) => accessor.Span;
}

public readonly ref struct ReadOnlyAccessor<T, TIndex>(Span<T> span)
    where TIndex : IIndex
{
    public ReadOnlySpan<T> Span { get; } = span;

    public readonly ref readonly T this[TIndex index] => ref Span[index.Index];

    public ReadOnlySpan<T>.Enumerator GetEnumerator() => Span.GetEnumerator();

    public static implicit operator ReadOnlyAccessor<T, TIndex>(Span<T> span) => new(span);

    public static implicit operator ReadOnlyAccessor<T, TIndex>(T[] array) => new(array);

    public static implicit operator ReadOnlySpan<T>(ReadOnlyAccessor<T, TIndex> accessor) => accessor.Span;
}

public readonly ref struct Accessor<T, TType, TIndex>(Span<T> span, ImmutableDictionary<TType, TIndex> typeToIndex)
    where TType : notnull
    where TIndex : IIndex
{
    public Span<T> Span { get; } = span;

    public readonly ref T this[TIndex index] => ref Span[index.Index];

    public readonly ref T this[TType type] => ref Span[typeToIndex[type].Index];

    public Span<T>.Enumerator GetEnumerator() => Span.GetEnumerator();

    public static implicit operator Span<T>(Accessor<T, TType, TIndex> accessor) => accessor.Span;
}

public readonly ref struct ReadOnlyAccessor<T, TType, TIndex>(
    ReadOnlySpan<T> span,
    ImmutableDictionary<TType, TIndex> typeToIndex
)
    where TType : notnull
    where TIndex : IIndex
{
    public ReadOnlySpan<T> Span { get; } = span;

    public readonly ref readonly T this[TIndex index] => ref Span[index.Index];

    public readonly ref readonly T this[TType type] => ref Span[typeToIndex[type].Index];

    public ReadOnlySpan<T>.Enumerator GetEnumerator() => Span.GetEnumerator();

    public static implicit operator ReadOnlySpan<T>(ReadOnlyAccessor<T, TType, TIndex> accessor) => accessor.Span;
}
