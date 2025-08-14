// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: MIT

using System;
using Godot;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Bruce965.Godot.Services;

public abstract partial class GameServicesBase : Node
{
    public IServiceProvider Provider { get; private set; }

    public override void _Ready()
    {
        ServiceCollection services = [];
        ConfigureDefaultServices(services);
        ConfigureServices(services);

        Provider = services.BuildServiceProvider();

        using IServiceScope scope = Provider.CreateScope();

        ILogger logger = scope.ServiceProvider.GetService<ILogger<GameServicesBase>>();

        logger?.LogInformation("Game services configured.");
    }

    public override void _ExitTree()
    {
        if (Provider is IDisposable d)
            d.Dispose();
    }

    static void ConfigureDefaultServices(ServiceCollection services)
    {
        services.AddSingleton(typeof(ILogger<>), typeof(GodotLogger<>));
    }

    protected abstract void ConfigureServices(ServiceCollection services);
}
