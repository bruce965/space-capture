// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: AGPL-3.0-only

using System.Text.Json.Serialization;

namespace SpaceCapture.Shared;

/// <summary>
/// Type of planetary upgrade.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<PlanetaryUpgradeType>))]
public enum PlanetaryUpgradeType
{
    [JsonStringEnumMemberName("walls")]
    Walls,

    [JsonStringEnumMemberName("aa")]
    AntiAir,
}
