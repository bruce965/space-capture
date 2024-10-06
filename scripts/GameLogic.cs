using System.Collections.Generic;
using Godot;
using Godot.Collections;

namespace SpaceCapture;

public partial class GameLogic : Node
{
    const float LogicTicksPerSecond = 20f;
    const float LogicSecondsPerTick = 1f / LogicTicksPerSecond;

    GameState _game;
    List<ControlPlanet> _planetControls = [];

    /// <summary>
    /// Container for planets.
    /// </summary>
    /// <value></value>
    [Export]
    public Node2D PlanetsContainer { get; set; }

    /// <summary>
    /// Container for trails.
    /// </summary>
    /// <value></value>
    [Export]
    public Node2D TrailsContainer { get; set; }

    /// <summary>
    /// Container for fleets.
    /// </summary>
    /// <value></value>
    [Export]
    public Node2D FleetsContainer { get; set; }

    /// <summary>
    /// UI to interact with planets.
    /// </summary>
    /// <value></value>
    [Export]
    public PlanetsUI PlanetsUI { get; set; }

    /// <summary>
    /// Game players.
    /// </summary>
    /// <value></value>
    [Export]
    public Array<Player> Players { get; set; }

    /// <summary>
    /// Game planets.
    /// </summary>
    /// <value></value>
    [Export]
    public Array<Planet> Planets { get; set; }

    double _extraTime;

    public override void _Ready()
    {
        // New game.
        _game = new();

        // Add players.
        for (int i = 0; i < Players.Count; i++)
            _game.AddPlayer();

        // Add planets.
        for (int i = 0; i < Planets.Count; i++)
        {
            // Data.
            GameState.PlanetData data = new();
            data.Position = (Vector2I)Planets[i].Location;
            data.GrowEveryTicks = 10;
            data.PlayerId = i < Players.Count - 1 ? i + 1 : 0;
            data.Population = 0;
            _game.AddPlanet(data);

            // UI control.
            ControlPlanet control = SceneTemplates.Scenes.ControlPlanet.Instantiate<ControlPlanet>();
            control.Position = Planets[i].Location;
            control.Player = Players[data.PlayerId];
            control.Population = data.Population;
            _planetControls.Add(control);
            PlanetsContainer.AddChild(control);
            PlanetsUI.RegisterPlanet(control);
        }

        // Register UI signals.
        _game.PlanetPlayerChanged += SetPlanetPlayer;
        _game.PlanetPopulationChanged += SetPlanetPopulation;
        _game.FleetDispatched += ShowDispatchedFleet;
        PlanetsUI.FleetDispatched += DispatchFleet;
    }

    public override void _Process(double delta)
    {
        _extraTime += delta;
        
        while (_extraTime > LogicSecondsPerTick)
        {
            _extraTime -= LogicSecondsPerTick;
            _game.Tick();
        }
    }

    #region Graphics

    void SetPlanetPlayer(int planetId, int playerId)
        => _planetControls[planetId].Player = Players[playerId];

    void SetPlanetPopulation(int planetId, int population)
        => _planetControls[planetId].Population = population;

    void ShowDispatchedFleet(int fromPlanetId, int toPlanetId, int count, int playerId, int departedAtTick, int arrivesAtTick)
    {
        Trail trail = SceneTemplates.Scenes.Trail.Instantiate<Trail>();
        trail.StartPosition = Planets[fromPlanetId].Location;
        trail.EndPosition = Planets[toPlanetId].Location;
        trail.Color = Players[playerId].Color;
        trail.AutoFree = true;
        TrailsContainer.AddChild(trail);

        ShipsFleet fleet = SceneTemplates.Scenes.ShipsFleet.Instantiate<ShipsFleet>();
        fleet.DepartedAt = departedAtTick;
        fleet.ArrivesAt = arrivesAtTick;
        fleet.From = Planets[fromPlanetId].Location;
        fleet.To = Planets[toPlanetId].Location;
        fleet.Player = Players[playerId];
        fleet.Count = count;
        fleet.Trail = trail;
        FleetsContainer.AddChild(fleet);

        fleet.RegisterGame(_game);
    }

    #endregion

    #region Commands

    void DispatchFleet(ControlPlanet from, ControlPlanet to)
    {
        int playerId = Players.IndexOf(from.Player);
        int fromPlanetId = _planetControls.IndexOf(from);
        int toPlanetId = _planetControls.IndexOf(to);
        int maxCount = Mathf.CeilToInt(from.Population / 2f);
        _game.DispatchFleet(playerId, fromPlanetId, toPlanetId, maxCount);
    }

    #endregion
}
