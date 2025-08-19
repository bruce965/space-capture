// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: AGPL-3.0-only

using System.Collections.Immutable;
using SpaceCapture.Shared.Abstractions;

namespace SpaceCapture.Shared.Logic.Rules;

/// <summary>
/// Set of rules for a game.
/// </summary>
public partial class RulesSet : IImmutable
{
    /// <summary>
    /// Types of resources in this game.
    /// </summary>
    public ImmutableArray<ResourceRule> Resources { get; init; } = [];

    /// <summary>
    /// Types of structures in this game.
    /// </summary>
    public ImmutableArray<StructureRule> Structures { get; init; } = [];
}
