using System.Text.Json.Serialization;
using SpaceCapture.Shared.Types;

namespace SpaceCapture.Shared.Logic;

partial class GameState
{
    /// <summary>
    /// Planetary upgrade type, level, and health.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="count"></param>
    public readonly struct PlanetaryUpgrade(PlanetaryUpgradeType type, int level, FP32D10 health)
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
