// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: AGPL-3.0-only

using SpaceCapture.Shared.Logic.Rules;
using SpaceCapture.Shared.Logic.Stage;
using SpaceCapture.Shared.Utilities;

namespace SpaceCapture.Shared.Logic;

public static class GameGenerator
{
    public static GameStage FromSeed(DeterministicRandom seed)
    {
        DeterministicRandom rand = seed.Sanitize();

        GameStage stage = new()
        {
            Seed = seed,
            PlayersCount = 2,
            Rules = RulesSet.Standard,
            CelestialBodies =
            [
                new((0, 0))
                {
                    Class = CelestialBodyClass.YellowDwarf,
                    //Name = RandomNameGenerator.Star(ref rand),
                    //Size = 200,
                },
                new((-200, -50))
                {
                    Resources =
                    [
                        new(ResourceType.Population) { HardLimit = 1000 },
                        new(ResourceType.Food) { SoftLimit = 10, HardLimit = 12 },
                        new(ResourceType.Metal) { SoftLimit = 10, HardLimit = 12 },
                        new(ResourceType.Gas) { SoftLimit = 10, HardLimit = 12 },
                    ],
                    MaxUpgradeLevel = 3,
                    Class = CelestialBodyClass.Terra,
                    //Name = RandomNameGenerator.Planet(ref rand),
                    //Size = 100,
                },
                new((200, 50))
                {
                    Resources =
                    [
                        new(ResourceType.Population) { HardLimit = 1000 },
                        new(ResourceType.Food) { SoftLimit = 10, HardLimit = 12 },
                        new(ResourceType.Metal) { SoftLimit = 10, HardLimit = 12 },
                        new(ResourceType.Gas) { SoftLimit = 10, HardLimit = 12 },
                    ],
                    MaxUpgradeLevel = 3,
                    Class = CelestialBodyClass.GasGiant,
                    //Name = RandomNameGenerator.Planet(ref rand),
                    //Size = 100,
                },
            ],
        };

        return stage;
    }
}
