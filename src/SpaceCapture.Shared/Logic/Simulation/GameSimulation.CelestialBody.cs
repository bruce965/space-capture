// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: AGPL-3.0-only

using System.Diagnostics.CodeAnalysis;
using SpaceCapture.Shared.Abstractions;
using SpaceCapture.Shared.Logic.Stage;
using SpaceCapture.Shared.Utilities;

namespace SpaceCapture.Shared.Logic.Simulation;

partial class GameSimulation<TData>
{
    public struct CelestialBody(RulesCache rules, CelestialBodyConfiguration configuration, CelestialBodyIndex index)
        : ICloneable<CelestialBody>,
            ITransferable<CelestialBody>
    {
        RulesCache _rules = rules;
        CelestialBodyConfiguration _configuration = configuration;
        CelestialBodyIndex _index = index;
        PlayerIndex? _player;
        Resource[] _resources = rules.Resources.Span.ToArray(r => new Resource(
            r,
            configuration.Resources.FirstOrDefault(c => c.Type == r.Rules.Type, new(r.Rules.Type))
        ));
        Structure[] _structures = rules.Structures.Span.ToArray(s => new Structure(s));
        TData _data = default!;
        List<BuildQueueSlot>? _buildQueue;

        /// <summary>
        /// Custom data attached to this celestial body.
        /// </summary>
        [MaybeNull]
        public TData Data
        {
            readonly get => _data;
            set => _data = value;
        }

        public readonly CelestialBodyConfiguration Configuration => _configuration;

        public CelestialBodyIndex Index => _index;

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
            new(_resources, _rules.ResourceTypeToIndex);

        /// <summary>
        /// Structures on this celestial body.
        /// </summary>
        public readonly Accessor<Structure, StructureType, StructureTypeIndex> Structures =>
            new(_structures, _rules.StructureTypeToIndex);

        /// <summary>
        /// Build queue for structures.
        /// </summary>
        public List<BuildQueueSlot> BuildQueue => _buildQueue ??= [];

        /// <inheritdoc/>
        public readonly CelestialBody Clone() =>
            new()
            {
                _rules = _rules,
                _configuration = _configuration,
                _index = _index,
                _player = _player,
                _resources = _resources.DeepClone(),
                _structures = _structures.DeepClone(),
                _data = _data,
                _buildQueue = _buildQueue?.DeepClone(),
            };

        public void CopyFrom(CelestialBody other)
        {
            _rules = other._rules;
            _configuration = other._configuration;
            _index = other._index;
            _player = other._player;
            TransferHelper.Copy(ref _resources, other._resources);
            TransferHelper.Copy(ref _structures, other._structures);
            _data = other._data;
            TransferHelper.Copy(ref _buildQueue, other._buildQueue);
        }
    }
}
