// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: AGPL-3.0-only

using System.Text.Json.Serialization;

namespace SpaceCapture.Shared;

/// <summary>
/// Type of resource.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<ResourceType>))]
public enum ResourceType
{
    [JsonStringEnumMemberName("population")]
    Population,

    [JsonStringEnumMemberName("food")]
    Food,

    [JsonStringEnumMemberName("metal")]
    Metal,

    [JsonStringEnumMemberName("gas")]
    Gas,

    [JsonStringEnumMemberName("ammo")]
    Ammunitions,

    [JsonStringEnumMemberName("bombs")]
    Bombs,
}
