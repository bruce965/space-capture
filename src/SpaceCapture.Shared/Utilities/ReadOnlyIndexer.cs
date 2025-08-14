// SPDX-FileCopyrightText: Copyright 2021 Fabio Iotti
// SPDX-License-Identifier: MIT

// https://github.com/bruce965/util/blob/master/CSharp/Language/Indexer.cs

namespace SpaceCapture.Shared.Utilities;

/// <summary>
/// Indexer to be used as a property.
///
/// <code>
/// record Fruit(string Name, float Weight);
///
/// class Basket
/// {
///     public List&lt;Fruit&gt; Fruits { get; } = new();
///
///     public Indexer&lt;Basket, int, string&gt; FruitNames =&gt; new(this, (s, i) =&gt; s.Fruits[i].Name, (s, i, v) =&gt; s.Fruits[i].Name = v);
///     public ReadOnlyIndexer&lt;Basket, int, float&gt; FruitWeights =&gt; new(this, (s, i) =&gt; s.Fruits[i].Weight);
/// }
///
/// var basket = new Basket();
///
/// basket.Fruits.Add(new Fruit() { Name = "Banana", Weight = 120 });
/// Console.WriteLine(basket.FruitNames[0]);  // Banana
/// Console.WriteLine(basket.FruitWeights[0]);  // 120
///
/// basket.FruitNames[0] = "Kiwi";
/// Console.WriteLine(basket.FruitNames[0]);  // Kiwi
/// Console.WriteLine(basket.FruitWeights[0]);  // 120
/// </code>
/// </summary>
public readonly struct ReadOnlyIndexer<TSource, TKey, TValue>(
    TSource source,
    Func<TSource, TKey, TValue> getter
)
{
    readonly TSource _source = source;
    readonly Func<TSource, TKey, TValue> _getter = getter;

    public TValue this[TKey key] => _getter(_source, key);
}
