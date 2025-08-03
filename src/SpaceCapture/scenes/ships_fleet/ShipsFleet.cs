using System;
using System.Collections.Generic;
using Godot;

namespace SpaceCapture;

public partial class ShipsFleet : Node2D
{
    const int RankSize = 7;
    const float RanksDistance = 12f;
    const float ShoulderDistance = 8f;
    const float DisperseAtDistance = 100f;

    int _count;
    GameState _game;

    [Export]
    public int DepartedAt { get; set; } // Time.GetTicksMsec()

    [Export]
    public int ArrivesAt { get; set; } // Time.GetTicksMsec()

    [Export]
    public Vector2 From { get; set; }

    [Export]
    public Vector2 To { get; set; }

    [Export]
    public Player Player { get; set; }

    [Export]
    public int Count
    {
        get => _count;
        set
        {
            _count = value;
            UpdateShipsCount();
        }
    }

    [Export]
    public Trail Trail { get; set; }

    public void RegisterGame(GameState game)
    {
        _game = game;
        _game.GameTicked += UpdatePosition;
        UpdatePosition(game.CurrentTick);
    }

    public override void _ExitTree()
    {
        if (_game is not null)
        {
            _game.GameTicked -= UpdatePosition;
            _game = null;
        }
    }

    #region Graphics

    List<Ship> _ships = [];

    [Export]
    public PackedScene ShipTemplate { get; set; }

    void UpdateShipsCount()
    {
        int diff = Count - _ships.Count;

        for (int i = 0; i < diff; i++)
        {
            Ship ship = ShipTemplate.Instantiate<Ship>();
            ship.Color = Player.Color;
            AddChild(ship);
            ship.Position = From;
            ship.Rotation = Random.Shared.NextSingle() * (2f * MathF.PI);
            ship.Velocity = (Random.Shared.NextSingle() * 60f + 60f) * Vector2.FromAngle(ship.Rotation);
            _ships.Add(ship);
        }

        for (int i = 0; i < -diff; i++)
        {
            RemoveChild(_ships[^1]);
            _ships.RemoveAt(_ships.Count - 1);
        }
    }

    bool _arrived = false;

    void UpdatePosition(int tick)
    {
        if (!_arrived)
        {
            int clampedTick = Math.Clamp(tick, DepartedAt, ArrivesAt);
            float progress = (clampedTick - DepartedAt) / (float)(ArrivesAt - DepartedAt);
            Vector2 fleetPosition = From.Lerp(To, Math.Clamp(progress, 0f, 1f));
            float dispersiveness = Math.Clamp(MathF.Sqrt(Math.Min(fleetPosition.DistanceSquaredTo(From), fleetPosition.DistanceSquaredTo(To))) / DisperseAtDistance, 0f, 1f);

            float angle = From.AngleToPoint(To);
            int ranksCount = Mathf.CeilToInt((float)_ships.Count / RankSize);
            for (int i = 0; i < _ships.Count; i++)
            {
                int rank = Mathf.FloorToInt((float)i / RankSize);
                int positionInRank = i % RankSize;
                var shipsInRank = rank != ranksCount - 1 ? RankSize : (_ships.Count - (ranksCount - 1) * RankSize);
                Vector2 assignedPosition = new(rank * -RanksDistance - Math.Abs(positionInRank - shipsInRank / 2f) * 5f, (float)(positionInRank - (shipsInRank - 1) * .5f) * ShoulderDistance);
                _ships[i].TargetPosition = fleetPosition + Vector2.Zero.Lerp(assignedPosition.Rotated(angle) + Vector2.FromAngle(MathF.Sin(i + tick / 10f)) * 2f, dispersiveness);
            }

            if (tick >= ArrivesAt)
            {
                _arrived = true;
                Trail.ShowTrail = false;

                // TODO: fade out ships.

                GetTree().CreateTimer(5.0).Timeout += QueueFree;
            }
        }
    }

    #endregion
}
