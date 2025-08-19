// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: AGPL-3.0-only

using System.Diagnostics.CodeAnalysis;
using SpaceCapture.Shared.Abstractions;
using SpaceCapture.Shared.Logic.Rules;
using SpaceCapture.Shared.Utilities;

namespace SpaceCapture.Shared.Logic.Simulation;

partial class GameSimulation
{
    /// <summary>
    /// Snapshot of a game state.
    /// </summary>
    /// <param name="state"></param>
    sealed class Snapshot(GameSimulation game) : ICloneable<Snapshot>, ITransferable<Snapshot>
    {
        public GameSimulation Game { get; private set; } = game;

        DeterministicRandom _rand;

        CelestialBody[]? _celestialBodies;

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

        public Span<CelestialBody> CelestialBodies
        {
            get
            {
                if (_celestialBodies is null)
                {
                    _celestialBodies = new CelestialBody[Game._stage.CelestialBodies.Length];
                    for (int i = 0; i < _celestialBodies.Length; i++)
                        _celestialBodies[i] = new(Game._rules, Game._stage.CelestialBodies[i]);
                }

                return _celestialBodies;
            }
        }

        /// <inheritdoc/>
        public Snapshot Clone() =>
            new(Game)
            {
                Tick = Tick,
                _rand = _rand,
                _celestialBodies = _celestialBodies?.DeepClone(),
                ActionsCount = 0,
                EventsCount = 0,
            };

        public void CopyFrom(Snapshot other)
        {
            Game = other.Game;
            Tick = other.Tick;
            _rand = other._rand;
            TransferHelper.Copy(ref _celestialBodies, other._celestialBodies);
            ActionsCount = other.ActionsCount;
            EventsCount = other.EventsCount;
        }
    }

    //sealed class CelestialBodyResourceCache(CelestialBody body, ResourceRule rule, ResourceTypeIndex i)
    //    : ICloneable<CelestialBodyResourceCache>,
    //        ITransferable<CelestialBodyResourceCache>
    //{
    //    CelestialBody _body = body;
    //    ResourceRule _rule = rule;
    //    ResourceTypeIndex _i = i;

    //    List<ResourceCount>? _data;

    //    public ResourceRule Rule => _rule;

    //    public ResourceCount Data
    //    {
    //        get => _body.Data[_i]
    //        set => _data[_i] = value;
    //    }

    //    public CelestialBodyResourceCache Clone() => new(_body, _rule);

    //    public void CopyFrom(CelestialBodyResourceCache other)
    //    {
    //        _body = other._body;
    //        _rule = other._rule;
    //        _i = other._i;

    //        TransferHelper.CopyImmutable(ref _data, other._data);
    //    }
    //}

    //sealed class CelestialBodyStructureCache(CelestialBody cache, StructureRule rule)
    //    : ICloneable<CelestialBodyStructureCache>,
    //        ITransferable<CelestialBodyStructureCache>
    //{
    //    CelestialBody _cache = cache;
    //    StructureRule _rule = rule;
    //    int? _celestialBodyStructureIndex;

    //    List<StructureCount>? _data;

    //    public StructureRule Rule => _rule;

    //    public StructureCount Data
    //    {
    //        get
    //        {
    //            int i = InitAndGetIndex();
    //            return _data[i];
    //        }
    //        set
    //        {
    //            int i = InitAndGetIndex();
    //            _data[i] = value;
    //        }
    //    }

    //    [MemberNotNull(nameof(_data))]
    //    int InitAndGetIndex()
    //    {
    //        if (_data is not { } d)
    //        {
    //            d = _cache.Data.Structures;
    //            _data = d;
    //        }

    //        if (_celestialBodyStructureIndex is not { } i)
    //        {
    //            for (i = 0; i < d.Count; i++)
    //                if (d[i].Type == _rule.Type)
    //                    break;

    //            if (i == d.Count)
    //                d.Add(new(_rule.Type, 0));

    //            _celestialBodyStructureIndex = i;
    //        }

    //        return i;
    //    }

    //    public CelestialBodyStructureCache Clone() => new(_cache, _rule);

    //    public void CopyFrom(CelestialBodyStructureCache other)
    //    {
    //        _cache = other._cache;
    //        _rule = other._rule;
    //        _celestialBodyStructureIndex = other._celestialBodyStructureIndex;

    //        TransferHelper.CopyImmutable(ref _data, other._data);
    //    }
    //}
}
