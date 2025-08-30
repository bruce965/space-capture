// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: AGPL-3.0-only

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Godot;
using SpaceCapture.Shared.Logic.Actions;
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
    List<QueueSlotControl> _queueSlotControls;

    public void Show(GameSimulation<Node2D> game, GameSimulation<Node2D>.CelestialBodyIndex index)
    {
        EnsureInitialized(game);

        ref GameSimulation<Node2D>.CelestialBody celestialBody = ref game.CelestialBodies[index];

        // Update name.
        _nameLabel.Text = celestialBody.Configuration.Name;

        // Update resources.
        for (int i = 0; i < celestialBody.Resources.Span.Length; i++)
        {
            _resourceControls[i].Count = (float)celestialBody.Resources.Span[i].Count;
            _resourceControls[i].Visible = _resourceControls[i].Count > 0;
        }

        // Update structures.
        for (int i = 0; i < celestialBody.Structures.Span.Length; i++)
        {
            _structureControls[i].ActiveCount = celestialBody.Structures.Span[i].ActiveCount;
            _structureControls[i].TotalCount = celestialBody.Structures.Span[i].Count;
        }

        // Update build queue.
        int j = 0;
        foreach (BuildQueueSlot slot in celestialBody.BuildQueue)
        {
            QueueSlotControl control;
            if (_queueSlotControls.Count <= j)
            {
                control = _queueSlotTemplate.Instantiate<QueueSlotControl>();

                _queueSlotControls.Add(control);
                _queueContainer.AddChild(control);
            }
            else
            {
                control = _queueSlotControls[j];
            }

            control.Visible = true;
            control.Text = slot.Type.ToString();
            control.Icon = Templates.Icons[slot.Type];
            control.Progress = (double)slot.Progress / game.Rules.Structures[slot.Type].Rules.BuildTicks;

            j++;
        }

        for (; j < _queueSlotControls.Count; j++)
            _queueSlotControls[j].Visible = false;
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

        _resourceControls = new ResourceControl[game.Rules.Resources.Span.Length];
        for (int i = 0; i < _resourceControls.Length; i++)
        {
            Shared.ResourceType type = game.Rules.Resources.Span[i].Rules.Type;

            ResourceControl control = _resourceControlTemplate.Instantiate<ResourceControl>();
            control.Text = type.ToString();
            control.Icon = Templates.Icons[type];

            _resourceControls[i] = control;
            _resourcesContainer.AddChild(control);
        }

        foreach (Node child in _structuresContainer.GetChildren())
        {
            _structuresContainer.RemoveChild(child);
            child.QueueFree();
        }

        _structureControls = new StructureControl[game.Rules.Structures.Span.Length];
        for (int i = 0; i < _structureControls.Length; i++)
        {
            Shared.StructureType type = game.Rules.Structures.Span[i].Rules.Type;

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

        _queueSlotControls = [];

        foreach (Node child in _queueContainer.GetChildren())
        {
            _queueContainer.RemoveChild(child);
            child.QueueFree();
        }

        _lastGame = game;
    }
}
