// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: AGPL-3.0-only

using System.Diagnostics.CodeAnalysis;
using SpaceCapture.Shared.Logic.Simulation;

namespace SpaceCapture.Shared.Logic.Events;

/// <summary>
/// Interface to hook to events in the game simulation.
/// </summary>
/// <typeparam name="TData">Data associated to objects and actors in the simulation.</typeparam>
public abstract class GameEventListener<TData> : IDisposable
{
    readonly GameSimulation<TData> _game;
    readonly GameEventHandler _handler;

    bool _disposed;

    public GameEventListener(GameSimulation<TData> game)
    {
        _game = game;
        _handler = HandleEvent;

        _game.Event += _handler;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
                _game.Event -= _handler;

            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void HandleEvent(GameEvent ev, bool revert = false)
    {
        switch (ev)
        {
            #region Add

            case PlayerAddEvent evt:
                ThrowNotSupportedIfReverting(evt, revert);
                AddPlayer(ref _game.Players[new() { Index = evt.Index }]);
                break;

            case CelestialBodyAddEvent evt:
                ThrowNotSupportedIfReverting(evt, revert);
                AddCelestialBody(ref _game.CelestialBodies[new() { Index = evt.Index }]);
                break;

            #endregion

            #region Update

            case StageUpdateEvent:
                UpdateStage(_game);
                break;

            case PlayerUpdateEvent evt:
                UpdatePlayer(ref _game.Players[new() { Index = evt.Index }]);
                break;

            case CelestialBodyUpdateEvent evt:
                UpdateCelestialBody(ref _game.CelestialBodies[new() { Index = evt.Index }]);
                break;

            #endregion

            #region Remove

            #endregion

            default:
                throw new NotImplementedException($"Unknown game event: {ev.GetType().FullName}");
        }
    }

    static void ThrowNotSupportedIfReverting(GameEvent ev, [DoesNotReturnIf(true)] bool revert)
    {
        if (revert)
            throw new NotSupportedException($"Game event does not support revert: {ev.GetType().FullName}");
    }

    #region Add

    /// <summary>
    /// A new player joined the game.
    /// </summary>
    /// <param name="player"></param>
    public abstract void AddPlayer(ref GameSimulation<TData>.Player player);

    /// <summary>
    /// A new celestial body is now part of the game.
    /// </summary>
    /// <param name="body"></param>
    public abstract void AddCelestialBody(ref GameSimulation<TData>.CelestialBody body);

    #endregion

    #region Update

    /// <summary>
    /// Something changed in the <see cref="GameSimulation{TData}.Stage"/>,
    /// the rules of the game might have changed.
    /// </summary>
    /// <param name="game"></param>
    public abstract void UpdateStage(GameSimulation<TData> game);

    /// <summary>
    /// Something about a player changed.
    /// </summary>
    /// <param name="player"></param>
    public abstract void UpdatePlayer(ref GameSimulation<TData>.Player player);

    /// <summary>
    /// Something about a celestial body changed.
    /// </summary>
    /// <param name="body"></param>
    public abstract void UpdateCelestialBody(ref GameSimulation<TData>.CelestialBody body);

    #endregion

    #region Remove

    #endregion
}
