using System.ComponentModel;
using System.Text.Json.Serialization;
using SpaceCapture.Shared.Abstractions;
using SpaceCapture.Shared.Types;
using SpaceCapture.Shared.Utilities;

namespace SpaceCapture.Shared.Logic.State;

partial class GameState
{
    /// <summary>
    /// Data about a ship in a game.
    /// </summary>
    public struct ShipData : ICloneable<ShipData>, ITransferable<ShipData>
    {
        List<ResourceCount> _resources;

        /// <summary>
        /// Ship type.
        /// </summary>
        public ShipType Type { get; set; }

        /// <summary>
        /// Health of this ship.
        /// </summary>
        public FP32D10 Health { get; set; }

        /// <summary>
        /// Resources on this ship.
        /// </summary>
        public List<ResourceCount> Resources
        {
            get => _resources ??= [];
            set => _resources = value;
        }

        public ShipData Clone() => this with { Resources = Resources.DeepClone() };

        public void CopyFrom(ShipData other)
        {
            Type = other.Type;
            Health = other.Health;
            TransferHelper.CopyImmutable(ref _resources, other._resources);
        }
    }

    /// <summary>
    /// Fleet of ships in a game.
    /// </summary>
    public struct Fleet : ICloneable<Fleet>, ITransferable<Fleet>
    {
        List<ShipData>? _ships;

        /// <summary>
        /// Index of the player that controls this fleet.
        /// </summary>
        [JsonPropertyName("player")]
        public int Player { get; set; }

        /// <summary>
        /// Current position of this fleet.
        /// </summary>
        [JsonPropertyName("position")]
        [JsonConverter(typeof(Vector2<FP32D10>.JsonConverter))]
        public Vector2<FP32D10> CurrentPosition { get; set; }

        /// <summary>
        /// Target position of this fleet.
        /// </summary>
        [JsonPropertyName("target")]
        [JsonConverter(typeof(Vector2<FP32D10>.JsonConverter))]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Vector2<FP32D10>? TargetPosition { get; set; }

        /// <summary>
        /// Ships in this fleet.
        /// </summary>
        [JsonPropertyName("ships")]
        public List<ShipData> Ships
        {
            get => _ships ??= [];
            set => _ships = value;
        }

        public Fleet Clone() => this with { Ships = Ships.DeepClone() };

        public void CopyFrom(Fleet other)
        {
            Player = other.Player;
            CurrentPosition = other.CurrentPosition;
            TargetPosition = other.TargetPosition;
            TransferHelper.Copy(ref _ships, other._ships);
        }
    }

    List<Fleet>? _fleets;

    /// <summary>
    /// Data about fleets in a game.
    /// </summary>
    [JsonIgnore]
    public List<Fleet> Fleets
    {
        get => _fleets ??= [];
        set => _fleets = value;
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    [JsonPropertyName("fleets")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public List<Fleet>? FleetsJson
    {
        get => _fleets is { Count: > 0 } s ? s : null;
        set => _fleets = value;
    }
}
