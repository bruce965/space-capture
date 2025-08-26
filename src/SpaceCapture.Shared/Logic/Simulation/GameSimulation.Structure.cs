// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: AGPL-3.0-only

using System.Diagnostics;
using SpaceCapture.Shared.Abstractions;
using SpaceCapture.Shared.Logic.Rules;
using SpaceCapture.Shared.Types;

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

        /// <summary>
        /// How much cumulative unrepaired damage has been dealt to this type of structures.
        /// </summary>
        /// <remarks>
        /// For each <see cref="StructureRule.MaxHealth" /> points of damage, one structure will be treated as inactive.
        /// </remarks>
        public FP48D16 Damage { get; set; }

        /// <summary>
        /// Repair structures that need repairing.
        /// </summary>
        public bool RepairDamaged { get; set; } = true;

        /// <inheritdoc/>
        public readonly Structure Clone() => this;

        public void CopyFrom(Structure other) => this = other;
    }
}
