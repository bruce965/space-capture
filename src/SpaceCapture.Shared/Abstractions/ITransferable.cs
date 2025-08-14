// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: MIT

namespace SpaceCapture.Shared.Abstractions;

/// <summary>
/// Supports copying data from another object, avoiding allocations when possible.
/// </summary>
/// <typeparam name="TSelf"></typeparam>
public interface ITransferable<TSelf>
    where TSelf : notnull
{
    void CopyFrom(TSelf other);
}
