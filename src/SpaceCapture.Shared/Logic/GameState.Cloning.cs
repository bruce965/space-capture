namespace SpaceCapture.Shared.Logic;

partial class GameState : ICloneable
{
    public GameState Clone() =>
        new(Configuration)
        {
            CurrentTick = CurrentTick,
            Players = [.. Players],
            CelestialBodies = [.. CelestialBodies],
            Fleets = [.. Fleets],
        };

    object ICloneable.Clone() => Clone();
}
