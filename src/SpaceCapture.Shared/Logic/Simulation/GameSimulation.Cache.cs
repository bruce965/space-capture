// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: AGPL-3.0-only

using System.Collections.Immutable;
using System.Diagnostics;
using SpaceCapture.Shared.Abstractions;
using SpaceCapture.Shared.Logic.Configuration;
using SpaceCapture.Shared.Logic.State;
using SpaceCapture.Shared.Utilities;

namespace SpaceCapture.Shared.Logic.Simulation;

partial class GameSimulation
{
    /// <summary>
    /// Representation of the current game state, in form that takes more memory but can be accessed faster.
    /// </summary>
    /// <param name="state"></param>
    sealed class Cache(GameState state) : ICloneable<Cache>, ITransferable<Cache>
    {
        GameState _state = state;
        CelestialBodyCache[]? _celestialBodies;

        /// <summary>
        /// How many actions have been performed since the beginning of the simulation.
        /// </summary>
        public int ActionsCount { get; set; }

        /// <summary>
        /// How many events have occurred since the beginning of the simulation.
        /// </summary>
        public int EventsCount { get; set; }

        /// <summary>
        /// Snapshot of the state of a game.
        /// </summary>
        internal GameState State
        {
            get => _state;
            set
            {
                _state = value;
                _celestialBodies = null;
            }
        }

        public GameConfiguration Configuration => _state.Configuration;

        public Span<CelestialBodyCache> CelestialBodies
        {
            get
            {
                if (_celestialBodies is null)
                {
                    _celestialBodies = new CelestialBodyCache[_state.CelestialBodies.Length];
                    for (int i = 0; i < _celestialBodies.Length; i++)
                        _celestialBodies[i] = new(this, i);
                }

                return _celestialBodies;
            }
        }

        /// <inheritdoc/>
        public Cache Clone() => new(_state) { ActionsCount = 0, EventsCount = 0 };

        public void CopyFrom(Cache other)
        {
            _state.CopyFrom(other._state);
            TransferHelper.Copy(ref _celestialBodies, other._celestialBodies);
            ActionsCount = other.ActionsCount;
            EventsCount = other.EventsCount;
        }
    }

    sealed class CelestialBodyCache(Cache cache, int i)
        : ICloneable<CelestialBodyCache>,
            ITransferable<CelestialBodyCache>
    {
        Cache _cache = cache;
        int _i = i;

        ImmutableArray<CelestialBodyConfiguration>? _configuration;
        GameState.CelestialBodyData[]? _data;
        CelestialBodyResourcesCache[]? _resources;
        Dictionary<StructureType, CelestialBodyStructuresCache>? _structures;

        public ref readonly CelestialBodyConfiguration Configuration
        {
            get
            {
                if (_configuration is not { } c)
                {
                    c = _cache.Configuration.CelestialBodies;
                    _configuration = c;
                }

                return ref c.AsSpan()[_i];
            }
        }

        public ref GameState.CelestialBodyData Data
        {
            get
            {
                if (_data is not { } d)
                {
                    d = _cache.State.CelestialBodies;
                    _data = d;
                }

                return ref d.AsSpan()[_i];
            }
        }

        public Span<CelestialBodyResourcesCache> Resources
        {
            get
            {
                if (_resources is not { } r)
                {
                    r = new CelestialBodyResourcesCache[
                        _cache.Configuration.CelestialBodies.Length
                    ];
                    for (int i = 0; i < r.Length; i++)
                        r[i] = new(this, i);

                    _resources = r;
                }

                return r;
            }
        }

        public ReadOnlyIndexer<
            CelestialBodyCache,
            StructureType,
            CelestialBodyStructuresCache
        > Structures =>
            new(
                this,
                static (self, type) =>
                {
                    if (self._structures is not { } s)
                    {
                        s = [];
                        self._structures = s;
                    }

                    if (!s.TryGetValue(type, out var cache))
                        cache = s[type] = new(self, type);

                    return cache;
                }
            );

        public CelestialBodyCache Clone() => new(_cache, _i);

        public void CopyFrom(CelestialBodyCache other)
        {
            _cache = other._cache;
            _i = other._i;

            _configuration = other._configuration;
            TransferHelper.Copy(ref _data, other._data);
            TransferHelper.Copy(ref _resources, other._resources);
            TransferHelper.Copy(ref _structures, other._structures);
        }
    }

    sealed class CelestialBodyResourcesCache(CelestialBodyCache cache, int celestialBodyIndex)
        : ICloneable<CelestialBodyResourcesCache>,
            ITransferable<CelestialBodyResourcesCache>
    {
        CelestialBodyCache _cache = cache;
        int _celestialBodyIndex = celestialBodyIndex;
        int? _celestialBodyResourceIndex;

        ImmutableArray<CelestialBodyResource>? _configuration;
        List<GameState.ResourceCount>? _data;

        public ref readonly CelestialBodyResource Configuration
        {
            get
            {
                if (_configuration is not { } c)
                {
                    c = _cache.Configuration.Resources;
                    _configuration = c;
                }

                return ref c.AsSpan()[_celestialBodyIndex];
            }
        }

        public GameState.ResourceCount Data
        {
            get
            {
                if (_data is not { } d)
                {
                    d = _cache.Data.Resources;
                    _data = d;
                }

                if (_celestialBodyResourceIndex is not { } i)
                {
                    for (i = 0; i < d.Count; i++)
                        if (d[i].Type == Configuration.Type)
                            break;

                    if (i == d.Count)
                        d.Add(new(Configuration.Type, 0));

                    _celestialBodyResourceIndex = i;
                }

                return d[i];
            }
            set
            {
                if (_data is not { } d)
                {
                    d = _cache.Data.Resources;
                    _data = d;
                }

                if (_celestialBodyResourceIndex is not { } i)
                {
                    for (i = 0; i < d.Count; i++)
                        if (d[i].Type == Configuration.Type)
                            break;

                    if (i == d.Count)
                        d.Add(new(Configuration.Type, 0));

                    _celestialBodyResourceIndex = i;
                }

                Debug.Assert(value.Type == d[i].Type);

                d[i] = value;
            }
        }

        public CelestialBodyResourcesCache Clone() => new(_cache, _celestialBodyIndex);

        public void CopyFrom(CelestialBodyResourcesCache other)
        {
            _cache = other._cache;
            _celestialBodyIndex = other._celestialBodyIndex;
            _celestialBodyResourceIndex = other._celestialBodyResourceIndex;

            _configuration = other._configuration;
            TransferHelper.CopyImmutable(ref _data, other._data);
        }
    }

    sealed class CelestialBodyStructuresCache(CelestialBodyCache cache, StructureType type)
        : ICloneable<CelestialBodyStructuresCache>,
            ITransferable<CelestialBodyStructuresCache>
    {
        CelestialBodyCache _cache = cache;
        StructureType _type = type;
        int? _celestialBodyStructureIndex;

        List<GameState.StructureCount>? _data;

        public GameState.StructureCount Data
        {
            get
            {
                if (_data is not { } d)
                {
                    d = _cache.Data.Structures;
                    _data = d;
                }

                if (_celestialBodyStructureIndex is not { } i)
                {
                    for (i = 0; i < d.Count; i++)
                        if (d[i].Type == _type)
                            break;

                    if (i == d.Count)
                        d.Add(new(_type, 0));

                    _celestialBodyStructureIndex = i;
                }

                return d[i];
            }
            set
            {
                if (_data is not { } d)
                {
                    d = _cache.Data.Structures;
                    _data = d;
                }

                if (_celestialBodyStructureIndex is not { } i)
                {
                    for (i = 0; i < d.Count; i++)
                        if (d[i].Type == _type)
                            break;

                    if (i == d.Count)
                        d.Add(new(_type, 0));

                    _celestialBodyStructureIndex = i;
                }

                Debug.Assert(value.Type == d[i].Type);

                d[i] = value;
            }
        }

        public CelestialBodyStructuresCache Clone() => new(_cache, _type);

        public void CopyFrom(CelestialBodyStructuresCache other)
        {
            _cache = other._cache;
            _type = other._type;
            _celestialBodyStructureIndex = other._celestialBodyStructureIndex;

            TransferHelper.CopyImmutable(ref _data, other._data);
        }
    }
}
