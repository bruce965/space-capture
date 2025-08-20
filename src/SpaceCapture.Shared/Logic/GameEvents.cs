// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: AGPL-3.0-only

namespace SpaceCapture.Shared.Logic;

public abstract record class GameEvent(long Tick);

//public record class CelestialBodyCreateEvent(long Tick, ) : GameEvent(Tick);
