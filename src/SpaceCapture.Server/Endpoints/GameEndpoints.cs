// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: AGPL-3.0-only

using Microsoft.AspNetCore.Mvc;
using SpaceCapture.Shared;
using SpaceCapture.Shared.Logic.Configuration;
using SpaceCapture.Shared.Logic.Rules;
using SpaceCapture.Shared.Logic.State;
using SpaceCapture.Shared.Utilities;

namespace SpaceCapture.Server.Endpoints;

public static class GameEndpoints
{
    public static IResult NewGame([FromQuery] DeterministicRandom.Seed? seed = null)
    {
        DeterministicRandom.Seed gameSeed = seed ?? DeterministicRandom.NewSeed();

        DeterministicRandom.Seed rand = gameSeed.Sanitize();

        GameConfiguration configuration = new()
        {
            Seed = gameSeed,
            Rules = RulesSet.Standard,
            PlayersCount = 2,
            CelestialBodies =
            [
                new(RandomNameGenerator.Star(ref rand), (0, 0)),
                new(RandomNameGenerator.Planet(ref rand), (-200, -50))
                {
                    MaxUpgradeLevel = 3,
                    PopulationCap = 1000,
                    Resources =
                    [
                        new(ResourceType.Food, 1, 10),
                        new(ResourceType.Metal, 1, 10),
                        new(ResourceType.Gas, 1, 10),
                    ],
                },
                new(RandomNameGenerator.Planet(ref rand), (200, 50))
                {
                    MaxUpgradeLevel = 3,
                    PopulationCap = 1000,
                    Resources =
                    [
                        new(ResourceType.Food, 1, 10),
                        new(ResourceType.Metal, 1, 10),
                        new(ResourceType.Gas, 1, 10),
                    ],
                },
            ],
        };

        GameState game = new(configuration)
        {
            CelestialBodies =
            [
                new(),
                new()
                {
                    Player = 0,
                    Resources = [new(ResourceType.Population, 10), new(ResourceType.Food, 10)],
                },
                new()
                {
                    Player = 1,
                    Resources = [new(ResourceType.Population, 10), new(ResourceType.Food, 10)],
                },
            ],
        };

        // string json = JsonSerializer.Serialize(game, AppJsonSerializerContext.Default.GameState);
        // GameState state = JsonSerializer.Deserialize(json, AppJsonSerializerContext.Default.GameState)!;

        return Results.Ok(game);
    }
}
