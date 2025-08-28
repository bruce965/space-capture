// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: AGPL-3.0-only

using SpaceCapture.Shared.Types;

namespace SpaceCapture.Shared.Logic.Rules;

partial class RulesSet
{
    /// <summary>
    /// Set of rules for standard games.
    /// </summary>
    public static RulesSet Standard { get; } =
        new()
        {
            Resources =
            [
                new(ResourceType.Population) { DamageAbsorbtion = (FP48D16)1 / 10, IsActive = true },
                new(ResourceType.Food) { DamageAbsorbtion = (FP48D16)1 / 1000 },
                new(ResourceType.Metal) { DamageAbsorbtion = (FP48D16)1 / 1000 },
                new(ResourceType.Gas) { DamageAbsorbtion = (FP48D16)1 / 1000 },
                new(ResourceType.Ammunitions) { DamageAbsorbtion = (FP48D16)1 / 100 },
                new(ResourceType.Bombs) { DamageAbsorbtion = (FP48D16)1 / 100 },
            ],
            Structures =
            [
                #region Special
                new(StructureType.House)
                {
                    MaxHealth = 2,
                    DamageAbsorbtion = (FP48D16)1 / 20,
                    BuildTicks = GameConstants.TicksPerSecond * 3,
                    BuildCost = [new(ResourceType.Metal) { Count = 10 }],
                    RepairCost = [new(ResourceType.Metal) { Count = 5 }],
                    ActiveCost = [new(ResourceType.Food) { Count = 1 * GameConstants.SecondsPerTick }],
                    Produces = [new(ResourceType.Population) { Count = GameConstants.SecondsPerTick / 10 }],
                    Stores = [new(ResourceType.Population) { Count = 10 }],
                },
                #endregion

                #region Factories
                new(StructureType.Farm)
                {
                    MaxHealth = 2,
                    DamageAbsorbtion = (FP48D16)1 / 30,
                    BuildTicks = GameConstants.TicksPerSecond * 5,
                    BuildCost = [new(ResourceType.Food) { Count = 2 }, new(ResourceType.Metal) { Count = 10 }],
                    RepairCost = [new(ResourceType.Food) { Count = 1 }, new(ResourceType.Metal) { Count = 5 }],
                    ActivationCost = [new(ResourceType.Population) { Count = 1 }],
                    Produces = [new(ResourceType.Food) { Count = 1 * GameConstants.SecondsPerTick }],
                    Stores = [new(ResourceType.Food) { Count = 10 }],
                },
                new(StructureType.MetalMine)
                {
                    MaxHealth = 2,
                    DamageAbsorbtion = (FP48D16)1 / 20,
                    BuildTicks = GameConstants.TicksPerSecond * 5,
                    BuildCost = [new(ResourceType.Food) { Count = 10 }],
                    RepairCost = [new(ResourceType.Food) { Count = 5 }],
                    ActivationCost = [new(ResourceType.Population) { Count = 10 }],
                    ActiveCost = [new(ResourceType.Food) { Count = 1 * GameConstants.SecondsPerTick }],
                    Produces = [new(ResourceType.Metal) { Count = 1 * GameConstants.SecondsPerTick }],
                    Stores = [new(ResourceType.Metal) { Count = 10 }],
                },
                new(StructureType.GasMine)
                {
                    MaxHealth = 2,
                    DamageAbsorbtion = (FP48D16)1 / 20,
                    BuildTicks = GameConstants.TicksPerSecond * 5,
                    BuildCost = [new(ResourceType.Food) { Count = 6 }, new(ResourceType.Metal) { Count = 10 }],
                    RepairCost = [new(ResourceType.Food) { Count = 3 }, new(ResourceType.Metal) { Count = 5 }],
                    ActivationCost = [new(ResourceType.Population) { Count = 10 }],
                    ActiveCost = [new(ResourceType.Food) { Count = 1 * GameConstants.SecondsPerTick }],
                    Produces = [new(ResourceType.Gas) { Count = 1 * GameConstants.SecondsPerTick }],
                    Stores = [new(ResourceType.Gas) { Count = 10 }],
                },
                #endregion

                #region Planetary Upgrades
                new(StructureType.WallsI)
                {
                    MaxHealth = 100,
                    DamageAbsorbtion = (FP48D16)7 / 10,
                    BuildCost = [new(ResourceType.Metal) { Count = 100 }],
                    RepairCost = [new(ResourceType.Metal) { Count = 100 }],
                },
                new(StructureType.WallsII)
                {
                    MaxHealth = 200,
                    DamageAbsorbtion = (FP48D16)8 / 10,
                    UpgradeOf = StructureType.WallsI,
                    BuildCost = [new(ResourceType.Metal) { Count = 300 }],
                    RepairCost = [new(ResourceType.Metal) { Count = 200 }],
                },
                new(StructureType.WallsIII)
                {
                    MaxHealth = 300,
                    DamageAbsorbtion = (FP48D16)9 / 10,
                    UpgradeOf = StructureType.WallsII,
                    BuildCost = [new(ResourceType.Metal) { Count = 1000 }],
                    RepairCost = [new(ResourceType.Metal) { Count = 300 }],
                },
                new(StructureType.AntiAirI)
                {
                    MaxHealth = 10,
                    DamageAbsorbtion = (FP48D16)1 / 20,
                    BuildCost = [new(ResourceType.Metal) { Count = 20 }, new(ResourceType.Bombs) { Count = 10 }],
                    RepairCost = [new(ResourceType.Metal) { Count = 20 }],
                    ActivationCost = [new(ResourceType.Population) { Count = 10 }],
                    // TODO: shooting.
                },
                new(StructureType.AntiAirII)
                {
                    MaxHealth = 10,
                    DamageAbsorbtion = (FP48D16)1 / 20,
                    UpgradeOf = StructureType.AntiAirI,
                    BuildCost = [new(ResourceType.Metal) { Count = 50 }, new(ResourceType.Bombs) { Count = 30 }],
                    RepairCost = [new(ResourceType.Metal) { Count = 50 }],
                    ActivationCost = [new(ResourceType.Population) { Count = 20 }],
                    // TODO: shooting.
                },
                new(StructureType.AntiAirIII)
                {
                    MaxHealth = 10,
                    DamageAbsorbtion = (FP48D16)1 / 20,
                    UpgradeOf = StructureType.AntiAirII,
                    BuildCost = [new(ResourceType.Metal) { Count = 150 }, new(ResourceType.Bombs) { Count = 100 }],
                    RepairCost = [new(ResourceType.Metal) { Count = 150 }],
                    ActivationCost = [new(ResourceType.Population) { Count = 30 }],
                    // TODO: shooting.
                },
                #endregion
            ],
        };
}
