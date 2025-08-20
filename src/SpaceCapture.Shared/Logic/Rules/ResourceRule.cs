// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: AGPL-3.0-only

using System.Diagnostics;
using SpaceCapture.Shared.Abstractions;
using SpaceCapture.Shared.Types;

namespace SpaceCapture.Shared.Logic.Rules;

/// <summary>
/// Describes the behaviour of a type of resource.
/// </summary>
/// <param name="type"></param>
[DebuggerDisplay($"{{{nameof(Type)},nq}}")]
public readonly struct ResourceRule(ResourceType type) : IImmutable
{
    /// <summary>
    /// Type of resource.
    /// </summary>
    public ResourceType Type => type;

    /// <summary>
    /// Damage absorbtion coefficient (<c>0</c> to absorbe no damage; <c>1</c> to absorbe all damage).
    /// </summary>
    /// <remarks>
    /// Damage is applied to structures first, and then to resources.
    /// </remarks>
    public FP32D16 DamageAbsorbtion { get; init; }

    /// <summary>
    /// If <see langword="true"/>, clestial bodies will remain under control of the player as long
    /// as there are some resources of this type left.
    /// </summary>
    public bool IsActive { get; init; }
}
