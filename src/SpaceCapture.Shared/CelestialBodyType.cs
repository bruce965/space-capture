// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: AGPL-3.0-only

using System.Text.Json.Serialization;

namespace SpaceCapture.Shared;

[JsonConverter(typeof(JsonStringEnumConverter<CelestialBodyType>))]
public enum CelestialBodyType
{
    [JsonStringEnumMemberName("yellow-dwarf")]
    YellowDwarf,

    [JsonStringEnumMemberName("terra")]
    Terra,
}
