// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: MIT

using System.Buffers;
using System.Text.Json;
using System.Text.Json.Serialization;
using SpaceCapture.Shared.Abstractions;
using SpaceCapture.Shared.Utilities;

namespace SpaceCapture.Shared.Types;

public readonly struct Vector2<T>(T x, T y) : IImmutable
{
    public class JsonConverter : JsonConverter<Vector2<T>>
    {
        public override Vector2<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            JsonSerializer.Deserialize<T[]>(ref reader, options) is { Length: >= 1 } v
                ? new(v[0], v[1])
                : throw new JsonException("Invalid Vector2<T>.");

        public override void Write(Utf8JsonWriter writer, Vector2<T> value, JsonSerializerOptions options)
        {
            using BufferLease<T> v = BufferPool.Rent<T>(2);
            v[0] = value.X;
            v[1] = value.Y;
            JsonSerializer.Serialize<ReadOnlyMemory<T>>(writer, v, options);
        }
    }

    public T X => x;

    public T Y => y;

    public void Deconstruct(out T x, out T y) => (x, y) = (X, Y);

    public static implicit operator (T X, T Y)(Vector2<T> vector) => (vector.X, vector.Y);

    public static implicit operator Vector2<T>((T X, T Y) vector) => new(vector.X, vector.Y);
}
