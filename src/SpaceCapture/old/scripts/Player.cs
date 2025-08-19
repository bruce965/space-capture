// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: AGPL-3.0-only

using Godot;

namespace SpaceCapture;

[GlobalClass]
public partial class Player : Resource
{
    [Export]
    public Color Color { get; set; } = Colors.Magenta;

    [Export]
    public Texture2D Icon { get; set; }

    public virtual void ProcessGameTick(GameLogic game)
    {
    }
}
