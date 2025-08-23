// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: MIT

using System;
using System.Globalization;

namespace SpaceCapture;

public static class Utility
{
    /// <summary>
    /// Represent a rounded-down positive number in short for (max 3 digits or letters up to 99999).
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    // csharpier-ignore
    public static string ToShortString(float value) =>
        value switch
        {
            < 0 => "NEG",
            < 10 => MathF.Round(value, 1, MidpointRounding.ToZero).ToString("0.0", CultureInfo.InvariantCulture),
            < 1_000 => MathF.Floor(value).ToString(CultureInfo.InvariantCulture),
            < 1_000_000 => $"{MathF.Floor(value / 1_000).ToString(CultureInfo.InvariantCulture)}K",
            < 1_000_000_000 => $"{MathF.Floor(value / 1_000_000).ToString(CultureInfo.InvariantCulture)}M",
            < 1_000_000_000_000 => $"{MathF.Floor(value / 1_000_000_000).ToString(CultureInfo.InvariantCulture)}G",
            < 100_000_000_000_000 => $"{MathF.Floor(value / 1_000_000_000_000).ToString(CultureInfo.InvariantCulture)}T",
            _ => "MAX"
        };
}
