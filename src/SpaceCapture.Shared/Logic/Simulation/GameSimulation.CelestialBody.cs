// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: AGPL-3.0-only

using SpaceCapture.Shared.Abstractions;
using SpaceCapture.Shared.Logic.Stage;
using SpaceCapture.Shared.Utilities;

namespace SpaceCapture.Shared.Logic.Simulation;

partial class GameSimulation
{
    struct CelestialBody(RulesCache rules, CelestialBodyConfiguration configuration)
        : ICloneable<CelestialBody>,
            ITransferable<CelestialBody>
    {
        CelestialBodyConfiguration _configuration = configuration;
        PlayerIndex? _player;
        Resource[] _resources = rules.Resources.ToArray(r => new Resource(
            r,
            configuration.Resources.FirstOrDefault(c => c.Type == r.Rules.Type, new(r.Rules.Type))
        ));
        Structure[] _structures = rules.Structures.ToArray(s => new Structure(s));

        public readonly CelestialBodyConfiguration Configuration => _configuration;

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
        public readonly Span<Resource> Resources => _resources;

        /// <summary>
        /// Structures on this celestial body.
        /// </summary>
        public readonly Span<Structure> Structures => _structures;

        /// <inheritdoc/>
        public readonly CelestialBody Clone() =>
            new()
            {
                _configuration = _configuration,
                _player = _player,
                _resources = _resources.DeepClone(),
                _structures = _structures.DeepClone(),
            };

        public void CopyFrom(CelestialBody other)
        {
            _configuration = other._configuration;
            _player = other._player;
            TransferHelper.Copy(ref _resources, other._resources);
            TransferHelper.Copy(ref _structures, other._structures);
        }
    }
}
