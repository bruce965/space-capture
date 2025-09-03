// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: AGPL-3.0-only

using System.Collections.Immutable;
using System.Text.Json.Serialization;
using SpaceCapture.Shared.Utilities;

namespace SpaceCapture.Shared;

/// <summary>
/// Visual class of a celestial body.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<CelestialBodyClass>))]
public enum CelestialBodyClass
{
    #region Stars

    [JsonStringEnumMemberName("yellow-dwarf")]
    YellowDwarf,

    #endregion

    #region Planets

    [JsonStringEnumMemberName("terra")]
    Terra,

    [JsonStringEnumMemberName("gas-giant")]
    GasGiant,

    #endregion
}

public static class CelestialBodyClassExtensions
{
    public static CelestialBodyType GetClassType(this CelestialBodyClass @class) =>
        @class switch
        {
            < CelestialBodyClass.Terra => CelestialBodyType.Star,
            _ => CelestialBodyType.Planet,
        };
}

/// <summary>
/// General type of a celestial body.
/// </summary>
[Flags]
public enum CelestialBodyType
{
    Star = 1 << 0,
    Planet = 1 << 1,

    None = 0,
    Any = Star | Planet,
}

public static class CelestialBodyTypeExtensions
{
    static readonly ImmutableDictionary<CelestialBodyType, ImmutableArray<CelestialBodyClass>> s_classesByType =
        EnumHelper
            .ValuesOf<CelestialBodyType>()
            .ToImmutableDictionary(
                t => t,
                t =>
                    EnumHelper.ValuesOf<CelestialBodyClass>().Where(c => t.HasFlag(c.GetClassType())).ToImmutableArray()
            );

    public static ImmutableArray<CelestialBodyClass> GetClasses(this CelestialBodyType type) =>
        s_classesByType.TryGetValue(type, out ImmutableArray<CelestialBodyClass> classes) ? classes : [];
}
