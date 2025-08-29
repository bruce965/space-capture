// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: AGPL-3.0-only

using System.Collections.Immutable;
using SpaceCapture.Shared.Abstractions;
using SpaceCapture.Shared.Logic.Simulation;
using SpaceCapture.Shared.Types;

namespace SpaceCapture.Shared.Logic.Rules;

/// <summary>
/// Describes the behaviour of a type of ship.
/// </summary>
/// <param name="type"></param>
public readonly struct ShipRule(ShipType type) : IImmutable
{
    /// <summary>
    /// Type of ship.
    /// </summary>
    public ShipType Type => type;

    /// <summary>
    /// Full health level of this ship.
    /// </summary>
    public FP48D16 MaxHealth { get; init; }

    /// <summary>
    /// Max speed of this ship.
    /// </summary>
    public FP48D16 MaxSpeed { get; init; }

    /// <summary>
    /// Acceleration of this ship.
    /// </summary>
    public FP48D16 Acceleration { get; init; }

    /// <summary>
    /// Max cargo space for regular resources.
    /// </summary>
    public FP48D16 MaxCargo { get; init; }

    /// <summary>
    /// Max cargo space for special resources (e.g.: fuel, ammo).
    /// </summary>
    public ImmutableArray<ResourceCount> MaxSpecialCargo { get; init; }

    #region Build

    /// <summary>
    /// Ship required to be built before this ship can replace it.
    /// </summary>
    public ShipType? UpgradeOf { get; init; }

    /// <summary>
    /// How many ticks are required to build this ship.
    /// </summary>
    public long BuildTicks { get; init; } = 1;

    /// <summary>
    /// Resources required to build this ship, or <see langword="null"/> to forbid building.
    /// </summary>
    public ImmutableArray<ResourceCount>? BuildCost { get; init; }

    /// <summary>
    /// Cost to repair this ship or <see langword="null"/> if this ship is not repairable,
    /// scales linearly with the level of damage.
    /// </summary>
    public ImmutableArray<ResourceCount>? RepairCost { get; init; }

    /// <summary>
    /// Resources locked inside this ship when active. Not consumed, but
    /// cannot be used for anything else until this ship is active.
    /// Refunded when the ship is deactivated.
    /// </summary>
    public ImmutableArray<ResourceCount> ActivationCost { get; init; } = [];

    /// <summary>
    /// Resources required to keep this ship active for one tick when not docked.
    /// </summary>
    public ImmutableArray<ResourceCount> LifeSupportCost { get; init; } = [];

    /// <summary>
    /// Resources required to reach max speed with this ship and then decelerate
    /// back to a halt.
    /// </summary>
    public ImmutableArray<ResourceCount> ThrustCost { get; init; } = [];

    #endregion

    #region Weapons

    /// <summary>
    /// Time required to reload between shots.
    /// </summary>
    public FP48D16 ReloadTime { get; init; }

    /// <summary>
    /// Range of fire.
    /// </summary>
    public FP48D16 WeaponRange { get; init; }

    /// <summary>
    /// Damage with one shot to other active ships or <see langword="null"/> if
    /// this ship cannot attack other ships.
    /// </summary>
    public FP48D16? DamageToAir { get; init; }

    /// <summary>
    /// Damage with one shot to ground structures, resources and docked ships or
    /// <see langword="null"/> if this ship cannot attack the ground.
    /// </summary>
    public FP48D16? DamageToGround { get; init; }

    /// <summary>
    /// Resources required to fire one shot with this ship.
    /// </summary>
    public ImmutableArray<ResourceCount> WeaponCost { get; init; } = [];

    #endregion
}
