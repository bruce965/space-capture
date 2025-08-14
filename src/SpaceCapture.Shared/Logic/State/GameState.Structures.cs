using System.Text.Json.Serialization;
using SpaceCapture.Shared.Abstractions;

namespace SpaceCapture.Shared.Logic.State;

partial class GameState
{
    /// <summary>
    /// Structure type and amount of those structures.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="count"></param>
    public readonly struct StructureCount(StructureType type, int count) : IImmutable
    {
        /// <summary>
        /// Structure type.
        /// </summary>
        [JsonPropertyName("type")]
        public StructureType Type => type;

        /// <summary>
        /// Amount.
        /// </summary>
        [JsonPropertyName("count")]
        public int Count => count;
    }
}
