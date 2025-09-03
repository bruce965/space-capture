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
public readonly struct CelestialBodyConfiguration(Vector2<FP48D16> location) : IImmutable
{
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

    #region Visual Properties

    /// <summary>
    /// Visual class of this celestial body.
    /// </summary>
    [JsonPropertyName("class")]
    public CelestialBodyClass? Class { get; init; }

    /// <summary>
    /// Name of this celestial body.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    /// <summary>
    /// Visual size of this celestial body in pixels at zoom level 1.
    /// </summary>
    public FP48D16? Size { get; init; }

    /// <summary>
    /// Rotation of this celestial body in degress.
    /// </summary>
    public FP48D16? Tilt { get; init; }

    /// <summary>
    /// Visual rotation speed of this celestial body in rad/sec.
    /// </summary>
    public FP48D16? RotationSpeed { get; init; }

    /// <summary>
    /// Visual fluidity of the mantle of this celestial body between 0 (still) and 1 (extremely fluid).
    /// </summary>
    public FP48D16? Fluidity { get; init; }

    /// <summary>
    /// Size of clouds on this celestial body between 0 (no clouds) and 1 (covered in clouds completely).
    /// </summary>
    public FP48D16? CloudsSize { get; init; }

    /// <summary>
    /// Density of clouds on this celestial body between 0 (very thin) and 1 (very thick).
    /// </summary>
    public FP48D16? CloudsDensity { get; init; }

    /// <summary>
    /// How often clouds change shape on this celestial body between 0 (static) and 1 (extremely turbulent).
    /// </summary>
    public FP48D16? CloudsTurbulence { get; init; }

    /// <summary>
    /// Wind speed on this celestial body rad/sec.
    /// </summary>
    public FP48D16? WindSpeed { get; init; }

    /// <summary>
    /// Size of the atmosphere halo around this celestial body in pixels at zoom level 1.
    /// </summary>
    public FP48D16? AtmosphereSize { get; init; }

    /// <summary>
    /// Color of the atmosphere halo around this celestial body.
    /// </summary>
    public Color? AtmosphereColor { get; init; }

    #endregion
}
