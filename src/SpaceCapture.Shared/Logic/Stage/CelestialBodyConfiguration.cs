// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: AGPL-3.0-only

using System.Collections.Immutable;
using System.Text.Json.Serialization;
using SpaceCapture.Shared.Abstractions;
using SpaceCapture.Shared.Json.Serialization;
using SpaceCapture.Shared.Types;

namespace SpaceCapture.Shared.Logic.Stage;

/// <summary>
/// Celestial body configuration.
/// </summary>
public readonly struct CelestialBodyConfiguration(CelestialBodyType type, string name, Vector2<FP48D16> location)
    : IImmutable
{
    /// <summary>
    /// Type of this celestial body.
    /// </summary>
    [JsonPropertyName("type")]
    public CelestialBodyType Type => type;

    /// <summary>
    /// Name of this celestial body.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name => name;

    /// <summary>
    /// Location of this celestial body in global space.
    /// </summary>
    [JsonPropertyName("location")]
    [JsonConverter(typeof(Vector2<FP48D16>.JsonConverter))]
    public Vector2<FP48D16> Location => location;

    /// <summary>
    /// Resources provided by this celestial body.
    /// </summary>
    [JsonPropertyName("resources")]
    [JsonConverter(typeof(ImmutableArrayJsonConverter<CelestialBodyResource>))]
    public ImmutableArray<CelestialBodyResource> Resources { get; init; } = [];

    /// <summary>
    /// Maximum level of planetary upgrades allowed on this celestial body.
    /// </summary>
    [JsonPropertyName("level")]
    public int MaxUpgradeLevel { get; init; }
}
