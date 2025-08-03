using Godot;

namespace SpaceCapture;

public partial class ProceduralPlanet : Node2D
{
    [ExportGroup("Planet")]

    /// <summary>
    /// Size of the planet in pixels.
    /// </summary>
    /// <value></value>
    [Export(PropertyHint.Range, "30,500,.001,or_greater,or_less")]
    public float Size
    {
        get => Scale.X;
        set
        {
            Scale = value * Vector2.One;
            ((ShaderMaterial)Material).SetShaderParameter("size", value);
        }
    }

    /// <summary>
    /// Rotation speed of the planet in rad/sec.
    /// </summary>
    /// <value></value>
    [Export(PropertyHint.Range, "0,1,.001,or_greater,or_less")]
    public float RotationSpeed
    {
        get => (float)((ShaderMaterial)Material).GetShaderParameter("rotationSpeed").AsDouble();
        set => ((ShaderMaterial)Material).SetShaderParameter("rotationSpeed", value);
    }

    [ExportGroup("Weather")]

    /// <summary>
    /// Size of clouds between 0 (no clouds) and 1 (covered in clouds completely).
    /// </summary>
    /// <value></value>
    [Export(PropertyHint.Range, "0,1,.001,or_greater,or_less")]
    public float CloudsSize
    {
        get => (float)((ShaderMaterial)Material).GetShaderParameter("cloudsSize").AsDouble();
        set => ((ShaderMaterial)Material).SetShaderParameter("cloudsSize", value);
    }

    /// <summary>
    /// Density of clouds between 0 (very thin) and 1 (very thick).
    /// </summary>
    /// <value></value>
    [Export(PropertyHint.Range, "0,1,.001,or_greater,or_less")]
    public float CloudDensity
    {
        get => (float)((ShaderMaterial)Material).GetShaderParameter("cloudsDensity").AsDouble();
        set => ((ShaderMaterial)Material).SetShaderParameter("cloudsDensity", value);
    }

    /// <summary>
    /// How often clouds change shape.
    /// </summary>
    /// <value></value>
    [Export(PropertyHint.Range, "0,1,.001,or_greater")]
    public float CloudTurbulence
    {
        get => (float)((ShaderMaterial)Material).GetShaderParameter("cloudsTurbulence").AsDouble();
        set => ((ShaderMaterial)Material).SetShaderParameter("cloudsTurbulence", value);
    }

    /// <summary>
    /// Wind speed in rad/sec.
    /// </summary>
    /// <value></value>
    [Export(PropertyHint.Range, "0,1,.001,or_greater")]
    public float WindSpeed
    {
        get => (float)((ShaderMaterial)Material).GetShaderParameter("windSpeed").AsDouble();
        set => ((ShaderMaterial)Material).SetShaderParameter("windSpeed", value);
    }

    [ExportGroup("Atmosphere")]

    /// <summary>
    /// Size of the atmosphere halo around the planet.
    /// </summary>
    /// <value></value>
    [Export(PropertyHint.Range, "0,1,.001")]
    public float AtmosphereSize
    {
        get => (float)((ShaderMaterial)Material).GetShaderParameter("atmosphereSize").AsDouble();
        set => ((ShaderMaterial)Material).SetShaderParameter("atmosphereSize", value);
    }

    /// <summary>
    /// Size of the atmosphere halo around the planet.
    /// </summary>
    /// <value></value>
    [Export(PropertyHint.Range, "0,1,.001")]
    public Color AtmosphereColor
    {
        get => ((ShaderMaterial)Material).GetShaderParameter("atmosphereColor").AsColor();
        set => ((ShaderMaterial)Material).SetShaderParameter("atmosphereColor", value);
    }

    public override void _Ready()
    {
        Size = 100f;
        RotationSpeed = .05f;

        CloudsSize = .05f;
        CloudDensity = .22f;
        CloudTurbulence = .01f;
        WindSpeed = .22f;

        AtmosphereSize = .3f;
        AtmosphereColor = new(0f, .3f, 1f, .3f);
    }

}
