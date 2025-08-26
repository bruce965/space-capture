// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: AGPL-3.0-only

using System.Diagnostics;
using System.Text.Json.Serialization;
using SpaceCapture.Shared.Abstractions;
using SpaceCapture.Shared.Types;

namespace SpaceCapture.Shared.Logic.Simulation;

/// <summary>
/// Resource type and amount of that resource.
/// </summary>
/// <param name="type"></param>
[DebuggerDisplay($"{{{nameof(Type)},nq}} ({{{nameof(Count)},nq}})")]
public struct ResourceCount(ResourceType type) : ICloneable<ResourceCount>, ITransferable<ResourceCount>
{
    /// <summary>
    /// Resource type.
    /// </summary>
    [JsonPropertyName("type")]
    public readonly ResourceType Type => type;

    /// <summary>
    /// Amount.
    /// </summary>
    [JsonPropertyName("count")]
    public FP48D16 Count { get; set; }

    public readonly ResourceCount Clone() => this;

    public void CopyFrom(ResourceCount other) => this = other;
}
