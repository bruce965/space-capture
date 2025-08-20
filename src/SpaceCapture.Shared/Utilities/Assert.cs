// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: MIT

using System.Runtime.CompilerServices;

namespace SpaceCapture.Shared.Utilities;

public static class Assert
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Upcast<T>(T value) => value;
}
