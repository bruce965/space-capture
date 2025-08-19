// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: AGPL-3.0-only

using System.Text.Json.Serialization;

namespace SpaceCapture.Shared;

/// <summary>
/// Type of structure.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<StructureType>))]
public enum StructureType
{
    #region Special

    [JsonStringEnumMemberName("house")]
    House,

    #endregion

    #region Factories

    [JsonStringEnumMemberName("farm")]
    Farm,

    [JsonStringEnumMemberName("metal-mine")]
    MetalMine,

    [JsonStringEnumMemberName("gas-mine")]
    GasMine,

    #endregion

    #region Planteary Upgrades

    [JsonStringEnumMemberName("walls1")]
    WallsI,

    [JsonStringEnumMemberName("walls2")]
    WallsII,

    [JsonStringEnumMemberName("walls3")]
    WallsIII,

    [JsonStringEnumMemberName("aa1")]
    AntiAirI,

    [JsonStringEnumMemberName("aa2")]
    AntiAirII,

    [JsonStringEnumMemberName("aa3")]
    AntiAirIII,

    #endregion
}
