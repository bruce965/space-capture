using SpaceCapture.Shared.Abstractions;

namespace SpaceCapture.Shared.Logic.Rules;

public readonly struct FactoryRule(StructureType structure, ResourceType resource) : IImmutable
{
    public StructureType Structure => structure;

    public ResourceType Resource => resource;
}
