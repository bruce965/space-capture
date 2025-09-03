// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: MIT

using SpaceCapture.Shared.Abstractions;

namespace SpaceCapture.Shared.Types;

public readonly record struct Color(byte Red, byte Green, byte Blue, byte Alpha = 255) : IImmutable
{
    public uint Rgba => (uint)Red << 24 | (uint)Green << 16 | (uint)Blue << 8 | Alpha;
}
