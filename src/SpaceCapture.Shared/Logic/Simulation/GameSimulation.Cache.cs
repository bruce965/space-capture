// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: AGPL-3.0-only

using System.Collections.Immutable;
using System.Diagnostics;
using SpaceCapture.Shared.Logic.Rules;
using SpaceCapture.Shared.Types;

namespace SpaceCapture.Shared.Logic.Simulation;

partial class GameSimulation<TData>
{
    /// <summary>
    /// Representation of the game rules, in a form that takes more memory but can be accessed faster.
    /// </summary>
    /// <param name="rules"></param>
    public sealed class RulesCache(RulesSet rules)
    {
        public RulesSet Rules => rules;

        public ImmutableArray<ResourceRuleCache> Resources =
        [
            .. rules.Resources.Select((r, i) => new ResourceRuleCache(r, new() { Index = i })),
        ];

        public ImmutableArray<StructureRuleCache> Structures =
        [
            .. rules.Structures.Select(
                (s, i) =>
                    new StructureRuleCache
                    {
                        Rules = s,
                        Index = new() { Index = i },
                        BuildCost = Cache(s.BuildCost, rules, s.BuildTicks),
                        RepairCost = Cache(s.RepairCost, rules, s.BuildTicks),
                        CommitCost = Cache(s.CommitCost, rules),
                        ActiveCost = Cache(s.ActiveCost, rules),
                        Produces = Cache(s.Produces, rules),
                        Stores = Cache(s.Stores, rules),
                        RepairedDamagePerTick = s.MaxHealth / (FP48D16)s.BuildTicks,
                    }
            ),
        ];

        internal ImmutableDictionary<ResourceType, ResourceTypeIndex> ResourceTypeToIndex = rules
            .Resources.Select(
                (r, i) => KeyValuePair.Create<ResourceType, ResourceTypeIndex>(r.Type, new() { Index = i })
            )
            .ToImmutableDictionary();

        internal ImmutableDictionary<StructureType, StructureTypeIndex> StructureTypeToIndex = rules
            .Structures.Select(
                (s, i) => KeyValuePair.Create<StructureType, StructureTypeIndex>(s.Type, new() { Index = i })
            )
            .ToImmutableDictionary();

        static ImmutableArray<ResourceCountCache>? Cache(
            ImmutableArray<ResourceCount>? count,
            RulesSet rules,
            long ticks = 1
        ) => count is { } c ? Cache(c, rules, ticks) : null;

        static ImmutableArray<ResourceCountCache> Cache(
            ImmutableArray<ResourceCount> count,
            RulesSet rules,
            long ticks = 1
        ) =>
            [
                .. count.Select(c => new ResourceCountCache
                {
                    Data = c,
                    Index = new() { Index = rules.Resources.Index().First(r => r.Item.Type == c.Type).Index },
                    CostPerTick = c.Count / (FP48D16)ticks,
                }),
            ];
    }

    [DebuggerDisplay($"{{{nameof(Rules)},nq}}")]
    public readonly struct ResourceRuleCache(ResourceRule rules, ResourceTypeIndex index)
    {
        public ResourceRule Rules => rules;

        public ResourceTypeIndex Index => index;
    }

    [DebuggerDisplay($"{{{nameof(Rules)},nq}}")]
    public readonly record struct StructureRuleCache(
        StructureRule Rules,
        StructureTypeIndex Index,
        ImmutableArray<ResourceCountCache>? BuildCost,
        ImmutableArray<ResourceCountCache>? RepairCost,
        ImmutableArray<ResourceCountCache> CommitCost,
        ImmutableArray<ResourceCountCache> ActiveCost,
        ImmutableArray<ResourceCountCache> Produces,
        ImmutableArray<ResourceCountCache> Stores,
        FP48D16 RepairedDamagePerTick
    );

    [DebuggerDisplay($"{{{nameof(Data)},nq}}")]
    public readonly record struct ResourceCountCache(ResourceCount Data, ResourceTypeIndex Index, FP48D16 CostPerTick);
}
