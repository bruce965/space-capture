using System.Text.Json.Serialization;
using SpaceCapture.Shared.Types;

namespace SpaceCapture.Shared.Logic;

partial class GameState
{
    /// <summary>
    /// Resource type and amount of that resource.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="count"></param>
    public readonly struct ResourceCount(ResourceType type, FP32D10 count)
    {
        /// <summary>
        /// Resource type.
        /// </summary>
        [JsonPropertyName("type")]
        public ResourceType Type => type;

        /// <summary>
        /// Amount.
        /// </summary>
        [JsonPropertyName("count")]
        public FP32D10 Count => count;
    }
}
