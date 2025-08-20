// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: AGPL-3.0-only

using System.Diagnostics.CodeAnalysis;
using SpaceCapture.Shared.Abstractions;

namespace SpaceCapture.Shared.Logic.Simulation;

partial class GameSimulation<TData>
{
    public struct Player(PlayerIndex index) : ICloneable<Player>, ITransferable<Player>
    {
        public PlayerIndex Index => index;

        /// <summary>
        /// Custom data attached to this player.
        /// </summary>
        [MaybeNull]
        public TData Data { get; set; }

        /// <inheritdoc/>
        public readonly Player Clone() => new() { Data = Data! };

        public void CopyFrom(Player other)
        {
            Data = other.Data!;
        }
    }
}
