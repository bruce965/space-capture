// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: AGPL-3.0-only

using Microsoft.AspNetCore.Mvc;
using SpaceCapture.Shared;
using SpaceCapture.Shared.Logic.Rules;
using SpaceCapture.Shared.Logic.Simulation;
using SpaceCapture.Shared.Logic.Stage;
using SpaceCapture.Shared.Utilities;

namespace SpaceCapture.Server.Endpoints;

public static class GameEndpoints
{
    public static IResult NewGame(
        [FromQuery] DeterministicRandom? seed = null,
        [FromQuery] int ticks = 0
    )
    {
        DeterministicRandom gameSeed = seed ?? new DeterministicRandom();

        DeterministicRandom rand = gameSeed.Sanitize();

        GameStage stage = new()
        {
            Seed = gameSeed,
            PlayersCount = 2,
            Rules = RulesSet.Standard,
            CelestialBodies =
            [
                new(CelestialBodyType.Terra, RandomNameGenerator.Star(ref rand), (0, 0)),
                new(CelestialBodyType.Terra, RandomNameGenerator.Planet(ref rand), (-200, -50))
                {
                    Resources =
                    [
                        new(ResourceType.Population) { HardLimit = 1000 },
                        new(ResourceType.Food) { SoftLimit = 10, HardLimit = 12 },
                        new(ResourceType.Metal) { SoftLimit = 10, HardLimit = 12 },
                        new(ResourceType.Gas) { SoftLimit = 10, HardLimit = 12 },
                    ],
                    MaxUpgradeLevel = 3,
                },
                new(CelestialBodyType.Terra, RandomNameGenerator.Planet(ref rand), (200, 50))
                {
                    Resources =
                    [
                        new(ResourceType.Population) { HardLimit = 1000 },
                        new(ResourceType.Food) { SoftLimit = 10, HardLimit = 12 },
                        new(ResourceType.Metal) { SoftLimit = 10, HardLimit = 12 },
                        new(ResourceType.Gas) { SoftLimit = 10, HardLimit = 12 },
                    ],
                    MaxUpgradeLevel = 3,
                },
            ],
        };

        GameSimulation simulation = new(stage)
        {
            //CelestialBodies =
            //[
            //    new(),
            //    new()
            //    {
            //        Player = 0,
            //        Resources = [new(ResourceType.Population, 10), new(ResourceType.Food, 10)],
            //    },
            //    new()
            //    {
            //        Player = 1,
            //        Resources = [new(ResourceType.Population, 10), new(ResourceType.Food, 10)],
            //    },
            //],
        };

        for (int i = 0; i < ticks; i++)
            simulation.TickClock();

        //string json = JsonSerializer.Serialize(simulation.State), AppJsonSerializerContext.Default.GameState);
        //simulation.State = JsonSerializer.Deserialize(json, AppJsonSerializerContext.Default.GameState)!;

        return Results.Ok(stage);
    }
}
