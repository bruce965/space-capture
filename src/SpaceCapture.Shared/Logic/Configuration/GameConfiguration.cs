using System.Collections.Immutable;
using System.Text.Json.Serialization;
using SpaceCapture.Shared.Utilities;

namespace SpaceCapture.Shared.Logic.Configuration;

/// <summary>
/// Configuration of a game.
/// </summary>
public class GameConfiguration
{
    /// <summary>
    /// Seed used to initialize and generate random events this game.
    /// </summary>
    [JsonPropertyName("seed")]
    public required DeterministicRandom.Seed Seed { get; init; }

    /// <summary>
    /// Number of players in this game.
    /// </summary>
    [JsonPropertyName("players")]
    public required int PlayersCount { get; init; }

    /// <summary>
    /// Configuration of the celestial bodies in this game.
    /// </summary>
    [JsonPropertyName("planets")]
    public required ImmutableArray<CelestialBodyConfiguration> CelestialBodies { get; init; }
}
