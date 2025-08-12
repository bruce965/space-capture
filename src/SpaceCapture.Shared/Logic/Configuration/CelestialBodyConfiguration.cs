using System.Collections.Immutable;
using System.Text.Json.Serialization;
using SpaceCapture.Shared.Json.Serialization;
using SpaceCapture.Shared.Types;

namespace SpaceCapture.Shared.Logic.Configuration;

/// <summary>
/// Celestial body configuration.
/// </summary>
public readonly struct CelestialBodyConfiguration(string name, Vector2<FP32D10> location)
{
    /// <summary>
    /// Name of this celestial body.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name => name;

    /// <summary>
    /// Location of this celestial body in global space.
    /// </summary>
    [JsonPropertyName("location")]
    [JsonConverter(typeof(Vector2<FP32D10>.JsonConverter))]
    public Vector2<FP32D10> Location => location;

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

    /// <summary>
    /// Max allowed number of people waiting on this celestial body without a ship.
    /// </summary>
    [JsonPropertyName("cap")]
    public int PopulationCap { get; init; }
}
