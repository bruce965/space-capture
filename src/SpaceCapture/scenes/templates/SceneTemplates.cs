using Godot;

namespace SpaceCapture;

[GlobalClass]
public partial class SceneTemplates : Resource
{
    public static SceneTemplates Scenes { get; } = ResourceLoader.Load<SceneTemplates>("res://scenes/templates/scene_templates.tres");

    [Export]
    public PackedScene ControlPlanet { get; set; }

    [Export]
    public PackedScene Trail { get; set; }

    [Export]
    public PackedScene ShipsFleet { get; set; }
}
