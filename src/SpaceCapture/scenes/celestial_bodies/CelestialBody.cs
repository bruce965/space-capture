// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: AGPL-3.0-only

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
    Label _label;

    public void Sync(GameSimulation<Node2D>.CelestialBody data)
    {
        StringBuilder sb = new();

        sb.AppendLine(data.Configuration.Name);

        if (data.BuildQueue.Count > 0)
            sb.AppendLine($"Building {data.BuildQueue[0].Type} {data.BuildQueue[0].Progress}/?");

        _label.Text = sb.ToString().TrimEnd();
    }

    public void OnGuiInput(InputEvent evt)
    {
        if (evt is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left })
            Game.Simulation.ExecuteAction(
                new BuildStructureAction(Game.Simulation.Tick, Index, StructureType.MetalMine)
            );
    }
}
