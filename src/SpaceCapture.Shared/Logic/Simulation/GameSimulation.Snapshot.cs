// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: AGPL-3.0-only

using SpaceCapture.Shared.Abstractions;
using SpaceCapture.Shared.Utilities;

namespace SpaceCapture.Shared.Logic.Simulation;

partial class GameSimulation<TData>
{
    /// <summary>
    /// Hot snapshot of a running game.
    /// </summary>
    /// <param name="game"></param>
    sealed class Snapshot(GameSimulation<TData> game) : ICloneable<Snapshot>, ITransferable<Snapshot>
    {
        public GameSimulation<TData> Game { get; private set; } = game;

        DeterministicRandom _rand;

        Player[]? _players = InitPlayers(game);

        CelestialBody[]? _celestialBodies = InitCelestialBodies(game);

        public long Tick { get; set; }

        /// <summary>
        /// Deterministic random number generator.
        /// </summary>
        public ref DeterministicRandom Random => ref _rand;

        /// <summary>
        /// How many actions have been performed since the beginning of the simulation.
        /// </summary>
        public int ActionsCount { get; set; }

        /// <summary>
        /// How many events have occurred since the beginning of the simulation.
        /// </summary>
        public int EventsCount { get; set; }

        public Span<Player> Players => _players;

        public Span<CelestialBody> CelestialBodies => _celestialBodies;

        static Player[] InitPlayers(GameSimulation<TData> game)
        {
            Player[] players = new Player[game._stage.PlayersCount];
            for (int i = 0; i < players.Length; i++)
                players[i] = new(new() { Index = i });

            return players;
        }

        static CelestialBody[] InitCelestialBodies(GameSimulation<TData> game)
        {
            CelestialBody[] celestialBodies = new CelestialBody[game._stage.CelestialBodies.Length];
            for (int i = 0; i < celestialBodies.Length; i++)
                celestialBodies[i] = new(game._rules, game._stage.CelestialBodies[i], new() { Index = i });

            return celestialBodies;
        }

        /// <inheritdoc/>
        public Snapshot Clone() =>
            new(Game)
            {
                Tick = Tick,
                _rand = _rand,
                _players = _players?.DeepClone(),
                _celestialBodies = _celestialBodies?.DeepClone(),
                ActionsCount = 0,
                EventsCount = 0,
            };

        /// <inheritdoc/>
        public void CopyFrom(Snapshot other)
        {
            Game = other.Game;
            Tick = other.Tick;
            _rand = other._rand;
            TransferHelper.Copy(ref _players, other._players);
            TransferHelper.Copy(ref _celestialBodies, other._celestialBodies);
            ActionsCount = other.ActionsCount;
            EventsCount = other.EventsCount;
        }
    }
}
