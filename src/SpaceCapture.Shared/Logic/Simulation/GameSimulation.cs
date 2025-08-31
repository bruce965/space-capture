// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: AGPL-3.0-only

using System.Buffers;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using SpaceCapture.Shared.Logic.Actions;
using SpaceCapture.Shared.Logic.Events;
using SpaceCapture.Shared.Logic.Stage;
using SpaceCapture.Shared.Types;
using SpaceCapture.Shared.Utilities;

namespace SpaceCapture.Shared.Logic.Simulation;

/// <inheritdoc cref="GameSimulation{TData}"/>
public class GameSimulation(GameStage stage) : GameSimulation<Empty>(stage);

/// <summary>
/// Executes all the game logic.
/// </summary>
/// <typeparam name="TData">Data associated to objects and actors in this simulation.</typeparam>
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
    /// Actions that have been executed or planned in the game simulation.
    /// </summary>
    readonly List<GameAction> _actionsHistory;

    /// <summary>
    /// <see cref="Snapshot.ActionsCount"/> of the first item in <see cref="_actionsHistory"/>.
    /// </summary>
    int _actionsCountStart;

    /// <summary>
    /// Initialize a new simulation from the specified state.
    /// </summary>
    /// <param name="state"></param>
    public GameSimulation(GameStage stage)
    {
        _stage = stage;
        _rules = new(_stage.Rules);

        Snapshot c = new(this);
        _snapshots = [c, c.Clone(), c.Clone(), c.Clone(), c.Clone(), c.Clone()];

        _actionsHistory = new(65536);
        _actionsCountStart = 0;
    }

    /// <summary>
    /// Commit current state of the simulation, releasing all previous snapshots,
    /// thus freeing memory and preventing rollbacks to before the current time.
    /// </summary>
    public void Commit()
    {
        _actionsHistory.RemoveRange(0, Current.ActionsCount - _actionsCountStart);
        _actionsCountStart = Current.ActionsCount;

        CommitEvents();

        for (int i = 0; i < _snapshots.Length - 1; i++)
            _snapshots[i].CopyFrom(Current);
    }

    /// <summary>
    /// Emit events to initialize an external game UI with all object and actors
    /// currently in the game simulation.
    /// </summary>
    public void Initialize()
    {
        Event?.Invoke(new StageUpdateEvent());

        foreach (ref Player player in Players)
            Event?.Invoke(new PlayerAddEvent(player.Index.Index));

        foreach (ref CelestialBody body in CelestialBodies)
            Event?.Invoke(new CelestialBodyAddEvent(body.Index.Index));
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

        // If this action is in the past, rollback time.
        if (action.Tick < currentTick)
            MoveClock(action.Tick, forceDiscardFutureEvents: false);

        // Insert this action in the right spot in the timeline.
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

        // If this action was in the past, return to present time.
        if (action.Tick < currentTick)
            MoveClock(currentTick, forceDiscardFutureEvents: false);
    }

    /// <summary>
    /// Move the clock back or forward to the specified tick.
    /// </summary>
    /// <remarks>
    /// Moving the clock backwards may emit counter-events that cancel all
    /// events that would have happened after <paramref name="tick"/>.
    /// </remarks>
    /// <param name="tick"></param>
    /// <param name="forceDiscardFutureEvents"><inheritdoc cref="EmitPendingEvents" path="/param[@name='forceDiscardFutureEvents']"/></param>
    /// <exception cref="ArgumentOutOfRangeException">Unable to rollback to before last commit or the start of the game.</exception>
    public void MoveClock(long tick, bool forceDiscardFutureEvents)
    {
        // Ensure there are no pending events at or after the target tick.
        Debug.Assert(_pendingEvents.Count is 0 || _pendingEvents[^1].Tick < tick);

        // The requested tick is in the past, a rollback is necessary.
        if (tick < Current.Tick)
        {
            if (tick < Initial.Tick)
                throw new ArgumentOutOfRangeException(
                    nameof(tick),
                    "Unable to rollback to before last commit or the start of the game."
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

        Debug.Assert(_pendingEvents.Count is 0);

        if (forceDiscardFutureEvents)
            EmitPendingEvents(forceDiscardFutureEvents: true);
    }

    /// <summary>
    /// Advance the clock by one tick.
    /// </summary>
    public void TickClock()
    {
        // Step into the next tick.
        Current.Tick++;

        // Mark all entities as not "updated during last tick".
        ResetUpdateFlags();

        // Process actions.
        ProcessActions(in _rules, ref Current, _actionsHistory);

        // TODO: move fleets.

        // TODO: resolve conflicts.

        // Produce resources on all celestial bodies that have factories.
        ProcessStructures(in _rules, ref Current);

        // Repair damaged structures and process build queues.
        ProcessBuildQueue(in _rules, ref Current);

        // Take a "near" snapshot if enough time has passed since last one.
        ShiftSnapshots(NearSnapshotTicks, ref PrevNearSnapshot, ref NextNearSnapshot, ref Current);

        // Take a "far" snapshot if enough time has passed since last one.
        ShiftSnapshots(FarSnapshotTicks, ref PrevFarSnapshot, ref NextFarSnapshot, ref Current);

        // Generate an update event for each entity that was "updated during last tick".
        AddUpdateFlagEvents();

        // Emit events produced during this tick.
        EmitPendingEvents(forceDiscardFutureEvents: false);
    }

    /// <summary>
    /// Reset <see cref="CelestialBody.UpdatedDuringLastTick"/>.
    /// </summary>
    /// <param name="current"></param>
    void ResetUpdateFlags()
    {
        foreach (ref CelestialBody body in Current.CelestialBodies)
            body.UpdatedDuringLastTick = false;
    }

    /// <summary>
    /// Add a pending update event for each <see cref="CelestialBody.UpdatedDuringLastTick"/>.
    /// </summary>
    /// <param name="current"></param>
    void AddUpdateFlagEvents()
    {
        foreach (ref CelestialBody body in Current.CelestialBodies)
            if (body.UpdatedDuringLastTick)
                AddPendingEvent(new CelestialBodyUpdateEvent(body.Index.Index));
    }

    /// <summary>
    /// Process all actions scheduled for the current tick.
    /// </summary>
    /// <param name="rules"></param>
    /// <param name="current"></param>
    /// <param name="changedCelestialBodies"></param>
    /// <param name="history"></param>
    static void ProcessActions(in RulesCache rules, ref Snapshot current, List<GameAction> history)
    {
        for (; current.ActionsCount < history.Count; current.ActionsCount++)
            ProcessAction(in rules, ref current, history[current.ActionsCount]);
    }

    /// <summary>
    /// Process a single action.
    /// </summary>
    /// <param name="rules"></param>
    /// <param name="current"></param>
    /// <param name="action"></param>
    static void ProcessAction(in RulesCache rules, ref Snapshot current, GameAction action)
    {
        switch (action)
        {
            case ActivateStructureAction act:
                _ = TryActivateStructure(in rules, ref current.CelestialBodies[act.CelestialBody], act.Structure);
                break;

            case DeactivateStructureAction act:
                _ = TryDeactivateStructure(in rules, ref current.CelestialBodies[act.CelestialBody], act.Structure);
                break;

            case BuildStructureAction act:
                ref CelestialBody b1 = ref current.CelestialBodies[act.CelestialBody];
                b1.BuildQueue.Add(new(act.Structure));
                b1.UpdatedDuringLastTick = true;
                break;

            case ToggleRepairStructureAction act:
                ref CelestialBody b2 = ref current.CelestialBodies[act.CelestialBody];
                ref Structure s1 = ref b2.Structures[act.Structure];
                if (s1.RepairDamaged != act.Enabled)
                {
                    s1.RepairDamaged = act.Enabled;
                    b2.UpdatedDuringLastTick = true;
                }
                break;

            default:
                throw new NotImplementedException($"Unknown game action: {action.GetType().FullName}");
        }
    }

    /// <summary>
    /// Produce resources on all celestial bodies that have factories.
    /// </summary>
    /// <param name="rules"></param>
    /// <param name="current"></param>
    static void ProcessStructures(in RulesCache rules, ref Snapshot current)
    {
        Span<int> activeStructures = stackalloc int[rules.Rules.Structures.Length];

        // Iterate all celestial bodies. The order does not matter, each
        // celestial body is isolated from the others.
        foreach (ref CelestialBody body in current.CelestialBodies)
        {
            // For each celestial body, iterate all structure types at random.
            // Iterating randomly is necessary so that even in case of resource
            // starvation, all structure types have equal chance to activate.
            foreach (ref Structure structure in current.Random.Shuffled(body.Structures.Span))
            {
                int enoughResourcesFor = structure.ActiveCount;
                if (enoughResourcesFor is 0)
                    continue;

                // Check how many active structures have enough resources to run.
                foreach (ResourceCountCache resource in structure.TypeData.ActiveCost)
                {
                    int max = (int)(body.Resources[resource.Index].Count / resource.CountPerTick);
                    enoughResourcesFor = Math.Min(enoughResourcesFor, max);
                }

                if (enoughResourcesFor is 0)
                    continue;

                // Ensure that there is enough space on the planet to store the products.
                foreach (ResourceCountCache product in structure.TypeData.Produces)
                {
                    ref Resource r = ref body.Resources[product.Index];

                    if (r.SoftLimit is not { } limit)
                        continue;

                    int max = (int)FP48D16.Ceiling((limit - r.Count) / product.CountPerTick);
                    enoughResourcesFor = Math.Clamp(max, 0, enoughResourcesFor);
                }

                if (enoughResourcesFor is 0)
                    continue;

                body.UpdatedDuringLastTick = true;

                // Use up resources.
                foreach (ResourceCountCache cost in structure.TypeData.ActiveCost)
                    body.Resources[cost.Index].Count -= cost.CountPerTick * enoughResourcesFor;

                // Increase structure products.
                foreach (ResourceCountCache product in structure.TypeData.Produces)
                    body.Resources[product.Index].Count +=
                        product.CountPerTick
                        * enoughResourcesFor
                        * body.Resources[product.Index].Configuration.ProductionMultiplier;
            }
        }
    }

    /// <summary>
    /// Repair damaged structures and process build queues.
    /// </summary>
    /// <param name="rules"></param>
    /// <param name="current"></param>
    static void ProcessBuildQueue(in RulesCache rules, ref Snapshot current)
    {
        // Iterate all celestial bodies. The order does not matter, each
        // celestial body is isolated from the others.
        foreach (ref CelestialBody body in current.CelestialBodies)
        {
            // Repair damaged structures in random order, one per-tick.
            bool somethingHasBeenRepaired = false;
            foreach (ref Structure structure in current.Random.Shuffled(body.Structures.Span))
            {
                if (!structure.RepairDamaged || structure.Damage == 0)
                    continue;

                // Some structures cannot be repaired, in which case they are simply skipped.
                if (structure.TypeData.RepairCost is not { } repairCost)
                    continue;

                // Make sure that there are enough resources to process one repair tick for this structure.
                if (!TryTakeResources(ref body, repairCost))
                    break;

                // Repair.
                structure.Damage = FP48D16.Max(0, structure.Damage - structure.TypeData.RepairedDamagePerTick);

                body.UpdatedDuringLastTick = true;

                somethingHasBeenRepaired = true;
                break;
            }

            // Either repair or build something on each tick, but not both.
            if (somethingHasBeenRepaired)
                continue;

            // Build the first structure in the queue.
            for (int i = 0; i < body.BuildQueue.Count; i++)
            {
                BuildQueueSlot build = body.BuildQueue[i];

                StructureRuleCache structureRule = rules.Structures[build.Type];

                // Some structures cannot be built, in which case they are simply removed from the build queue.
                if (structureRule.BuildCost is not { } buildCost)
                {
                    body.UpdatedDuringLastTick = true;
                    body.BuildQueue.RemoveAt(i--);
                    continue;
                }

                // Make sure that there are enough resources to process one build tick for this structure.
                if (!TryTakeResources(ref body, buildCost))
                    break;

                body.UpdatedDuringLastTick = true;

                // Increase the build progress and check if the build process is complete.
                if (++build.Progress >= structureRule.Rules.BuildTicks)
                {
                    ref Structure structure = ref body.Structures[structureRule.Index];

                    structure.Count++;

                    // If all structures of this type are active, try activating the new one right away.
                    if (structure.ActiveCount == structure.Count - 1)
                        TryActivateStructure(in rules, ref body, ref structure);

                    body.BuildQueue.RemoveAt(i);
                    break;
                }

                body.BuildQueue[i] = build;
                break;
            }
        }
    }

    /// <summary>
    /// Try to take some resources from a celestial body.
    /// </summary>
    /// <param name="body"></param>
    /// <param name="resources"></param>
    /// <returns></returns>
    static bool TryTakeResources(ref CelestialBody body, ImmutableArray<ResourceCountCache> resources)
    {
        // Ensure that there are enough resources available.
        foreach (ResourceCountCache resource in resources)
            if (body.Resources[resource.Index].Count < resource.CountPerTick)
                return false;

        // Take the necessary resources.
        foreach (ResourceCountCache resource in resources)
            body.Resources[resource.Index].Count -= resource.CountPerTick;

        return true;
    }

    /// <summary>
    /// Add resources to a celestial body.
    /// </summary>
    /// <param name="body"></param>
    /// <param name="resources"></param>
    static void AddResources(ref CelestialBody body, ImmutableArray<ResourceCountCache> resources)
    {
        // Add the specified resources, without exceeding the limit.
        foreach (ResourceCountCache resource in resources)
        {
            ref Resource r = ref body.Resources[resource.Index];

            FP48D16 initialCount = r.Count;

            r.Count += resource.Data.Count;

            if (r.Configuration.HardLimit is { } limit && r.Count > limit)
                r.Count = limit;

            if (r.Count != initialCount)
                body.UpdatedDuringLastTick = true;
        }
    }

    /// <summary>
    /// Try activating a structure if it exists and the necessary resources are available.
    /// </summary>
    /// <param name="rules"></param>
    /// <param name="body"></param>
    /// <param name="structure"></param>
    /// <returns></returns>
    static bool TryActivateStructure(in RulesCache rules, ref CelestialBody body, StructureType structure) =>
        TryActivateStructure(in rules, ref body, ref body.Structures[structure]);

    /// <inheritdoc cref="TryActivateStructure(in RulesCache, ref CelestialBody, StructureType)"/>
    static bool TryActivateStructure(in RulesCache rules, ref CelestialBody body, ref Structure structure)
    {
        Debug.Assert(Unsafe.AreSame(ref structure, ref body.Structures[structure.TypeData.Index]));

        if (
            structure.Count <= structure.ActiveCount
            || !TryTakeResources(ref body, rules.Structures[structure.TypeData.Index].ActivationCost)
        )
            return false;

        ref readonly StructureRuleCache rule = ref rules.Structures[structure.TypeData.Index];

        structure.ActiveCount++;

        foreach (ResourceCountCache storedResource in rule.Stores)
        {
            ref Resource r = ref body.Resources[storedResource.Index];
            r.SoftLimit += storedResource.Data.Count;
            r.HardLimit += storedResource.Data.Count;

            if (r.HardLimit is { } limit)
                r.Count = FP48D16.Min(r.Count, limit);
        }

        body.UpdatedDuringLastTick = true;

        return true;
    }

    /// <summary>
    /// Try deactivating a structure if it exists.
    /// </summary>
    /// <param name="rules"></param>
    /// <param name="body"></param>
    /// <param name="structure"></param>
    /// <returns></returns>
    static bool TryDeactivateStructure(in RulesCache rules, ref CelestialBody body, StructureType structure) =>
        TryDeactivateStructure(in rules, ref body, ref body.Structures[structure]);

    /// <inheritdoc cref="TryActivateStructure(in RulesCache, ref CelestialBody, StructureType)"/>
    static bool TryDeactivateStructure(in RulesCache rules, ref CelestialBody body, ref Structure structure)
    {
        Debug.Assert(Unsafe.AreSame(ref structure, ref body.Structures[structure.TypeData.Index]));

        if (structure.ActiveCount <= 0)
            return false;

        ref readonly StructureRuleCache rule = ref rules.Structures[structure.TypeData.Index];

        structure.ActiveCount--;

        AddResources(ref body, rule.ActivationCost);

        foreach (ResourceCountCache storedResource in rule.Stores)
        {
            ref Resource r = ref body.Resources[storedResource.Index];
            r.SoftLimit -= storedResource.Data.Count;
            r.HardLimit -= storedResource.Data.Count;

            if (r.HardLimit is { } limit)
                r.Count = FP48D16.Min(r.Count, limit);
        }

        body.UpdatedDuringLastTick = true;

        return true;
    }

    /// <summary>
    /// Shift snapshots down by one position, and make <paramref name="current"/> a new snapshot.
    /// </summary>
    /// <param name="ticksInterval">How many ticks since last snapshot to wait before shifting.</param>
    /// <param name="prev">Becomes <paramref name="next"/>.</param>
    /// <param name="next">Becomes <paramref name="current"/>.</param>
    /// <param name="current">Becomes a new copy of <paramref name="current"/>.</param>
    static void ShiftSnapshots(long ticksInterval, ref Snapshot prev, ref Snapshot next, ref Snapshot current)
    {
        if (current.Tick - next.Tick <= ticksInterval)
            return;

        (prev, next, current) = (next, current, prev);
        current.CopyFrom(next);
    }
}
