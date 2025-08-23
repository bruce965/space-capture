// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: AGPL-3.0-only

using SpaceCapture.Shared.Logic.Stage;
using SpaceCapture.Shared.Types;
using SpaceCapture.Shared.Utilities;

namespace SpaceCapture.Shared.Logic.Simulation;

/// <inheritdoc cref="GameSimulation{TData}"/>
public class GameSimulation(GameStage stage) : GameSimulation<Empty>(stage);

/// <summary>
/// Executes all the game logic.
/// </summary>
public partial class GameSimulation<TData>
{
    const long FarSnapshotTicks = GameConstants.TicksPerSecond * 60 * 5; // 5 minutes
    const long NearSnapshotTicks = GameConstants.TicksPerSecond * 5; // 5 seconds

    readonly GameStage _stage;

    readonly RulesCache _rules;

    readonly Snapshot[] _snapshots;
    ref Snapshot Initial => ref _snapshots[0]; // Initial state.
    ref Snapshot PrevFarSnapshot => ref _snapshots[1]; // State 5~10 minutes ago.
    ref Snapshot NextFarSnapshot => ref _snapshots[2]; // State 0~5 minutes ago.
    ref Snapshot PrevNearSnapshot => ref _snapshots[3]; // State 5~10 seconds ago.
    ref Snapshot NextNearSnapshot => ref _snapshots[4]; // State 0~5 seconds ago.
    ref Snapshot Current => ref _snapshots[5]; // Current state.

    public long Tick => Current.Tick;

    public RulesCache Rules => _rules;

    public GameStage Stage => _stage;

    public Accessor<Player, PlayerIndex> Players => Current.Players;

    public Accessor<CelestialBody, CelestialBodyIndex> CelestialBodies => Current.CelestialBodies;

    /// <summary>
    /// Something happened in the game. This might also be an event that reverts
    /// another event that occurred previously, for instance when executing an
    /// action in the past invalidates another action that came afterwards.
    /// </summary>
    public event Action<GameEvent>? Event;

    readonly List<GameAction> _actionsHistory; // Executed or planned actions.

    readonly List<GameEvent> _eventsHistory; // Occurred events, excluding cancelled.

    /// <summary>
    /// Initialize a new simuation from the specified state.
    /// </summary>
    /// <param name="state"></param>
    public GameSimulation(GameStage stage)
    {
        _stage = stage;
        _rules = new(_stage.Rules);

        Snapshot c = new(this);
        _snapshots = [c, c.Clone(), c.Clone(), c.Clone(), c.Clone(), c.Clone()];

        _actionsHistory = new(65536);
        _eventsHistory = new(65536);
    }

    /// <summary>
    /// Commit current state of the simulation, releasing all previous snapshots,
    /// thus freeing memory and preventing rollbacks to before the current time.
    /// </summary>
    public void Commit()
    {
        _actionsHistory.RemoveRange(0, Current.ActionsCount);
        _eventsHistory.RemoveRange(0, Current.EventsCount);

        for (int i = 0; i < _snapshots.Length - 1; i++)
            _snapshots[i].CopyFrom(Current);
    }

    /// <summary>
    /// Emit events to initialize the game stage.
    /// </summary>
    public void Initialize()
    {
        // TODO
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
        long currentTick = Current.Tick;
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
        if (tick < Current.Tick)
        {
            if (tick < Initial.Tick)
                throw new ArgumentOutOfRangeException(
                    nameof(tick),
                    "Unable to rollback to before the start of the game."
                );

            for (int i = _snapshots.Length - 1; i >= 0; i--)
            {
                // Look for the most recent snapshot before or at the requested tick.
                Snapshot snapshot = _snapshots[i];
                if (snapshot.Tick >= tick)
                    continue;

                // Rollback all the snapshots that follow it, including the current state.
                for (int j = i + 1; j < _snapshots.Length; j++)
                    _snapshots[j].CopyFrom(snapshot);

                break;
            }
        }

        // Fast-forward to the requested tick.
        while (Current.Tick < tick)
            TickClock();
    }

    /// <summary>
    /// Advance the clock by one tick.
    /// </summary>
    public void TickClock()
    {
        // Step into the next tick.
        Current.Tick++;

        // Process actions.
        ProcessActions(this, ref Current, _actionsHistory);

        // TODO: move fleets.

        // TODO: resolve conflicts.

        // Produce resources on all celestial bodies that have factories.
        ProcessStructures(in _rules, in Current);

        ProcessBuildQueue(in _rules, in Current);

        // Take a "near" snapshot if enough time has passed since last one.
        RollSnapshots(NearSnapshotTicks, ref PrevNearSnapshot, ref NextNearSnapshot, ref Current);

        // Take a "far" snapshot if enough time has passed since last one.
        RollSnapshots(FarSnapshotTicks, ref PrevFarSnapshot, ref NextFarSnapshot, ref Current);
    }

    void ProcessAction(GameAction action)
    {
        switch (action)
        {
            case BuildStructureAction act:
                Current.CelestialBodies[act.CelestialBody].BuildQueue.Add(new(act.Structure));
                break;

            default:
                throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Process all actions scheduled for the current tick.
    /// </summary>
    /// <param name="self"></param>
    /// <param name="current"></param>
    /// <param name="history"></param>
    static void ProcessActions(
        GameSimulation<TData> self,
        ref Snapshot current,
        List<GameAction> history
    )
    {
        for (; current.ActionsCount < history.Count; current.ActionsCount++)
        {
            self.ProcessAction(history[current.ActionsCount]);

            // TODO: validate past events and revert the ones that are no longer valid.
        }
    }

    /// <summary>
    /// Process all active structures.
    /// </summary>
    /// <param name="rules"></param>
    /// <param name="current"></param>
    static void ProcessStructures(in RulesCache rules, in Snapshot current)
    {
        Span<int> activeStructures = stackalloc int[rules.Rules.Structures.Length];

        // Iterate all celestial bodies. The order does not matter, each
        // celestial body is isolated from the others.
        foreach (CelestialBody body in current.CelestialBodies)
        {
            // For each celestial body, iterate all structure types at random.
            // Iterating randomly is necessary so that even in case of resource
            // starvation, all structure types have equal chance to activate.
            foreach (ref Structure structure in current.Random.Shuffled(body.Structures.Span))
            {
                // Check how many active structures have enough resources to run.
                int enoughResourcesForActiveCount = structure.ActiveCount;
                foreach (ResourceCountCache resource in structure.TypeData.ActiveCost)
                {
                    int max = (int)(body.Resources[resource.Index].Count / resource.Data.Count);
                    if (max < enoughResourcesForActiveCount)
                        enoughResourcesForActiveCount = max;

                    // TODO: take storage limits into consideration.
                }

                // Use up resources.
                foreach (ResourceCountCache resource in structure.TypeData.ActiveCost)
                    body.Resources[resource.Index].Count -=
                        resource.Data.Count * enoughResourcesForActiveCount;

                // Increase structure products.
                foreach (ResourceCountCache resource in structure.TypeData.Produces)
                    body.Resources[resource.Index].Count +=
                        resource.Data.Count
                        * enoughResourcesForActiveCount
                        * body.Resources[resource.Index].Configuration.ProductionMultiplier;
            }
        }
    }

    /// <summary>
    /// Process build queues.
    /// </summary>
    /// <param name="rules"></param>
    /// <param name="current"></param>
    static void ProcessBuildQueue(in RulesCache rules, in Snapshot current)
    {
        // Iterate all celestial bodies. The order does not matter, each
        // celestial body is isolated from the others.
        foreach (CelestialBody body in current.CelestialBodies)
        {
            if (body.BuildQueue.Count is 0)
                continue;

            for (int i = 0; i < body.BuildQueue.Count; i++)
            {
                BuildQueueSlot build = body.BuildQueue[i];

                StructureRuleCache structure = rules.Structures[
                    rules.StructureTypeToIndex[build.Type].Index
                ];

                if (structure.BuildCost is not { } cost)
                {
                    body.BuildQueue.RemoveAt(i--);
                    continue;
                }

                bool canBuild = true;
                foreach (ResourceCountCache resource in cost)
                {
                    if (body.Resources[resource.Index].Count < resource.CostPerTick)
                    {
                        canBuild = false;
                        break;
                    }
                }

                if (!canBuild)
                    break;

                foreach (ResourceCountCache resource in cost)
                    body.Resources[resource.Index].Count -= resource.CostPerTick;

                if (++build.Progress >= structure.Rules.BuildTicks)
                {
                    body.Structures[structure.Index].Count++;
                    body.Structures[structure.Index].ActiveCount++;

                    body.BuildQueue.RemoveAt(i);
                    break;
                }

                body.BuildQueue[i] = build;
                break;
            }
        }
    }

    static void RollSnapshots(
        long ticksInterval,
        ref Snapshot prev,
        ref Snapshot next,
        ref Snapshot current
    )
    {
        if (current.Tick - next.Tick <= ticksInterval)
            return;

        (prev, next, current) = (next, current, prev);
        current.CopyFrom(next);
    }
}
