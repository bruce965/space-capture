// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: MIT

using System.ComponentModel;

namespace SpaceCapture.Shared.Abstractions;

[EditorBrowsable(EditorBrowsableState.Never)]
public interface IIndex
{
    internal int Index { get; }
}
