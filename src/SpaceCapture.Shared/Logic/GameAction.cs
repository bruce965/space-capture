namespace SpaceCapture.Shared.Logic;

public readonly record struct GameAction(long Tick);

public readonly record struct GameEvent(long Tick);
