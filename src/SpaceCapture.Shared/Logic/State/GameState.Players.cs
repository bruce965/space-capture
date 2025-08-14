// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: AGPL-3.0-only

using System.Text.Json.Serialization;
using SpaceCapture.Shared.Abstractions;

namespace SpaceCapture.Shared.Logic.State;

partial class GameState
{
    /// <summary>
    /// Data about a player in a game.
    /// </summary>
    public struct PlayerData : ICloneable<PlayerData>, ITransferable<PlayerData>
    {
        /// <inheritdoc/>
        public readonly PlayerData Clone() => new();

        public void CopyFrom(PlayerData other) { }
    }

    /// <summary>
    /// Data about the players in a game.
    /// </summary>
    [JsonPropertyName("players")]
    public PlayerData[] Players { get; set; } = new PlayerData[configuration.PlayersCount];
}
