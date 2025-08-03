using System;
using Godot;

namespace SpaceCapture;

public partial class Ship : Node2D
{
    const float Acceleration = 100f;
    const float RotationSpeed = 3f;
    const float MaxSpeed = 100f;

    Color _color;

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
    public Vector2 TargetPosition { get; set; }

    [Export]
    public Vector2 Velocity { get; set; }

    public override void _Process(double delta)
    {
        Vector2 targetVelocity = (TargetPosition - GlobalPosition).Normalized() * MaxSpeed;
        Vector2 adjustedTargetVelocity = targetVelocity - Velocity;
        Vector2 acceleration = adjustedTargetVelocity == Vector2.Zero ? Vector2.Zero : adjustedTargetVelocity.Normalized() * Acceleration;

        float decelerateAtDistance = Velocity.LengthSquared() / (2f * Acceleration);
        float distanceSquared = GlobalPosition.DistanceSquaredTo(TargetPosition);
        bool slow_down = distanceSquared < decelerateAtDistance*decelerateAtDistance;
        if (slow_down)
            acceleration = Velocity.Normalized() * -Acceleration;

        Velocity += acceleration * (float)delta;

        float speedSquared = Velocity.LengthSquared();
        if (speedSquared > MaxSpeed*MaxSpeed)
            Velocity = Velocity / MathF.Sqrt(speedSquared) * MaxSpeed;

        GlobalPosition += Velocity * (float)delta;

        Rotation = Mathf.RotateToward(Rotation, GlobalPosition.AngleToPoint(TargetPosition), Math.Min(1f, (float)delta) * RotationSpeed);
    }

    void UpdateColor(Color shipColor)
    {
        ((ShaderMaterial)GetNode<Line2D>("%LineTrail").Material).SetShaderParameter("color", shipColor);
    }
}
