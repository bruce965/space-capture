using System.Text.Json.Serialization;

namespace SpaceCapture.Shared.Logic;

partial class GameState
{
    /// <summary>
    /// Structure type and amount of those structures.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="count"></param>
    public readonly struct StructureCount(StructureType type, int count)
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
