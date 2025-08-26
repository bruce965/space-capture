// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: AGPL-3.0-only

using System.ComponentModel;
using Godot;
using Godot.Collections;
using SpaceCapture.Shared;

namespace SpaceCapture;

public partial class Templates : Resource
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public readonly struct IconTemplates
    {
        public Texture2D this[ResourceType type] => s_instance._resourceIcons[type];

        public Texture2D this[StructureType type] => s_instance._structureIcons[type];
    }

    static readonly Templates s_instance = ResourceLoader.Load<Templates>("res://scenes/templates/resource.tres");

    public static IconTemplates Icons => default;

    [Export]
    Dictionary<CelestialBodyType, PackedScene> _celestialBodies;

    [Export]
    Dictionary<ResourceType, Texture2D> _resourceIcons;

    [Export]
    Dictionary<StructureType, Texture2D> _structureIcons;

    public static CelestialBody Instantiate(CelestialBodyType type) =>
        s_instance._celestialBodies[type].Instantiate<CelestialBody>();
}
