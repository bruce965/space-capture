// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: AGPL-3.0-only

using System.Diagnostics.CodeAnalysis;
using SpaceCapture.Shared.Abstractions;
using SpaceCapture.Shared.Logic.Stage;
using SpaceCapture.Shared.Utilities;

namespace SpaceCapture.Shared.Logic.Simulation;

partial class GameSimulation<TData>
{
    public struct CelestialBody(
        GameSimulation<TData> game,
        CelestialBodyConfiguration configuration,
        CelestialBodyIndex index
    ) : ICloneable<CelestialBody>, ITransferable<CelestialBody>
    {
        GameSimulation<TData> _game = game;
        CelestialBodyConfiguration _configuration = configuration;
        CelestialBodyIndex _index = index;
        PlayerIndex? _player;
        Resource[] _resources = game._rules.Resources.Span.ToArray(r => new Resource(
            r,
            configuration.Resources.FirstOrDefault(c => c.Type == r.Rules.Type, new(r.Rules.Type))
        ));
        Structure[] _structures = game._rules.Structures.Span.ToArray(s => new Structure(s));
        TData _data = default!;
        List<BuildQueueSlot>? _buildQueue;
        bool _updatedDuringLastTick;

        /// <summary>
        /// Custom data attached to this celestial body.
        /// </summary>
        [MaybeNull]
        public TData Data
        {
            readonly get => _data;
            set => _data = value;
        }

        public readonly GameSimulation<TData> Game => _game;

        public readonly CelestialBodyConfiguration Configuration => _configuration;

        public readonly CelestialBodyIndex Index => _index;

        /// <summary>
        /// Index of the player that currently owns this celestial body.
        /// </summary>
        public PlayerIndex? Player
        {
            readonly get => _player;
            set => _player = value;
        }

        /// <summary>
        /// Resources on this celestial body.
        /// </summary>
        public readonly Accessor<Resource, ResourceType, ResourceTypeIndex> Resources =>
            new(_resources, _game._rules._resourceTypeToIndex);

        /// <summary>
        /// Structures on this celestial body.
        /// </summary>
        public readonly Accessor<Structure, StructureType, StructureTypeIndex> Structures =>
            new(_structures, _game._rules._structureTypeToIndex);

        /// <summary>
        /// Build queue for structures.
        /// </summary>
        public List<BuildQueueSlot> BuildQueue => _buildQueue ??= [];

        /// <summary>
        /// Whether this celestial body was updated during last tick.
        /// </summary>
        public bool UpdatedDuringLastTick
        {
            readonly get => _updatedDuringLastTick;
            set => _updatedDuringLastTick = value;
        }

        /// <inheritdoc/>
        public readonly CelestialBody Clone() =>
            new()
            {
                _game = _game,
                _configuration = _configuration,
                _index = _index,
                _player = _player,
                _resources = _resources.DeepClone(),
                _structures = _structures.DeepClone(),
                _data = _data,
                _buildQueue = _buildQueue?.DeepClone(),
                _updatedDuringLastTick = _updatedDuringLastTick,
            };

        public void CopyFrom(CelestialBody other)
        {
            _game = other._game;
            _configuration = other._configuration;
            _index = other._index;
            _player = other._player;
            TransferHelper.Copy(ref _resources, other._resources);
            TransferHelper.Copy(ref _structures, other._structures);
            _data = other._data;
            TransferHelper.Copy(ref _buildQueue, other._buildQueue);
            _updatedDuringLastTick = other._updatedDuringLastTick;
        }
    }
}
