// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: AGPL-3.0-only

namespace SpaceCapture.Shared.Logic.Events;

public abstract record class GameEvent(long Tick);

public record class PlayerAddEvent(long Tick, int Index) : GameEvent(Tick);

public record class CelestialBodyAddEvent(long Tick, int Index) : GameEvent(Tick);

public record class StageUpdateEvent(long Tick) : GameEvent(Tick);

public record class PlayerUpdateEvent(long Tick, int Index) : GameEvent(Tick);

public record class CelestialBodyUpdateEvent(long Tick, int Index) : GameEvent(Tick);

public record class PlayerRemoveEvent(long Tick, int Index) : GameEvent(Tick);

public record class CelestialBodyRemoveEvent(long Tick, int Index) : GameEvent(Tick);
