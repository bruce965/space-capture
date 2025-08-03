using Godot;

namespace SpaceCapture;

public partial class Trail : Sprite2D
{
    static float s_invTextureWidth = float.NaN;

    Color _color;
    bool _showTrail = true;
    Vector2 _startPosition;
    Vector2 _endPosition;
    float _opacity = 0f;

    [Export]
    public Color Color
    {
        get => _color;
        set
        {
            _color = value;
            UpdateColor(_color);
        }
    }

    [Export]
    public bool ShowTrail
    {
        get => _showTrail;
        set
        {
            _showTrail = value;
            Visible = true;
        }
    }

    [Export]
    public bool AutoFree { get; set; }

    [Export]
    public Vector2 StartPosition
    {
        get => _startPosition;
        set
        {
            _startPosition = value;
            UpdateTransform(value, _endPosition);
        }
    }

    [Export]
    public Vector2 EndPosition
    {
        get => _endPosition;
        set
        {
            _endPosition = value;
            UpdateTransform(_startPosition, value);
        }
    }

    void UpdateTransform(Vector2 start, Vector2 end)
    {
        GlobalPosition = (start + end) * .5f;
        LookAt(end);
        
        if (float.IsNaN(s_invTextureWidth))
            s_invTextureWidth = 1f / Texture.GetWidth();
            
        Scale = new Vector2((end - start).Length() * s_invTextureWidth, 1f);
    }

    void UpdateColor(Color trailColor)
    {
        Color c = new(trailColor.R, trailColor.G, trailColor.B, trailColor.A * _opacity);
        ((ShaderMaterial)Material).SetShaderParameter("color", c);
    }

    public override void _Process(double delta)
    {
        float prevOpacity = _opacity;
        _opacity = Utils.Damp(_opacity, ShowTrail ? 1f : 0f, 1e-4f, delta);
        if (_opacity != prevOpacity)
        {
            if (_opacity < .01f)
            {
                _opacity = 0f;
                Visible = false;
                if (AutoFree)
                    QueueFree();
            }
            else if (_opacity > .99f)
            {
                _opacity = 1f;
            }

            UpdateColor(Color);
        }
    }
}
