// SPDX-FileCopyrightText: Copyright 2021-2025 Fabio Iotti
// SPDX-License-Identifier: MIT

// https://github.com/bruce965/util/blob/master/CSharp/Language/Indexer.cs

namespace SpaceCapture.Shared.Utilities;

/// <summary>
/// Indexer to be used as a property.
/// </summary>
/// <typeparam name="TSource"></typeparam>
/// <typeparam name="TKey"></typeparam>
/// <typeparam name="TValue"></typeparam>
/// <param name="source"></param>
/// <param name="getter"></param>
public readonly struct ReadOnlyIndexer<TSource, TKey, TValue>(TSource source, Func<TSource, TKey, TValue> getter)
{
    readonly TSource _source = source;
    readonly Func<TSource, TKey, TValue> _getter = getter;

    public TValue this[TKey key] => _getter(_source, key);
}
