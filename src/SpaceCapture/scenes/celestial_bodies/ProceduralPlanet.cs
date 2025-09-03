// SPDX-FileCopyrightText: Copyright 2024 Fabio Iotti
// SPDX-License-Identifier: AGPL-3.0-only

using System;
using Godot;
using SpaceCapture.Shared;
using SpaceCapture.Shared.Logic.Stage;
using SpaceCapture.Shared.Utilities;

namespace SpaceCapture;

public partial class ProceduralPlanet : Node2D
{
    [ExportGroup("Planet")]
    /// <summary>
    /// Minimum size of the planet in pixels.
    /// </summary>
    /// <value></value>
    [Export(PropertyHint.Range, "30,500,.001,or_greater,or_less")]
    public float MinSize { get; set; } = 30;

    /// <summary>
    /// Maximum size of the planet in pixels.
    /// </summary>
    /// <value></value>
    [Export(PropertyHint.Range, "30,500,.001,or_greater,or_less")]
    public float MaxSize { get; set; } = 500;

    /// <summary>
    /// Minimum rotation of the planet in deg.
    /// </summary>
    [Export(PropertyHint.Range, "30,360,.001")]
    public float MinTilt { get; set; } = 0;

    /// <summary>
    /// Maximum rotation of the planet in deg.
    /// </summary>
    [Export(PropertyHint.Range, "30,360,.001")]
    public float MaxTilt { get; set; } = 0;

    /// <summary>
    /// Minimum rotation speed of the planet in rad/sec.
    /// </summary>
    /// <value></value>
    [Export(PropertyHint.Range, "0,1,.001,or_greater,or_less")]
    public float MinRotationSpeed { get; set; } = 0;

    /// <summary>
    /// Maximum rotation speed of the planet in rad/sec.
    /// </summary>
    /// <value></value>
    [Export(PropertyHint.Range, "0,1,.001,or_greater,or_less")]
    public float MaxRotationSpeed { get; set; } = 1;

    [ExportGroup("Star")]
    /// <summary>
    /// Whether this body is emissive, like a star.
    /// </summary>
    /// <value></value>
    [Export]
    public bool Emissive { get; set; }

    /// <summary>
    /// Minimum fluidity of the mantle.
    /// </summary>
    /// <value></value>
    [Export(PropertyHint.Range, "0,1,.001")]
    public float MinFluidity { get; set; } = 0;

    /// <summary>
    /// Maximum fluidity of the mantle.
    /// </summary>
    /// <value></value>
    [Export(PropertyHint.Range, "0,1,.001")]
    public float MaxFluidity { get; set; } = 1;

    [ExportGroup("Weather")]
    /// <summary>
    /// Minimum size of clouds between 0 (no clouds) and 1 (covered in clouds completely).
    /// </summary>
    /// <value></value>
    [Export(PropertyHint.Range, "0,1,.001,or_greater,or_less")]
    public float MinCloudsSize { get; set; } = 0;

    /// <summary>
    /// Maximum size of clouds between 0 (no clouds) and 1 (covered in clouds completely).
    /// </summary>
    /// <value></value>
    [Export(PropertyHint.Range, "0,1,.001,or_greater,or_less")]
    public float MaxCloudsSize { get; set; } = 1;

    /// <summary>
    /// Minimum density of clouds between 0 (very thin) and 1 (very thick).
    /// </summary>
    /// <value></value>
    [Export(PropertyHint.Range, "0,1,.001,or_greater,or_less")]
    public float MinCloudsDensity { get; set; } = 0;

    /// <summary>
    /// Maximum density of clouds between 0 (very thin) and 1 (very thick).
    /// </summary>
    /// <value></value>
    [Export(PropertyHint.Range, "0,1,.001,or_greater,or_less")]
    public float MaxCloudsDensity { get; set; } = 1;

    /// <summary>
    /// Minimum value of how often clouds change shape.
    /// </summary>
    /// <value></value>
    [Export(PropertyHint.Range, "0,1,.001,or_greater")]
    public float MinCloudsTurbulence { get; set; } = 0;

    /// <summary>
    /// Maximum value of how often clouds change shape.
    /// </summary>
    /// <value></value>
    [Export(PropertyHint.Range, "0,1,.001,or_greater")]
    public float MaxCloudsTurbulence { get; set; } = 1;

    /// <summary>
    /// Minimum wind speed in rad/sec.
    /// </summary>
    /// <value></value>
    [Export(PropertyHint.Range, "0,1,.001,or_greater")]
    public float MinWindSpeed { get; set; } = 0;

    /// <summary>
    /// Maximum wind speed in rad/sec.
    /// </summary>
    /// <value></value>
    [Export(PropertyHint.Range, "0,1,.001,or_greater")]
    public float MaxWindSpeed { get; set; } = 1;

    [ExportGroup("Atmosphere")]
    /// <summary>
    /// Minimum size of the atmosphere halo around the planet.
    /// </summary>
    /// <value></value>
    [Export(PropertyHint.Range, "0,1,.001")]
    public float MinAtmosphereSize { get; set; } = 0;

    /// <summary>
    /// Maximum size of the atmosphere halo around the planet.
    /// </summary>
    /// <value></value>
    [Export(PropertyHint.Range, "0,1,.001")]
    public float MaxAtmosphereSize { get; set; } = 1;

    /// <summary>
    /// Range of possible colors of the atmosphere halo around the planet.
    /// </summary>
    /// <value></value>
    [Export(PropertyHint.Range, "0,1,.001")]
    public Gradient AtmosphereColorRange { get; set; }

    public void Initialize(CelestialBodyType type, CelestialBodyConfiguration configuration, DeterministicRandom seed)
    {
        // Always generate all properties at random without skipping any, to preserve determinism.
        CelestialBodyClass randClass = seed.Pick(type.GetClasses().AsSpan());
        float randSize = RandFloat(ref seed, MinSize, MaxSize);
        float randTilt = RandFloat(ref seed, MinTilt, MaxTilt);
        float randRotationSpeed = RandFloat(ref seed, MinRotationSpeed, MaxRotationSpeed);
        float randFluidity = RandFloat(ref seed, MinFluidity, MaxFluidity);
        float randCloudsSize = RandFloat(ref seed, MinCloudsSize, MaxCloudsSize);
        float randCloudsDensity = RandFloat(ref seed, MinCloudsDensity, MaxCloudsDensity);
        float randCloudsTurbulence = RandFloat(ref seed, MinCloudsTurbulence, MaxCloudsTurbulence);
        float randWindSpeed = RandFloat(ref seed, MinWindSpeed, MaxWindSpeed);
        float randAtmosphereSize = RandFloat(ref seed, MinAtmosphereSize, MaxAtmosphereSize);
        float randAtmosphereColor = RandFloat(ref seed, 0, 1);

        int noiseSeed = seed.Next();

        // Get the actual values (either random or overridden by the configuration).
        CelestialBodyClass _ = configuration.Class ?? randClass;
        float size = (float?)configuration.Size ?? randSize;
        float tilt = (float?)configuration.Tilt ?? randTilt;
        float rotationSpeed = (float?)configuration.RotationSpeed ?? randRotationSpeed;
        float fluidity = (float?)configuration.Fluidity ?? randFluidity;
        float cloudsSize = (float?)configuration.CloudsSize ?? randCloudsSize;
        float cloudsDensity = (float?)configuration.CloudsDensity ?? randCloudsDensity;
        float cloudsTurbulence = (float?)configuration.CloudsTurbulence ?? randCloudsTurbulence;
        float windSpeed = (float?)configuration.WindSpeed ?? randWindSpeed;
        float atmosphereSize = (float?)configuration.AtmosphereSize ?? randAtmosphereSize;
        Color atmosphereColor = configuration.AtmosphereColor is { } c
            ? new(c.Rgba)
            : AtmosphereColorRange.Sample(randAtmosphereColor);

        // Assign properties to the Godot node.
        Scale = size * Vector2.One;
        RotationDegrees = tilt;

        ShaderMaterial material = (ShaderMaterial)Material;
        material.SetShaderParameter("size", size);
        material.SetShaderParameter("rotationSpeed", rotationSpeed);
        material.SetShaderParameter("emissive", Emissive);
        material.SetShaderParameter("fluidity", fluidity);
        material.SetShaderParameter("cloudsSize", cloudsSize);
        material.SetShaderParameter("cloudsDensity", cloudsDensity);
        material.SetShaderParameter("cloudsTurbulence", cloudsTurbulence);
        material.SetShaderParameter("windSpeed", windSpeed);
        material.SetShaderParameter("atmosphereSize", atmosphereSize);
        material.SetShaderParameter("atmosphereColor", atmosphereColor);

        NoiseTexture3D noiseTexture = (NoiseTexture3D)material.GetShaderParameter("noise");
        FastNoiseLite noise = (FastNoiseLite)noiseTexture.Noise;
        noise.Seed = noiseSeed;
    }

    static float RandFloat(ref DeterministicRandom seed, float min, float max) =>
        min + (float)seed.Next() / int.MaxValue * (max - min);
}
