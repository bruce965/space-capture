using System.Text.Json.Serialization;

namespace SpaceCapture.Shared.Logic;

partial class GameState : ICloneable
{
    /// <summary>
    /// Current time in ticks since the beginning of the simulation.
    /// </summary>
    [JsonPropertyName("tick")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public long CurrentTick { get; set; }

    public void TickGameClock()
    {
        CurrentTick++;

        // TODO: advance simulation.
    }
}
