// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: AGPL-3.0-only

using System.Diagnostics.CodeAnalysis;
using Godot;
using SpaceCapture.Shared.Logic;
using SpaceCapture.Shared.Logic.Simulation;

namespace SpaceCapture;

public partial class InfoPanel : Control
{
    [Export]
    Label _nameLabel;

    [Export]
    Control _resourcesContainer;

    [Export]
    Control _structuresContainer;

    [Export]
    Control _queueContainer;

    [Export]
    PackedScene _resourceControlTemplate;

    [Export]
    PackedScene _structureControlTemplate;

    [Export]
    PackedScene _queueSlotTemplate;

    GameSimulation<Node2D> _lastGame;
    ResourceControl[] _resourceControls;
    StructureControl[] _structureControls;

    public void Show(GameSimulation<Node2D> game, GameSimulation<Node2D>.CelestialBodyIndex index)
    {
        EnsureInitialized(game);

        ref GameSimulation<Node2D>.CelestialBody celestialBody = ref game.CelestialBodies[index];

        _nameLabel.Text = celestialBody.Configuration.Name;

        for (int i = 0; i < celestialBody.Resources.Span.Length; i++)
            _resourceControls[i].Count = (float)celestialBody.Resources.Span[i].Count;

        for (int i = 0; i < celestialBody.Structures.Span.Length; i++)
        {
            _structureControls[i].ActiveCount = celestialBody.Structures.Span[i].ActiveCount;
            _structureControls[i].TotalCount = celestialBody.Structures.Span[i].Count;
        }

        // TODO: build queue.
    }

    [MemberNotNull(nameof(_resourceControls))]
    void EnsureInitialized(GameSimulation<Node2D> game)
    {
        if (_lastGame == game)
            return;

        foreach (Node child in _resourcesContainer.GetChildren())
        {
            _resourcesContainer.RemoveChild(child);
            child.QueueFree();
        }

        _resourceControls = new ResourceControl[game.Rules.Resources.Length];
        for (int i = 0; i < _resourceControls.Length; i++)
        {
            ResourceControl control = _resourceControlTemplate.Instantiate<ResourceControl>();
            control.Text = game.Rules.Resources[i].Rules.Type.ToString();
            control.Icon = Templates.Icons[game.Rules.Resources[i].Rules.Type];

            _resourceControls[i] = control;
            _resourcesContainer.AddChild(control);
        }

        foreach (Node child in _structuresContainer.GetChildren())
        {
            _structuresContainer.RemoveChild(child);
            child.QueueFree();
        }

        _structureControls = new StructureControl[game.Rules.Structures.Length];
        for (int i = 0; i < _structureControls.Length; i++)
        {
            Shared.StructureType type = game.Rules.Structures[i].Rules.Type;

            StructureControl control = _structureControlTemplate.Instantiate<StructureControl>();
            control.Text = type.ToString();
            control.Icon = Templates.Icons[type];

            // TODO: use currently selected celestial body index.
            control.Activate += () => game.ExecuteAction(new ActivateStructureAction(game.Tick, 1, type));
            control.Deactivate += () => game.ExecuteAction(new DeactivateStructureAction(game.Tick, 1, type));
            control.Build += () => game.ExecuteAction(new BuildStructureAction(game.Tick, 1, type));
            control.ToggleRepair += x => game.ExecuteAction(new ToggleRepairStructureAction(game.Tick, 1, type, x));

            _structureControls[i] = control;
            _structuresContainer.AddChild(control);
        }

        _lastGame = game;
    }
}
