// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: AGPL-3.0-only

using System.Text.Json.Serialization;
using SpaceCapture.Server.Endpoints;
using SpaceCapture.Shared.Logic.Configuration;
using SpaceCapture.Shared.Types;

WebApplicationBuilder builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

WebApplication app = builder.Build();

RouteGroupBuilder v1Api = app.MapGroup("/api/v1");
v1Api.MapGet("/game/generate", GameEndpoints.NewGame);

app.Run();

[JsonSerializable(typeof(GameConfiguration))]
[JsonSerializable(typeof(GameState))]
[JsonSerializable(typeof(ReadOnlyMemory<FP32D10>))]
[JsonSerializable(typeof(CelestialBodyResource[]))]
[JsonSerializable(typeof(IEnumerable<CelestialBodyResource>))]
internal partial class AppJsonSerializerContext : JsonSerializerContext;
