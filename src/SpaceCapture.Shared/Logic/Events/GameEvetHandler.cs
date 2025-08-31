// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: AGPL-3.0-only

namespace SpaceCapture.Shared.Logic.Events;

/// <summary>
/// Handler for events that happen in the game.
/// </summary>
/// <param name="evt">Game event.</param>
/// <param name="revert">
/// If <see langword="true"/>, this previously-emitted event may no longer
/// be valid and should be reverted; it may be re-emitted at a later time
/// with <paramref name="revert"/> set to <see langword="false"/> if it will
/// be deemed to actually still be valid.
/// </param>
public delegate void GameEventHandler(GameEvent evt, bool revert = false);
