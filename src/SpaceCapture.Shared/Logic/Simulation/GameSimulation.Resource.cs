// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: AGPL-3.0-only

using System.Diagnostics;
using SpaceCapture.Shared.Abstractions;
using SpaceCapture.Shared.Logic.Stage;
using SpaceCapture.Shared.Types;

namespace SpaceCapture.Shared.Logic.Simulation;

partial class GameSimulation<TData>
{
    [DebuggerDisplay($"{{{nameof(TypeData)},nq}}")]
    public struct Resource(ResourceRuleCache rule, CelestialBodyResource configuration)
        : ICloneable<Resource>,
            ITransferable<Resource>
    {
        /// <summary>
        /// Data for this resource type.
        /// </summary>
        public readonly ResourceRuleCache TypeData => rule;

        /// <summary>
        /// Configuration for this resource type specific for a celestial body.
        /// </summary>
        public readonly CelestialBodyResource Configuration => configuration;

        /// <summary>
        /// Amount.
        /// </summary>
        public FP32D16 Count { get; set; }

        public readonly Resource Clone() => this;

        public void CopyFrom(Resource other) => this = other;
    }
}
