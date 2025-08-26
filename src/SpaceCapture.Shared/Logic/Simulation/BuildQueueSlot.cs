// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: AGPL-3.0-only

using System.Text.Json.Serialization;
using SpaceCapture.Shared.Abstractions;

namespace SpaceCapture.Shared.Logic.Simulation;

public struct BuildQueueSlot(StructureType type) : ICloneable<BuildQueueSlot>, ITransferable<BuildQueueSlot>
{
    /// <summary>
    /// Structure type.
    /// </summary>
    [JsonPropertyName("type")]
    public readonly StructureType Type => type;

    /// <summary>
    /// Progress in ticks.
    /// </summary>
    [JsonPropertyName("progress")]
    public long Progress { get; set; }

    public readonly BuildQueueSlot Clone() => this;

    public void CopyFrom(BuildQueueSlot other) => this = other;
}
