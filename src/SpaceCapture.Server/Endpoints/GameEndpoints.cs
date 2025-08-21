// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: AGPL-3.0-only

using Microsoft.AspNetCore.Mvc;
using SpaceCapture.Shared;
using SpaceCapture.Shared.Logic;
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

        GameStage stage = GameGenerator.FromSeed(gameSeed);

        GameSimulation simulation = new(stage);

        simulation.CelestialBodies.Span[1].Player = simulation.Players.Span[0].Index;
        simulation.CelestialBodies.Span[1].Resources[ResourceType.Population].Count = 10;
        simulation.CelestialBodies.Span[1].Resources[ResourceType.Food].Count = 10;

        simulation.CelestialBodies.Span[2].Player = simulation.Players.Span[1].Index;
        simulation.CelestialBodies.Span[2].Resources[ResourceType.Population].Count = 10;
        simulation.CelestialBodies.Span[2].Resources[ResourceType.Food].Count = 10;

        simulation.Commit();

        for (int i = 0; i < ticks; i++)
            simulation.TickClock();

        //string json = JsonSerializer.Serialize(simulation.State), AppJsonSerializerContext.Default.GameState);
        //simulation.State = JsonSerializer.Deserialize(json, AppJsonSerializerContext.Default.GameState)!;

        return Results.Ok(stage);
    }
}
