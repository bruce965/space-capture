// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: AGPL-3.0-only

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
