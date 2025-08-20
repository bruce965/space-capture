// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: AGPL-3.0-only

using System.Globalization;
using Godot;
using SpaceCapture.Shared.Logic;
using SpaceCapture.Shared.Logic.Simulation;
using SpaceCapture.Shared.Logic.Stage;
using SpaceCapture.Shared.Utilities;

namespace SpaceCapture;

public partial class GameLogic : Node
{
    /// <summary>
    /// Container for planets.
    /// </summary>
    [Export]
    public Node2D CelestialBodiesContainer { get; set; }

    GameSimulation<Node2D> _game;

    public override void _Ready()
    {
        DeterministicRandom seed = DeterministicRandom.Parse("test", CultureInfo.InvariantCulture);
        GameStage stage = GameGenerator.FromSeed(seed);
        _game = new(stage);

        foreach (ref var body in _game.CelestialBodies)
        {
            Node2D instance = Templates
                .Scenes.CelestialBodies[body.Configuration.Type]
                .Instantiate<Node2D>();

            CelestialBodiesContainer.AddChild(instance);

            instance.Position = new Vector2(
                body.Configuration.Location.X,
                body.Configuration.Location.Y
            );

            body.Data = instance;
        }
    }

    public override void _Process(double delta)
    {
        _game.TickClock();

        // TODO
    }
}
