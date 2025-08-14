// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: AGPL-3.0-only

using SpaceCapture.Shared.Abstractions;
using SpaceCapture.Shared.Utilities;

namespace SpaceCapture.Shared.Logic.State;

partial class GameState : ICloneable<GameState>, ITransferable<GameState>
{
    public GameState Clone() =>
        new(Configuration)
        {
            Tick = Tick,
            Players = Players.DeepClone(),
            CelestialBodies = CelestialBodies.DeepClone(),
            Fleets = Fleets.DeepClone(),
        };

    public void CopyFrom(GameState other)
    {
        Configuration = other.Configuration;
        Tick = other.Tick;
        Players.CopyFrom(other.Players);
        CelestialBodies.CopyFrom(other.CelestialBodies);
        Fleets.CopyFrom(other.Fleets);
    }
}
