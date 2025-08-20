// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: AGPL-3.0-only

using Godot;
using Godot.Collections;
using SpaceCapture.Shared;

namespace SpaceCapture;

public partial class Templates : Resource
{
    public static Templates Scenes { get; } =
        ResourceLoader.Load<Templates>("res://scenes/templates/resource.tres");

    [Export]
    public Dictionary<CelestialBodyType, PackedScene> CelestialBodies { get; set; }
}
