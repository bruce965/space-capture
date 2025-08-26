// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: AGPL-3.0-only

using System.Diagnostics;
using SpaceCapture.Shared.Abstractions;

namespace SpaceCapture.Shared.Logic.Simulation;

partial class GameSimulation<TData>
{
    [DebuggerDisplay($"{{{nameof(TypeData)},nq}}")]
    public struct Structure(StructureRuleCache rule) : ICloneable<Structure>, ITransferable<Structure>
    {
        /// <summary>
        /// Data for this structure type.
        /// </summary>
        public readonly StructureRuleCache TypeData => rule;

        /// <summary>
        /// Number of structures of this type that have been built.
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Number of active structures of this type that have been built.
        /// </summary>
        public int ActiveCount { get; set; }

        /// <inheritdoc/>
        public readonly Structure Clone() => this;

        public void CopyFrom(Structure other) => this = other;
    }
}
