// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: AGPL-3.0-only

namespace SpaceCapture.Shared.Logic.Rules;

partial class RulesSet
{
    /// <summary>
    /// Set of rules for standard games.
    /// </summary>
    public static RulesSet Standard { get; } =
        new()
        {
            Factories =
            [
                new(StructureType.Farm, ResourceType.Food),
                new(StructureType.MetalMine, ResourceType.Metal),
                new(StructureType.GasMine, ResourceType.Gas),
            ],
        };
}
