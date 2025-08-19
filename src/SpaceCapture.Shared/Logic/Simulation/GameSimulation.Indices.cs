// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: AGPL-3.0-only

namespace SpaceCapture.Shared.Logic.Simulation;

partial class GameSimulation
{
    readonly struct StructureTypeIndex
    {
        int I { get; init; }

        public static implicit operator int(StructureTypeIndex i) => i.I;

        public static explicit operator StructureTypeIndex(int i) => new() { I = i };
    }

    readonly struct ResourceTypeIndex
    {
        int I { get; init; }

        public static implicit operator int(ResourceTypeIndex i) => i.I;

        public static explicit operator ResourceTypeIndex(int i) => new() { I = i };
    }

    readonly struct CelestialBodyIndex
    {
        int I { get; init; }

        public static implicit operator int(CelestialBodyIndex i) => i.I;

        public static explicit operator CelestialBodyIndex(int i) => new() { I = i };
    }

    readonly struct PlayerIndex
    {
        int I { get; init; }

        public static implicit operator int(PlayerIndex i) => i.I;

        public static explicit operator PlayerIndex(int i) => new() { I = i };
    }
}
