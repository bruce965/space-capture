// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: AGPL-3.0-only

using Godot;

namespace SpaceCapture;

[GlobalClass]
public partial class Planet : Resource
{
    public enum PlanetType
    {
        Terrestrial,
    }

    [Export]
    public Vector2 Location { get; set; }

    [Export]
    public PlanetType Type { get; set; }
}
