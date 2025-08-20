// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: MIT

namespace SpaceCapture.Shared.Abstractions;

/// <summary>
/// Supports copying data from another object, avoiding allocations when possible.
/// </summary>
/// <typeparam name="TSelf"></typeparam>
public interface ITransferable<TSelf>
    where TSelf : notnull, ITransferable<TSelf>
{
    /// <summary>
    /// Efficiently replace data in the current object with a copy of the data
    /// from <paramref name="other"/>, avoiding allocations when possible.
    /// </summary>
    /// <param name="other"></param>
    void CopyFrom(TSelf other);
}
