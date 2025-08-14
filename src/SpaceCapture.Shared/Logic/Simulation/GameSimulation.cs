// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: AGPL-3.0-only

using SpaceCapture.Shared.Logic.State;
using SpaceCapture.Shared.Types;

namespace SpaceCapture.Shared.Logic.Simulation;

/// <summary>
/// Executes all the game logic.
/// </summary>
public partial class GameSimulation
{
    const long TicksPerSecond = 60;
    const long FarSnapshotTicks = TicksPerSecond * 60 * 5; // 5 minutes
    const long NearSnapshotTicks = TicksPerSecond * 5; // 5 seconds

    readonly Cache[] _snapshots;
    ref Cache Initial => ref _snapshots[0]; // Initial state.
    ref Cache PreviousFarSnapshot => ref _snapshots[1]; // State 5~10 minutes ago.
    ref Cache NextFarSnapshot => ref _snapshots[2]; // State 0~5 minutes ago.
    ref Cache PreviousNearSnapshot => ref _snapshots[3]; // State 5~10 seconds ago.
    ref Cache NextNearSnapshot => ref _snapshots[4]; // State 0~5 seconds ago.
    ref Cache Current => ref _snapshots[5]; // Current state.

    readonly RulesCache _rules;

    /// <summary>
    /// Something happened in the game. This might also be an event that reverts
    /// another event that occurred previously, for instance when executing an
    /// action in the past invalidates another action that came afterwards.
    /// </summary>
    public event Action<GameEvent>? Event;

    readonly List<GameAction> _actionsHistory; // Executed or planned actions.

    readonly List<GameEvent> _eventsHistory; // Occurred events, excluding cancelled.

    /// <summary>
    /// Current state of a game, should be treated as read-only.
    /// </summary>
    public GameState State => Current.State;

    /// <summary>
    /// Initialize a new simuation from the specified state.
    /// </summary>
    /// <param name="state"></param>
    public GameSimulation(GameState state)
    {
        Cache c = new(state.Clone());
        _rules = new(state.Configuration.Rules);
        _snapshots = [c, c.Clone(), c.Clone(), c.Clone(), c.Clone(), c.Clone()];

        _actionsHistory = new(65536);
        _eventsHistory = new(65536);
    }

    /// <summary>
    /// Execute an action, possibly in the past.
    /// </summary>
    /// <remarks>
    /// For maximum performance, <see cref="GameAction.Tick"/> should be set to
    /// at least one tick higher than the current <see cref="GameState.Tick"/>.
    /// </remarks>
    /// <param name="action"></param>
    public void ExecuteAction(GameAction action)
    {
        long currentTick = Current.State.Tick;
        if (action.Tick < currentTick)
            MoveClock(action.Tick);

        bool done = false;
        for (int i = _actionsHistory.Count - 1; i >= 0; i--)
        {
            if (_actionsHistory[i].Tick < action.Tick)
            {
                _actionsHistory.Insert(i + 1, action);
                done = true;
                break;
            }
        }

        if (!done)
            _actionsHistory.Insert(0, action);

        if (action.Tick < currentTick)
            MoveClock(currentTick);
    }

    /// <summary>
    /// Move the clock back or forward to the specified tick.
    /// </summary>
    /// <param name="tick"></param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public void MoveClock(long tick)
    {
        // The requested tick is in the past, a rollback is necessary.
        if (tick < Current.State.Tick)
        {
            if (tick < Initial.State.Tick)
                throw new ArgumentOutOfRangeException(
                    nameof(tick),
                    "Unable to rollback to before the start of the game."
                );

            for (int i = _snapshots.Length - 1; i >= 0; i--)
            {
                // Look for the most recent snapshot before or at the requested tick.
                Cache snapshot = _snapshots[i];
                if (snapshot.State.Tick >= tick)
                    continue;

                // Rollback all the snapshots that follow it, including the current state.
                for (int j = i + 1; j < _snapshots.Length; j++)
                    _snapshots[j].CopyFrom(snapshot);

                break;
            }
        }

        // Fast-forward to the requested tick.
        while (Current.State.Tick < tick)
            TickClock();
    }

    /// <summary>
    /// Advance the clock by one tick.
    /// </summary>
    public void TickClock()
    {
        // Step into the next tick.
        Current.State.Tick++;

        // Process actions.
        for (; Current.ActionsCount < _actionsHistory.Count; Current.ActionsCount++)
        {
            ProcessAction(_actionsHistory[Current.ActionsCount]);

            // TODO: validate past events and revert the ones that are no longer valid.
        }

        // TODO: move fleets.

        // TODO: resolve conflicts.

        // Produce resources.
        for (int i = 0; i < Current.State.Configuration.CelestialBodies.Length; i++)
        {
            foreach (CelestialBodyCache body in Current.CelestialBodies)
            {
                foreach (ref CelestialBodyResourcesCache res in body.Resources)
                {
                    int factoriesCount = 0;
                    foreach (var s in _rules.FactoriesByResource[res.Configuration.Type])
                        factoriesCount += body.Structures[s].Data.Count;

                    FP32D10 increment = res.Configuration.ProductionRate * factoriesCount;
                    res.Data = new(res.Data.Type, res.Data.Count + increment);
                }
            }
        }

        // Take a "near" snapshot if enough time has passed since last one.
        if (Current.State.Tick - NextNearSnapshot.State.Tick > NearSnapshotTicks)
        {
            (PreviousNearSnapshot, NextNearSnapshot, Current) = (
                NextNearSnapshot,
                Current,
                PreviousNearSnapshot
            );

            Current.CopyFrom(NextNearSnapshot);
        }

        // Take a "far" snapshot if enough time has passed since last one.
        if (Current.State.Tick - NextFarSnapshot.State.Tick > FarSnapshotTicks)
        {
            (PreviousFarSnapshot, NextFarSnapshot, Current) = (
                NextFarSnapshot,
                Current,
                PreviousFarSnapshot
            );

            Current.CopyFrom(NextFarSnapshot);
        }
    }

    void ProcessAction(GameAction action)
    {
        // TODO
    }
}
