// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: AGPL-3.0-only

namespace SpaceCapture.Shared.Logic.Events;

/// <summary>
/// Describes an event that happened in the game.
/// </summary>
public abstract record class GameEvent;

public record class PlayerAddEvent(int Index) : GameEvent;

public record class CelestialBodyAddEvent(int Index) : GameEvent;

public record class StageUpdateEvent : GameEvent;

public record class PlayerUpdateEvent(int Index) : GameEvent;

public record class CelestialBodyUpdateEvent(int Index) : GameEvent;
