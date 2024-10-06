using System;
using System.Numerics;
using Godot;

namespace SpaceCapture;

public static class Utils
{
    /// <summary>
    /// Time-independent damping with exponential-decay.
    /// </summary>
    /// <param name="current">Current value.</param>
    /// <param name="target">Target value.</param>
    /// <param name="smoothing">The proportion of <paramref name="current"/> remaining after one second, the rest is <paramref name="target"/>.</param>
    /// <param name="deltaTime">Seconds passed since last invocation.</param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T Damp<T>(T current, T target, float smoothing, double deltaTime)
        where T : IAdditionOperators<T, T, T>, ISubtractionOperators<T, T, T>, IMultiplyOperators<T, float, T>
        => Lerp(current, target, (float)(1d - Math.Pow(smoothing, deltaTime)));

    /// <inheritdoc cref="Damp{T}(T, T, float, double)" />
    public static Godot.Vector2 Damp(Godot.Vector2 current, Godot.Vector2 target, float smoothing, double deltaTime)
        => current.Lerp(target, (float)(1d - Math.Pow(smoothing, deltaTime)));

    /// <inheritdoc cref="Mathf.Lerp(double, double, double)"/>
    /// <typeparam name="T"></typeparam>
    public static T Lerp<T>(T from, T to, float weight)
        where T : IAdditionOperators<T, T, T>, ISubtractionOperators<T, T, T>, IMultiplyOperators<T, float, T>
        => from + (to - from) * weight;
}
