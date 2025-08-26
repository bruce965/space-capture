// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: AGPL-3.0-only

using System.Globalization;
using Godot;

namespace SpaceCapture;

public partial class StructureControl : Control
{
    [Export]
    TextureRect _iconRect;

    [Export]
    Label _activeCountLabel;

    [Export]
    Label _totalCountLabel;

    [Export]
    Button _decrementButton;

    [Export]
    Button _incrementButton;

    int _activeCount;
    int _totalCount;

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

    public int ActiveCount
    {
        get => _activeCount;
        set
        {
            _activeCount = value;
            _activeCountLabel.Text = value.ToString(CultureInfo.InvariantCulture);
            RefreshButtons();
        }
    }

    public int TotalCount
    {
        get => _totalCount;
        set
        {
            _totalCount = value;
            _totalCountLabel.Text = value.ToString(CultureInfo.InvariantCulture);
            RefreshButtons();
        }
    }

    [Signal]
    public delegate void ActivateEventHandler();

    [Signal]
    public delegate void DeactivateEventHandler();

    [Signal]
    public delegate void BuildEventHandler();

    [Signal]
    public delegate void ToggleRepairEventHandler(bool enabled);

    public override void _Ready()
    {
        RefreshButtons();
    }

    void RefreshButtons()
    {
        _decrementButton.Disabled = _activeCount <= 0;
        _incrementButton.Disabled = _activeCount >= _totalCount;
    }

    void OnDecrementButtonPressed() => EmitSignalDeactivate();

    void OnIncrementButtonPressed() => EmitSignalActivate();

    void OnBuildButtonPressed() => EmitSignalBuild();

    void OnRepairButtonToggled(bool toggledOn) => EmitSignalToggleRepair(toggledOn);
}
