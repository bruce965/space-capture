using Godot;

namespace SpaceCapture;

[GlobalClass]
public partial class Player : Resource
{
    [Export]
    public Color Color { get; set; } = Colors.Magenta;

    [Export]
    public Texture2D Icon { get; set; }

    public virtual void ProcessGameTick(GameLogic game)
    {
    }
}
