// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: AGPL-3.0-only

using System.Collections.Immutable;
using System.Diagnostics;
using SpaceCapture.Shared.Abstractions;
using SpaceCapture.Shared.Logic.Simulation;
using SpaceCapture.Shared.Types;

namespace SpaceCapture.Shared.Logic.Rules;

/// <summary>
/// Describes the behaviour of a type of structure.
/// </summary>
/// <param name="type"></param>
[DebuggerDisplay($"{{{nameof(Type)},nq}}")]
public readonly struct StructureRule(StructureType type) : IImmutable
{
    /// <summary>
    /// Type of structure.
    /// </summary>
    public StructureType Type => type;

    /// <summary>
    /// Full health level of this structure.
    /// </summary>
    public FP32D16 MaxHealth { get; init; }

    /// <summary>
    /// Damage absorbtion coefficient (<c>0</c> to absorbe no damage; <c>1</c> to absorbe all damage).
    /// </summary>
    public FP32D16 DamageAbsorbtion { get; init; }

    #region Build

    /// <summary>
    /// Structures required to be built before this structure can replace it.
    /// </summary>
    public StructureType? UpgradeOf { get; init; }

    /// <summary>
    /// How many ticks are required to build this structure.
    /// </summary>
    public long BuildTicks { get; init; } = 1;

    /// <summary>
    /// Resources required to build this structure, or <see langword="null"/> to forbid building.
    /// </summary>
    public ImmutableArray<ResourceCount>? BuildCost { get; init; }

    /// <summary>
    /// Cost to repair this structure or <see langword="null"/> if this structure is not repairable,
    /// scales linearly with the level of damage.
    /// </summary>
    public ImmutableArray<ResourceCount>? RepairCost { get; init; }

    /// <summary>
    /// Resources locked inside this structure when active. Not consumed, but
    /// cannot be used for anything else until this structure is active.
    /// </summary>
    public ImmutableArray<ResourceCount> CommitCost { get; init; } = [];

    /// <summary>
    /// Resources required to keep this structure active for one tick.
    /// </summary>
    public ImmutableArray<ResourceCount> ActiveCost { get; init; } = [];

    #endregion

    #region Effects

    /// <summary>
    /// Each tick, this structure produces/extracts this type of resource when active.
    /// </summary>
    public ImmutableArray<ResourceCount> Produces { get; init; } = [];

    /// <summary>
    /// This structure houses/stores these resources when active, thus raising the limit.
    /// </summary>
    public ImmutableArray<ResourceCount> Stores { get; init; } = [];

    // TODO: shooting (damage, rate, range, etc...) like ships.

    #endregion
}
