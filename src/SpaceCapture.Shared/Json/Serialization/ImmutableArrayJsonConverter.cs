// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: MIT

using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SpaceCapture.Shared.Json.Serialization;

public class ImmutableArrayJsonConverter<T> : JsonConverter<ImmutableArray<T>>
{
    public override ImmutableArray<T> Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    ) => JsonSerializer.Deserialize<T[]>(ref reader, options)?.ToImmutableArray() ?? [];

    public override void Write(
        Utf8JsonWriter writer,
        ImmutableArray<T> value,
        JsonSerializerOptions options
    ) => JsonSerializer.Serialize(writer, value.AsEnumerable(), options);
}
