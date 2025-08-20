// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: AGPL-3.0-only

using SpaceCapture.Shared.Abstractions;

namespace SpaceCapture.Shared.Logic.Simulation;

partial class GameSimulation<TData>
{
    public readonly struct StructureTypeIndex : IIndex
    {
        internal int Index { get; init; }

        int IIndex.Index => Index;
    }

    public readonly struct ResourceTypeIndex : IIndex
    {
        internal int Index { get; init; }

        int IIndex.Index => Index;
    }

    public readonly struct CelestialBodyIndex : IIndex
    {
        internal int Index { get; init; }

        int IIndex.Index => Index;
    }

    public readonly struct PlayerIndex : IIndex
    {
        internal int Index { get; init; }

        int IIndex.Index => Index;
    }
}
