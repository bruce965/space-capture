using System.Numerics;

namespace SpaceCapture.Shared.Types;

public static class Vector2Extensions
{
    public static Vector2<T> Add<T>(this Vector2<T> left, Vector2<T> right)
        where T : IAdditionOperators<T, T, T> => (left.X + right.X, left.Y + right.Y);

    public static Vector2<T> Subtract<T>(this Vector2<T> left, Vector2<T> right)
        where T : ISubtractionOperators<T, T, T> => (left.X - right.X, left.Y - right.Y);

    public static Vector2<T> Rotate<T>(this Vector2<T> value, T radians)
        where T : ITrigonometricFunctions<T>
    {
        (T sin, T cos) = T.SinCos(radians);
        return new(cos * value.X - sin * value.Y, sin * value.X + cos * value.Y);
    }
}
