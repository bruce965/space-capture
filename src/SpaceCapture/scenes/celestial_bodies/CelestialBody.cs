// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: AGPL-3.0-only

using System;
using System.Text;
using Godot;
using SpaceCapture.Shared;
using SpaceCapture.Shared.Logic;
using SpaceCapture.Shared.Logic.Simulation;

namespace SpaceCapture;

public partial class CelestialBody : Node2D
{
    public GameLogic Game { get; set; }

    public int Index { get; set; }

    [Export]
    public Label Label { get; set; }

    public void Sync(GameSimulation<Node2D>.CelestialBody data)
    {
        StringBuilder sb = new();
        foreach (ref var resource in data.Resources)
            if (resource.Count > 0)
                sb.AppendLine($"{resource.TypeData.Rules.Type}: {resource.Count}");

        foreach (ref var structure in data.Structures)
            if (structure.Count > 0)
                sb.AppendLine($"{structure.TypeData.Rules.Type}: {structure.Count}");

        if (data.BuildQueue.Count > 0)
            sb.AppendLine($"Building {data.BuildQueue[0].Type} {data.BuildQueue[0].Progress}/?");

        Label.Text = sb.ToString().TrimEnd();
    }

    public void OnGuiInput(InputEvent evt)
    {
        if (evt is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left })
            Game.Simulation.ExecuteAction(
                new BuildStructureAction(Game.Simulation.Tick, Index, StructureType.MetalMine)
            );
    }
}
