// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: AGPL-3.0-only

using System.Globalization;
using Godot;
using SpaceCapture.Shared;
using SpaceCapture.Shared.Logic;
using SpaceCapture.Shared.Logic.Simulation;
using SpaceCapture.Shared.Logic.Stage;
using SpaceCapture.Shared.Utilities;

namespace SpaceCapture;

public partial class GameLogic : Node
{
    [Export]
    Node2D _celestialBodiesContainer { get; set; }

    [Export]
    InfoPanel _infoPanel { get; set; }

    public GameSimulation<Node2D> Simulation { get; private set; }

    public override void _Ready()
    {
        DeterministicRandom seed = DeterministicRandom.Parse("test", CultureInfo.InvariantCulture);
        GameStage stage = GameGenerator.FromSeed(seed);
        Simulation = new(stage);

        Simulation.CelestialBodies.Span[1].Player = Simulation.Players.Span[0].Index;
        Simulation.CelestialBodies.Span[1].Resources[ResourceType.Population].Count = 10;
        Simulation.CelestialBodies.Span[1].Resources[ResourceType.Food].Count = 10;
        Simulation.CelestialBodies.Span[1].Structures[StructureType.Farm].Count = 1;
        Simulation.CelestialBodies.Span[1].Structures[StructureType.Farm].ActiveCount = 1;

        Simulation.CelestialBodies.Span[2].Player = Simulation.Players.Span[1].Index;
        Simulation.CelestialBodies.Span[2].Resources[ResourceType.Population].Count = 10;
        Simulation.CelestialBodies.Span[2].Resources[ResourceType.Food].Count = 10;
        Simulation.CelestialBodies.Span[2].Structures[StructureType.Farm].Count = 1;
        Simulation.CelestialBodies.Span[2].Structures[StructureType.Farm].ActiveCount = 1;

        Simulation.Commit();

        Simulation.Event += (ev, revert) => GD.Print(revert ? $"Reverting event: {ev}" : $"Event: {ev}");

        Simulation.Initialize();

        DeterministicRandom celestialBodySeed = seed;

        for (int i = 0; i < Simulation.CelestialBodies.Span.Length; i++)
        {
            ref var body = ref Simulation.CelestialBodies.Span[i];

            DeterministicRandom thisCelestialBodySeed = celestialBodySeed.NextSeed();
            DeterministicRandom rand = thisCelestialBodySeed;

            CelestialBodyType type = i is 0 ? CelestialBodyType.Star : CelestialBodyType.Planet;
            CelestialBodyClass @class = body.Configuration.Class ?? rand.Pick(type.GetClasses().AsSpan());

            CelestialBody instance = Templates.Instantiate(@class);

            _celestialBodiesContainer.AddChild(instance);

            instance.Game = this;
            instance.Index = i;
            instance.Position = new Vector2(body.Configuration.Location.X, body.Configuration.Location.Y);

            instance.Initialize(@type, body.Configuration, thisCelestialBodySeed);

            body.Data = instance;
        }
    }

    public override void _Process(double delta)
    {
        Simulation.TickClock();

        foreach (ref var body in Simulation.CelestialBodies)
            ((CelestialBody)body.Data).Sync(body);

        _infoPanel.Show(Simulation, Simulation.CelestialBodies.Span[1].Index);
    }
}
