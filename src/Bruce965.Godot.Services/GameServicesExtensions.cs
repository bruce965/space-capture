// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: MIT

using System;
using Bruce965.Godot.Services;
using Godot;
using Microsoft.Extensions.DependencyInjection;

namespace Godot;

public static class GameServicesExtensions
{
    public static IServiceProvider GetServiceProvider(this Node node) =>
        GetSingletonInstance(node).Provider;

    /// <inheritdoc cref="ServiceProviderServiceExtensions.GetService{T}(IServiceProvider)"/>
    public static T GetService<T>(this Node node) => node.GetServiceProvider().GetService<T>();

    static GameServicesBase GetSingletonInstance(Node node)
    {
        Window root = node.GetTree().Root;

        GameServicesBase instance = root.GetNodeOrNull<GameServicesBase>("GameServices");
        if (instance is null)
        {
            GD.PushError(
                $"Game services not configured. You should add a custom class that extends {nameof(GameServicesBase)} and register it as autoload with name 'GameServices'."
            );

            instance = new DefaultGameServices() { Name = "GameServices" };
            root.AddChild(instance);
        }

        return instance;
    }
}
