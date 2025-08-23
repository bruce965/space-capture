// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: AGPL-3.0-only

using Godot;

namespace SpaceCapture;

public partial class ResourceControl : Control
{
    [Export]
    TextureRect _iconRect;

    [Export]
    Label _countLabel;

    float _count;

    public string Text
    {
        get => TooltipText;
        set => TooltipText = value;
    }

    public Texture2D Icon
    {
        get => _iconRect.Texture;
        set => _iconRect.Texture = value;
    }

    public float Count
    {
        get => _count;
        set
        {
            _count = value;
            _countLabel.Text = Utility.ToShortString(value);
        }
    }
}
