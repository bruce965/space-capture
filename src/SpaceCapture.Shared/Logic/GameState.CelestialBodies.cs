using System.ComponentModel;
using System.Text.Json.Serialization;
using SpaceCapture.Shared.Types;

namespace SpaceCapture.Shared.Logic;

partial class GameState
{
    /// <summary>
    /// Data about a celestial body in a game.
    /// </summary>
    public struct CelestialBodyData
    {
        List<ResourceCount>? _resources;
        List<StructureCount>? _structures;
        List<PlanetaryUpgrade>? _planetaryUpgrades;

        /// <summary>
        /// Index of the player that currently owns this celestial body.
        /// </summary>
        [JsonPropertyName("player")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int? Player { get; set; }

        /// <summary>
        /// Resources on this celestial body.
        /// </summary>
        [JsonIgnore]
        public List<ResourceCount> Resources
        {
            get => _resources ??= [];
            set => _resources = value;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [JsonPropertyName("resources")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public List<ResourceCount>? ResourcesJson
        {
            get => _resources is { Count: > 0 } r ? r : null;
            set => _resources = value;
        }

        /// <summary>
        /// Structures on this celestial body.
        /// </summary>
        [JsonIgnore]
        public List<StructureCount> Structures
        {
            get => _structures ??= [];
            set => _structures = value;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [JsonPropertyName("structures")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public List<StructureCount>? StructuresJson
        {
            get => _structures is { Count: > 0 } s ? s : null;
            set => _structures = value;
        }

        /// <summary>
        /// Planetary upgrades on this celestial body.
        /// </summary>
        [JsonIgnore]
        public List<PlanetaryUpgrade> PlanetaryUpgrades
        {
            get => _planetaryUpgrades ??= [];
            set => _planetaryUpgrades = value;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [JsonPropertyName("upgrades")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public List<PlanetaryUpgrade>? PlanetaryUpgradesJson
        {
            get => _planetaryUpgrades is { Count: > 0 } u ? u : null;
            set => _planetaryUpgrades = value;
        }
    }

    /// <summary>
    /// Data about celestial bodies in a game.
    /// </summary>
    [JsonPropertyName("planets")]
    public CelestialBodyData[] CelestialBodies { get; set; } =
        new CelestialBodyData[configuration.CelestialBodies.Length];
}
