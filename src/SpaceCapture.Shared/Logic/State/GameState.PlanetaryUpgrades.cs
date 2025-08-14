using System.Text.Json.Serialization;
using SpaceCapture.Shared.Abstractions;
using SpaceCapture.Shared.Types;

namespace SpaceCapture.Shared.Logic.State;

partial class GameState
{
    /// <summary>
    /// Planetary upgrade type, level, and health.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="count"></param>
    public readonly struct PlanetaryUpgrade(PlanetaryUpgradeType type, int level, FP32D10 health)
        : IImmutable
    {
        /// <summary>
        /// Planetary upgrade type.
        /// </summary>
        [JsonPropertyName("type")]
        public PlanetaryUpgradeType Type => type;

        /// <summary>
        /// Upgrade level.
        /// </summary>
        [JsonPropertyName("level")]
        public int Level => level;

        /// <summary>
        /// Health.
        /// </summary>
        [JsonPropertyName("health")]
        public FP32D10 Health => health;
    }
}
