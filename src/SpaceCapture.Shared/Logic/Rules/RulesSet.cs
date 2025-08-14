using System.Collections.Immutable;
using SpaceCapture.Shared.Abstractions;

namespace SpaceCapture.Shared.Logic.Rules;

/// <summary>
/// Set of rules for a game.
/// </summary>
public partial class RulesSet : IImmutable
{
    /// <summary>
    /// Structures that produce/extract resources from celestial bodies.
    /// </summary>
    public ImmutableArray<FactoryRule> Factories { get; init; }
}
