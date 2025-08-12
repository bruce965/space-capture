using System.Text.Json.Serialization;
using SpaceCapture.Shared.Types;

namespace SpaceCapture.Shared.Logic.Configuration;

/// <summary>
/// Data about a resource from a celestial body.
/// </summary>
/// <param name="type"></param>
/// <param name="productionRate"></param>
/// <param name="maxCollectorsCount"></param>
public readonly struct CelestialBodyResource(
    ResourceType type,
    FP32D10 productionRate,
    int maxCollectorsCount
)
{
    /// <summary>
    /// Type of this resource.
    /// </summary>
    [JsonPropertyName("type")]
    public ResourceType Type => type;

    /// <summary>
    /// How much of this resource can be produced by each collector each tick.
    /// </summary>
    [JsonPropertyName("rate")]
    public FP32D10 ProductionRate => productionRate;

    /// <summary>
    /// Maximum amount of collectors.
    /// </summary>
    [JsonPropertyName("max")]
    public int MaxCollectorsCount => maxCollectorsCount;
}
