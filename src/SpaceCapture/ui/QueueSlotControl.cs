// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: AGPL-3.0-only

using Godot;

namespace SpaceCapture;

public partial class QueueSlotControl : Control
{
    [Export]
    TextureRect _iconRect;

    [Export]
    Label _nameLabel;

    [Export]
    ProgressBar _progressBar;

    public string Text
    {
        get => _nameLabel.Text;
        set => _nameLabel.Text = value;
    }

    public Texture2D Icon
    {
        get => _iconRect.Texture;
        set => _iconRect.Texture = value;
    }

    public double Progress
    {
        get => _progressBar.Value;
        set => _progressBar.Value = value;
    }
}
