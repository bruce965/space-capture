// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: AGPL-3.0-only

using System.Collections.Immutable;
using System.Diagnostics;
using SpaceCapture.Shared.Logic.Rules;
using SpaceCapture.Shared.Types;
using SpaceCapture.Shared.Utilities;

namespace SpaceCapture.Shared.Logic.Simulation;

partial class GameSimulation<TData>
{
    /// <summary>
    /// Representation of the game rules, in a form that takes more memory but can be accessed faster.
    /// </summary>
    /// <param name="rules"></param>
    public sealed class RulesCache(RulesSet rules)
    {
        /// <inheritdoc cref="RulesSet"/>
        public RulesSet Rules => rules;

        readonly ResourceRuleCache[] _resources =
        [
            .. rules.Resources.Select((r, i) => new ResourceRuleCache(r, new() { Index = i })),
        ];

        readonly StructureRuleCache[] _structures =
        [
            .. rules.Structures.Select(
                (s, i) =>
                    new StructureRuleCache
                    {
                        Rules = s,
                        Index = new() { Index = i },
                        BuildCost = Cache(s.BuildCost, rules, s.BuildTicks),
                        RepairCost = Cache(s.RepairCost, rules, s.BuildTicks),
                        ActivationCost = Cache(s.ActivationCost, rules),
                        ActiveCost = Cache(s.ActiveCost, rules),
                        Produces = Cache(s.Produces, rules),
                        Stores = Cache(s.Stores, rules),
                        RepairedDamagePerTick = s.MaxHealth / (FP48D16)s.BuildTicks,
                    }
            ),
        ];

        /// <inheritdoc cref="RulesSet.Resources"/>
        public ReadOnlyAccessor<ResourceRuleCache, ResourceType, ResourceTypeIndex> Resources =>
            new(_resources, _resourceTypeToIndex);

        /// <inheritdoc cref="RulesSet.Structures"/>
        public ReadOnlyAccessor<StructureRuleCache, StructureType, StructureTypeIndex> Structures =>
            new(_structures, _structureTypeToIndex);

        internal readonly ImmutableDictionary<ResourceType, ResourceTypeIndex> _resourceTypeToIndex = rules
            .Resources.Select(
                (r, i) => KeyValuePair.Create<ResourceType, ResourceTypeIndex>(r.Type, new() { Index = i })
            )
            .ToImmutableDictionary();

        internal readonly ImmutableDictionary<StructureType, StructureTypeIndex> _structureTypeToIndex = rules
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

    /// <summary>
    /// Representation of resource rules, in a form that takes more memory but can be accessed faster.
    /// </summary>
    /// <param name="Rules"><inheritdoc cref="ResourceRule" path="/summary"/></param>
    /// <param name="Index">Index of this resource in arrays.</param>
    [DebuggerDisplay($"{{{nameof(Rules)},nq}}")]
    public readonly record struct ResourceRuleCache(ResourceRule Rules, ResourceTypeIndex Index);

    /// <summary>
    /// Representation of structure rules, in a form that takes more memory but can be accessed faster.
    /// </summary>
    /// <param name="Rules"><inheritdoc cref="StructureRule" path="/summary"/></param>
    /// <param name="Index">Index of this structure in arrays.</param>
    /// <param name="BuildCost"><inheritdoc cref="StructureRule.BuildCost" path="/summary"/></param>
    /// <param name="RepairCost"><inheritdoc cref="StructureRule.RepairCost" path="/summary"/></param>
    /// <param name="ActivationCost"><inheritdoc cref="StructureRule.ActivationCost" path="/summary"/></param>
    /// <param name="ActiveCost"><inheritdoc cref="StructureRule.ActiveCost" path="/summary"/></param>
    /// <param name="Produces"><inheritdoc cref="StructureRule.Produces" path="/summary"/></param>
    /// <param name="Stores"><inheritdoc cref="StructureRule.Stores" path="/summary"/></param>
    /// <param name="RepairedDamagePerTick">How much damage may be repaired each tick.</param>
    [DebuggerDisplay($"{{{nameof(Rules)},nq}}")]
    public readonly record struct StructureRuleCache(
        StructureRule Rules,
        StructureTypeIndex Index,
        ImmutableArray<ResourceCountCache>? BuildCost,
        ImmutableArray<ResourceCountCache>? RepairCost,
        ImmutableArray<ResourceCountCache> ActivationCost,
        ImmutableArray<ResourceCountCache> ActiveCost,
        ImmutableArray<ResourceCountCache> Produces,
        ImmutableArray<ResourceCountCache> Stores,
        FP48D16 RepairedDamagePerTick
    );

    /// <summary>
    /// Representation of resource rules related to a specific structure type,
    /// in a form that takes more memory but can be accessed faster.
    /// </summary>
    /// <param name="Data"><inheritdoc cref="ResourceCount" path="/summary"/></param>
    /// <param name="Index">Index of this resource in arrays.</param>
    /// <param name="CostPerTick">How many resources of this type in one tick.</param>
    [DebuggerDisplay($"{{{nameof(Data)},nq}}")]
    public readonly record struct ResourceCountCache(ResourceCount Data, ResourceTypeIndex Index, FP48D16 CostPerTick);
}
