using System.Text.Json.Serialization;

namespace SpaceCapture.Shared.Logic.State;

partial class GameState
{
    /// <summary>
    /// Number of ticks processed since the beginning of the simulation.
    /// </summary>
    [JsonPropertyName("tick")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public long Tick { get; set; }
}
