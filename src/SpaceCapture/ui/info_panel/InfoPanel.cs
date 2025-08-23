// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: AGPL-3.0-only

using System.Diagnostics.CodeAnalysis;
using Godot;
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

    ResourceControl[] _resourceControls;

    GameSimulation<Node2D> _lastGame;

    public void Show(GameSimulation<Node2D> game, GameSimulation<Node2D>.CelestialBodyIndex index)
    {
        EnsureInitialized(game);

        ref GameSimulation<Node2D>.CelestialBody celestialBody = ref game.CelestialBodies[index];

        _nameLabel.Text = celestialBody.Configuration.Name;

        for (int i = 0; i < celestialBody.Resources.Span.Length; i++)
            _resourceControls[i].Count = (float)celestialBody.Resources.Span[i].Count;

        // TODO: structures.

        // TODO: build queue.

        _structuresContainer.GetChildren();
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

        // TODO: structures.

        _lastGame = game;
    }
}
