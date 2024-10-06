using Godot;

namespace SpaceCapture;

public partial class ControlPlanet : Node2D
{
    Player _player;
    int _population;

    /// <summary>
    /// The player currenly controlling this planet.
    /// </summary>
    /// <value></value>
    [Export]
    public Player Player
    {
        get => _player;
        set
        {
            _player = value;
            Color color = _player?.Color ?? Colors.Magenta;
            GetNode<ColorRect>("%Selection").Color = color;
            GetNode<Label>("%PopulationLabel").Modulate = color;
            GetNode<TextureRect>("%PopulationIcon").Modulate = color;
            GetNode<TextureRect>("%PopulationIcon").Texture = Player?.Icon;
        }
    }

    /// <summary>
    /// Number of units currently stationed at the planet.
    /// </summary>
    /// <value></value>
    [Export]
    public int Population
    {
        get => _population;
        set
        {
            _population = value;
            GetNode<Label>("%PopulationLabel").Text = $"{value}";
        }
    }

    public override void _Ready()
    {
        IsSelected = false;
    }

    [ExportCategory("UI")]
    object _placeholder1;

    [Signal]
    public delegate void SelectedEventHandler(ControlPlanet planet);

    [Signal]
    public delegate void PointerEnteredEventHandler(ControlPlanet planet);

    [Signal]
    public delegate void PointerExitedEventHandler(ControlPlanet planet);

    public bool IsSelected
    {
        get => GetNode<ColorRect>("%Selection").Visible;
        set => GetNode<ColorRect>("%Selection").Visible = value;
    }

    void OnInputListenerInputEvent(Node _viewport, InputEvent inputEvent, int _shape_idx)
    {
        // TODO: handle touch appropriately (InputEventScreenTouch).
        if (inputEvent is InputEventMouseButton e && e.ButtonIndex is MouseButton.Left && e.IsPressed())
            EmitSignal(SignalName.Selected, this);
    }

    void OnInputListenerMouseEntered()
        => EmitSignal(SignalName.PointerEntered, this);

    void OnInputListenerMouseExited()
        => EmitSignal(SignalName.PointerExited, this);
}
