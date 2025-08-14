// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: AGPL-3.0-only

using SpaceCapture.Shared.Abstractions;

namespace SpaceCapture.Shared.Logic.Rules;

public readonly struct FactoryRule(StructureType structure, ResourceType resource) : IImmutable
{
    public StructureType Structure => structure;

    public ResourceType Resource => resource;
}
