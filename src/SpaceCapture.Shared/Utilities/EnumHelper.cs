// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: MIT

using System.Collections.Immutable;

namespace SpaceCapture.Shared.Utilities;

public static class EnumHelper
{
    static class Values<TEnum>
        where TEnum : struct, Enum
    {
        internal static readonly ImmutableArray<TEnum> s_values = [.. Enum.GetValues<TEnum>()];
    }

    public static ImmutableArray<TEnum> ValuesOf<TEnum>()
        where TEnum : struct, Enum => Values<TEnum>.s_values;
}
