// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: MIT

using System.Text;

namespace SpaceCapture.Shared.Utilities;

public static class RandomNameGenerator
{
    static readonly MarkovChain _stars = Load("stars.bin");
    static readonly MarkovChain _planets = Load("planets.bin");

    static MarkovChain Load(string name)
    {
        string resourceName = $"SpaceCapture.Shared.Resources.{name}";

        using Stream? stream =
            typeof(RandomNameGenerator).Assembly.GetManifestResourceStream(resourceName)
            ?? throw new KeyNotFoundException();

        using StreamReader reader = new(stream, Encoding.UTF8);

        string data = reader.ReadToEnd();
        return new(data);
    }

    public static string Star(ref DeterministicRandom.Seed seed) => _stars.Generate(ref seed);

    public static string Planet(ref DeterministicRandom.Seed seed) => _planets.Generate(ref seed);
}
