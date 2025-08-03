using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;

namespace SpaceCapture;

public partial class GameState : Resource
{
    public class PlayerData
    {
        public List<Command> Commands { get; set; } = [];
    }

    public class PlanetData
    {
        public Vector2I Position { get; set; }
        public int GrowEveryTicks { get; set; }
        public int PlayerId { get; set; }
        public int Population { get; set; }
    }

    public class FleetData
    {
        public int Count { get; set; }
        public int PlayerId { get; set; }
        public int ToPlanetId { get; set; }
        public int ArrivesAtTick { get; set; }
    }

    public enum CommandType
    {
        DispatchFleet,
    }

    public abstract class Command(CommandType type)
    {
        public CommandType Type => type;
        public int WaitTick { get; set; }
    }

    public class DispatchFleetCommand() : Command(CommandType.DispatchFleet)
    {
        public int FromPlanetId { get; set; }
        public int ToPlanetId { get; set; }
        public int MaxCount { get; set; }
    }

    const int NeutralPlayerId = 0;
    const int FleetTakeoffPlusLandingTime = 20; // ticks
    const int FleetSpeed = 2; // pixels/tick

    [Signal]
    public delegate void GameTickedEventHandler(int tick);

    [Signal]
    public delegate void PlanetPopulationChangedEventHandler(int planetId, int population);

    [Signal]
    public delegate void PlanetPlayerChangedEventHandler(int planetId, int playerId);

    [Signal]
    public delegate void FleetDispatchedEventHandler(int fromPlanetId, int toPlanetId, int count, int playerId, int departedAtTick, int arrivesAtTick);

    List<PlayerData> _players = [];
    List<PlanetData> _planets = [];
    List<FleetData> _fleets = [];

    public int CurrentTick { get; private set; }

    /// <summary>
    /// Add a player to the game.
    /// <para>
    /// The first player is expected to be the game master / neutral player,
    /// who does not play and for whom the population never increases.
    /// </para>
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public int AddPlayer(PlayerData data = null)
    {
        _players.Add(data ?? new());
        return _players.Count - 1;
    }

    /// <summary>
    /// Add a planet to the game.
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public int AddPlanet(PlanetData data = null)
    {
        _planets.Add(data ?? new());
        return _planets.Count - 1;
    }

    /// <summary>
    /// Advance game status by one tick.
    /// </summary>
    /// <returns></returns>
    public void Tick()
    {
        CurrentTick++;

        // Handle planets growth.
        for (int planetId = 0; planetId < _planets.Count; planetId++)
        {
            PlanetData planet = _planets[planetId];
            if (planet.PlayerId is not NeutralPlayerId && CurrentTick % planet.GrowEveryTicks is 0)
            {
                planet.Population++;
                EmitSignal(SignalName.PlanetPopulationChanged, planetId, planet.Population);
            }
        }

        // Handle player commands.
        for (int player_id = 0; player_id < _players.Count; player_id++)
        {
            PlayerData player = _players[player_id];
            if (player.Commands.Count > 0 && CurrentTick >= player.Commands[0].WaitTick)
            {
                Command command = player.Commands[^1];
                player.Commands.RemoveAt(player.Commands.Count - 1);

                switch (command.Type)
                {
                    case CommandType.DispatchFleet:
                        HandleDispatchFleet(player_id, (DispatchFleetCommand)command);
                        break;

                    default:
                        Debug.Assert(false, $"Unknown command: {command}");
                        break;
                }
            }
        }

        // Handle fleets arrival.
        while (_fleets.Count > 0 && CurrentTick >= _fleets[0].ArrivesAtTick)
        {
            FleetData fleet = _fleets[^1];
            _fleets.RemoveAt(_fleets.Count - 1);

            _handle_fleet_arrival(fleet);
        }

        EmitSignal(SignalName.GameTicked, CurrentTick);
    }

    /// <summary>
    /// Dispatch a fleet.
    /// </summary>
    /// <param name="playerId"></param>
    /// <param name="fromPlanetId"></param>
    /// <param name="toPlanetId"></param>
    /// <param name="maxCount"></param>
    public void DispatchFleet(int playerId, int fromPlanetId, int toPlanetId, int maxCount)
        => IssueCommand(playerId, new DispatchFleetCommand() {
            FromPlanetId = fromPlanetId,
            ToPlanetId = toPlanetId,
            MaxCount = maxCount,
        });

    void IssueCommand(int player_id, Command command)
    {
        List<Command> commands = _players[player_id].Commands;
        int index = commands.Select(x => x.WaitTick).ToList().BinarySearch(command.WaitTick);
        if (index < 0)
            index = ~index;
        commands.Insert(index, command);
    }

    void HandleDispatchFleet(int playerId, DispatchFleetCommand dispatch)
    {
        PlanetData from = _planets[dispatch.FromPlanetId];
        if (from.PlayerId == playerId)
        {
            PlanetData to = _planets[dispatch.ToPlanetId];
            int count = Math.Min(dispatch.MaxCount, from.Population);
            from.Population -= count;
            EmitSignal(SignalName.PlanetPopulationChanged, dispatch.FromPlanetId, from.Population);

            FleetData fleet = new() {
                Count = count,
                PlayerId = playerId,
                ToPlanetId = dispatch.ToPlanetId,
                ArrivesAtTick = CurrentTick + FleetTakeoffPlusLandingTime + Mathf.CeilToInt(from.Position.DistanceTo(to.Position) / FleetSpeed),
            };

            int index = _fleets.Select(x => x.ArrivesAtTick).ToList().BinarySearch(fleet.ArrivesAtTick);
            if (index < 0)
                index = ~index;
            _fleets.Insert(index, fleet);

            EmitSignal(SignalName.FleetDispatched, dispatch.FromPlanetId, fleet.ToPlanetId, fleet.Count, fleet.PlayerId, CurrentTick, fleet.ArrivesAtTick);
        }
    }

    void _handle_fleet_arrival(FleetData fleet)
    {
        PlanetData to = _planets[fleet.ToPlanetId];
        if (fleet.PlayerId == to.PlayerId)
        {
            to.Population += fleet.Count;
        }
        else
        {
            to.Population -= fleet.Count;
            if (to.Population <= 0)
            {
                to.PlayerId = fleet.PlayerId;
                to.Population = Mathf.Abs(to.Population);
                EmitSignal(SignalName.PlanetPlayerChanged, fleet.ToPlanetId, to.PlayerId);
            }

            EmitSignal(SignalName.PlanetPopulationChanged, fleet.ToPlanetId, to.Population);
        }
    }
}
