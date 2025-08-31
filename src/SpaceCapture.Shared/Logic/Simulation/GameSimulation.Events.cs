// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: AGPL-3.0-only

using System.Diagnostics;
using SpaceCapture.Shared.Logic.Events;
using SpaceCapture.Shared.Utilities;

namespace SpaceCapture.Shared.Logic.Simulation;

partial class GameSimulation<TData>
{
    record struct GameEventData(long Tick, GameEvent Event);

    /// <summary>
    /// Something happened in the game. This might also be a counter-event that
    /// reverts en event that was emitted previously, for instance when
    /// executing an action in the past makes another later action invalid thus
    /// making the related future event never happen at all.
    /// </summary>
    public event GameEventHandler? Event;

    /// <summary>
    /// Events that occurred in the game simulation, not emitted yet.
    /// </summary>
    readonly List<GameEventData> _pendingEvents = [];

    /// <summary>
    /// Events that have been emitted, excluding events reverted by counter-events.
    /// If a rollback occurred, this list may include already-emitted events that
    /// may happen in the future and haven't been reverted by counter-events yet.
    /// </summary>
    readonly List<GameEventData> _emittedEventsHistory = new(65536);

    /// <summary>
    /// <see cref="Snapshot.EventsCount"/> of the first item in <see cref="_emittedEventsHistory"/>.
    /// </summary>
    int _eventsCountStart = 0;

    /// <summary>
    /// See <see cref="Commit"/>, this method commits past events.
    /// </summary>
    void CommitEvents()
    {
        _emittedEventsHistory.RemoveRange(0, Current.EventsCount - _eventsCountStart);
        _eventsCountStart = Current.EventsCount;
    }

    /// <summary>
    /// Emit events up to the current tick if they haven't been emitted yet,
    /// removing duplicates and producing counter-events for events that have
    /// already been emitted but are no longer valid due to changes in history.
    /// </summary>
    /// <param name="forceDiscardFutureEvents">
    /// Produce counter-events reverting all events that would have happened
    /// in the future but were rolled-back by moving the clock backwards or
    /// injecting actions in the past; even if this parameter is set to
    /// <see langword="false"/>, counter-events may be produced if history
    /// changed.
    /// </param>
    public void EmitPendingEvents(bool forceDiscardFutureEvents)
    {
        // First check all events that have already been emitted, up to next tick (not included).
        int indexOfNextTick = _emittedEventsHistory.FindIndex(Current.Tick, Current.EventsCount, (x, t) => x.Tick > t);
        if (indexOfNextTick is -1)
            indexOfNextTick = _emittedEventsHistory.Count;

        // Check that all already-emitted future events match present events.
        int iPresent,
            iEmitted;
        for (
            iPresent = 0, iEmitted = Current.EventsCount;
            iPresent < _pendingEvents.Count && iEmitted < indexOfNextTick - 1;
            iPresent++, iEmitted++
        )
        {
            // If this already-emitted future event doesn't match the event that
            // must be emitted at this time in the present, history has changed.
            if (_emittedEventsHistory[iEmitted] != _pendingEvents[iPresent])
                break;
        }

        // If a mismatch was detected between already-emitted future events
        // and the events that should be emitted in the present, we have to
        // invalidate all future events starting at the first mismatch in
        // chronological order. History has been rewritten!
        if (forceDiscardFutureEvents || iPresent != _pendingEvents.Count || iEmitted != Current.EventsCount)
            DiscardFutureEvents(iEmitted);

        // Emit new events.
        for (int i = iPresent; i < _pendingEvents.Count; i++)
        {
            Emit(_pendingEvents[i]);
            _emittedEventsHistory.Add(_pendingEvents[i]);
            Current.EventsCount++;
        }

        _pendingEvents.Clear();
    }

    void DiscardFutureEvents(int emittedEventsHistoryIndex)
    {
        // Revert all emitted events backwards starting at the specified index.
        for (int i = _emittedEventsHistory.Count - 1; i >= emittedEventsHistoryIndex; i--)
            Emit(_emittedEventsHistory[i], revert: true);

        _emittedEventsHistory.RemoveRange(
            emittedEventsHistoryIndex,
            _emittedEventsHistory.Count - emittedEventsHistoryIndex
        );
    }

    void AddPendingEvent(GameEvent ev) => _pendingEvents.Add(new(Current.Tick, ev));

    void Emit(GameEventData data, bool revert = false)
    {
        // Events should only be emitted in the present so that the consumer of
        // has a chance to read the state of the simulation at time of emission.
        // Unfortunately we lack this privilege when emitting counter-events.
        Debug.Assert(data.Tick == Current.Tick || revert);

        Event?.Invoke(data.Event, revert);
    }
}
