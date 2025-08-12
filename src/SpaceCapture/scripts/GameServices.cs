using Bruce965.Godot.Services;
using Microsoft.Extensions.DependencyInjection;

namespace SpaceCapture;

public partial class GameServices : GameServicesBase
{
    protected override void ConfigureServices(ServiceCollection services)
    {
        services.AddSingleton<GameLogic>();
    }
}
