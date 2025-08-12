using System.Text.Json.Serialization;

namespace SpaceCapture.Shared.Logic;

partial class GameState
{
    /// <summary>
    /// Data about a player in a game.
    /// </summary>
    public struct PlayerData;

    /// <summary>
    /// Data about the players in a game.
    /// </summary>
    [JsonPropertyName("players")]
    public PlayerData[] Players { get; set; } = new PlayerData[configuration.PlayersCount];
}
