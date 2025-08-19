// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: AGPL-3.0-only

using System.ComponentModel;
using System.Text.Json.Serialization;
using SpaceCapture.Shared.Abstractions;
using SpaceCapture.Shared.Types;

namespace SpaceCapture.Shared.Logic.Stage;

/// <summary>
/// Data about a resource from a celestial body.
/// </summary>
/// <param name="type"></param>
public readonly struct CelestialBodyResource(ResourceType type) : IImmutable
{
    /// <summary>
    /// Type of this resource.
    /// </summary>
    [JsonPropertyName("type")]
    public ResourceType Type => type;

    /// <summary>
    /// Production multiplier for this resource on this celestial body.
    /// </summary>
    [JsonIgnore]
    public FP32D16 ProductionMultiplier { get; init; } = 1;

    [EditorBrowsable(EditorBrowsableState.Never)]
    [JsonPropertyName("multiplier")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public FP32D16? ProductionMultiplierJson
    {
        get => ProductionMultiplier == 1 ? null : ProductionMultiplier;
        init => ProductionMultiplier = value ?? 1;
    }

    /// <summary>
    /// Maximum amount of this resource that can be stored on this celestial body.
    /// When this limit is reached, no more of this resource can be produced, but
    /// more can still be imported from other celestial bodies.
    /// </summary>
    [JsonPropertyName("soft-limit")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? SoftLimit { get; init; }

    /// <summary>
    /// Maximum amount of this resource that can be store on this celestial body.
    /// When this limit is exceeded, any excess resource may not be produced or
    /// unloaded onto this planet. Excess resources are immediately lost/destroyed.
    /// </summary>
    [JsonPropertyName("hard-limit")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? HardLimit { get; init; }
}
