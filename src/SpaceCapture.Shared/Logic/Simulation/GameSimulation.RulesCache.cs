using SpaceCapture.Shared.Logic.Rules;

namespace SpaceCapture.Shared.Logic.Simulation;

partial class GameSimulation
{
    /// <summary>
    /// Representation of the game rules, in form that takes more memory but can be accessed faster.
    /// </summary>
    /// <param name="rules"></param>
    sealed class RulesCache(RulesSet rules)
    {
        /// <summary>
        /// Types of structures that produce each type of resource.
        /// </summary>
        public ILookup<ResourceType, StructureType> FactoriesByResource = rules.Factories.ToLookup(
            x => x.Resource,
            x => x.Structure
        );
    }
}
