// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: AGPL-3.0-only

using SpaceCapture.Shared.Logic.Simulation;

namespace SpaceCapture.Shared.Logic.Events;

/// <summary>
/// Interface to hook to events in the game simulation.
/// </summary>
/// <typeparam name="TData">Data associated to objects and actors in the simulation.</typeparam>
public abstract class GameEventListener<TData> : IDisposable
{
    readonly GameSimulation<TData> _game;
    readonly Action<GameEvent> _listener;

    bool _disposed;

    public GameEventListener(GameSimulation<TData> game)
    {
        _game = game;
        _listener = ProcessEvent;

        _game.Event += _listener;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
                _game.Event -= _listener;

            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void ProcessEvent(GameEvent ev)
    {
        switch (ev)
        {
            #region Add

            case PlayerAddEvent evt:
                AddPlayer(ref _game.Players[new() { Index = evt.Index }]);
                break;

            case CelestialBodyAddEvent evt:
                AddCelestialBody(ref _game.CelestialBodies[new() { Index = evt.Index }]);
                break;

            #endregion

            #region Update

            case StageUpdateEvent evt:
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

            case PlayerRemoveEvent evt:
                // TODO: RemovePlayer(TODO);
                break;

            case CelestialBodyRemoveEvent evt:
                // TODO: RemoveCelestialBody(TODO);
                break;

            #endregion

            default:
                throw new NotImplementedException($"Unknown game action: {ev.GetType().FullName}");
        }
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

    /// <summary>
    /// A player left the game.
    /// </summary>
    /// <param name="player"></param>
    public abstract void RemovePlayer(ref readonly GameSimulation<TData>.Player player);

    /// <summary>
    /// A celestial body is no longer part of the game.
    /// </summary>
    /// <param name="body"></param>
    public abstract void RemoveCelestialBody(ref readonly GameSimulation<TData>.CelestialBody body);

    #endregion
}
