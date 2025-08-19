// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: AGPL-3.0-only

using System.Collections.Immutable;
using SpaceCapture.Shared.Logic.Rules;
using SpaceCapture.Shared.Logic.Stage;

namespace SpaceCapture.Shared.Logic.Simulation;

partial class GameSimulation
{
    /// <summary>
    /// Representation of the game rules, in a form that takes more memory but can be accessed faster.
    /// </summary>
    /// <param name="rules"></param>
    sealed class RulesCache(RulesSet rules)
    {
        public RulesSet Rules => rules;

        public ImmutableArray<ResourceRuleCache> Resources =
        [
            .. rules.Resources.Select((r, i) => new ResourceRuleCache(r, (ResourceTypeIndex)i)),
        ];

        public ImmutableArray<StructureRuleCache> Structures =
        [
            .. rules.Structures.Select(
                (s, i) =>
                    new StructureRuleCache
                    {
                        Rules = s,
                        Index = (StructureTypeIndex)i,
                        BuildCost = Cache(s.BuildCost, rules),
                        RepairCost = Cache(s.RepairCost, rules),
                        CommitCost = Cache(s.CommitCost, rules),
                        ActiveCost = Cache(s.ActiveCost, rules),
                        Produces = Cache(s.Produces, rules),
                        Stores = Cache(s.Stores, rules),
                    }
            ),
        ];

        static ImmutableArray<ResourceCountCache>? Cache(
            ImmutableArray<ResourceCount>? count,
            RulesSet rules
        ) => count is { } c ? Cache(c, rules) : null;

        static ImmutableArray<ResourceCountCache> Cache(
            ImmutableArray<ResourceCount> count,
            RulesSet rules
        ) =>
            [
                .. count.Select(c => new ResourceCountCache
                {
                    Data = c,
                    Index = (ResourceTypeIndex)
                        rules.Resources.Index().First(r => r.Item.Type == c.Type).Index,
                }),
            ];
    }

    readonly struct ResourceRuleCache(ResourceRule rules, ResourceTypeIndex index)
    {
        public ResourceRule Rules => rules;

        public ResourceTypeIndex Index => index;
    }

    readonly record struct StructureRuleCache(
        StructureRule Rules,
        StructureTypeIndex Index,
        ImmutableArray<ResourceCountCache>? BuildCost,
        ImmutableArray<ResourceCountCache>? RepairCost,
        ImmutableArray<ResourceCountCache> CommitCost,
        ImmutableArray<ResourceCountCache> ActiveCost,
        ImmutableArray<ResourceCountCache> Produces,
        ImmutableArray<ResourceCountCache> Stores
    );

    readonly record struct ResourceCountCache(ResourceCount Data, ResourceTypeIndex Index);
}
