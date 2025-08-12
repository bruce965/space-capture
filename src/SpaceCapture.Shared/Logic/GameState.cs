using System.ComponentModel;
using System.Text.Json.Serialization;
using SpaceCapture.Shared.Logic.Configuration;
using SpaceCapture.Shared.Utilities;

namespace SpaceCapture.Shared.Logic;

/// <summary>
/// Snapshot of the state of a game.
/// </summary>
public sealed partial class GameState(GameConfiguration configuration)
{
    [JsonPropertyName("config")]
    [JsonPropertyOrder(-1)]
    public GameConfiguration Configuration { get; set; } = configuration;

    [JsonIgnore]
    public DeterministicRandom.Seed RandomSeed { get; set; } = configuration.Seed;

    [EditorBrowsable(EditorBrowsableState.Never)]
    [JsonPropertyName("seed")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DeterministicRandom.Seed? JsonRandomSeed
    {
        get => RandomSeed == Configuration.Seed ? null : RandomSeed;
        set => RandomSeed = value ?? Configuration.Seed;
    }
}
