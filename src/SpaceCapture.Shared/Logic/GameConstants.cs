// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: AGPL-3.0-only

using SpaceCapture.Shared.Types;

namespace SpaceCapture.Shared.Logic;

/// <summary>
/// Game constants.
/// </summary>
public static class GameConstants
{
    /// <summary>
    /// How many game ticks in a real-time second.
    /// </summary>
    public const long TicksPerSecond = 32;

    /// <summary>
    /// How many real-time seconds does a game tick cover (approximate).
    /// </summary>
    public static readonly FP32D16 SecondsPerTick = (FP32D16)1 / (FP32D16)TicksPerSecond;
}
