using Godot;

namespace SpaceCapture;

public partial class PlanetsUI : Control
{
    Trail _trail;

    /// <summary>
    /// Container for trails.
    /// </summary>
    /// <value></value>
    [Export]
    public Node2D TrailsContainer { get; set; }

    [Signal]
    public delegate void FleetDispatchedEventHandler(ControlPlanet from, ControlPlanet to);

    public void RegisterPlanet(ControlPlanet control)
    {
        control.Selected += OnPlanetSelected;
        control.PointerEntered += OnPlanetPointerEntered;
        control.PointerExited += OnPlanetPointerExited;
    }

    public override void _Ready()
    {
        _trail = SceneTemplates.Scenes.Trail.Instantiate<Trail>();
        _trail.Visible = false;
        TrailsContainer.AddChild(_trail);
    }

    public override void _Process(double delta)
    {
        UpdateTrail(delta, false);
    }

    public override void _UnhandledInput(InputEvent inputEvent)
    {
        if (inputEvent is InputEventMouse e)
            HandleMouseInput(e);
    }

    #region Mouse input

    Vector2 _lastCursorPosition;

    void HandleMouseInput(InputEventMouse inputEvent)
    {
        if (inputEvent is InputEventMouseMotion e)
        {
            // The trail follows the cursor.
            _lastCursorPosition = e.GlobalPosition;
        }

        if (inputEvent is InputEventMouseButton e2 && e2.ButtonIndex is MouseButton.Left && e2.IsReleased())
        {
            // The trail disappears when no longer dragging.
            _draggingFromPlanet = false;
            
            // Detect mouse released on a planet and spawn a fleet.
            if (_selectedPlanet?.Player is PlayerLocal && _pointedPlanet is not null)
                EmitSignal(SignalName.FleetDispatched, _selectedPlanet, _pointedPlanet);
        }
    }

    #endregion

    #region Planets

    ControlPlanet _selectedPlanet;
    ControlPlanet _pointedPlanet;
    bool _draggingFromPlanet;

    void OnPlanetSelected(ControlPlanet planet)
    {
        if (_selectedPlanet is not null)
            _selectedPlanet.IsSelected = false;

        _selectedPlanet = planet;
        _selectedPlanet.IsSelected = true;

        _draggingFromPlanet = true;
        UpdateTrail(0.0, true);
    }

    void OnPlanetPointerEntered(ControlPlanet planet)
    {
        _pointedPlanet = planet;
    }

    void OnPlanetPointerExited(ControlPlanet planet)
    {
        if (_pointedPlanet == planet)
            _pointedPlanet = null;
    }

    #endregion

    #region Trail

    void UpdateTrail(double delta, bool snapPosition)
    {
        _trail.ShowTrail = _draggingFromPlanet && _selectedPlanet is not null && _selectedPlanet.Player is PlayerLocal;
        if (_trail.ShowTrail)
        {
            _trail.Color = _selectedPlanet.Player.Color;
            var targetPosition = _pointedPlanet?.GlobalPosition ?? _lastCursorPosition;
            _trail.StartPosition = _selectedPlanet.GlobalPosition;
            _trail.EndPosition = snapPosition ? targetPosition : Utils.Damp(_trail.EndPosition, targetPosition, 1e-20f, delta);
        }
    }

    #endregion
}
