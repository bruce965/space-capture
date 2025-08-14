// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: MIT

using System;
using Godot;
using Microsoft.Extensions.Logging;

namespace Bruce965.Godot.Services;

class GodotLogger<T> : ILogger<T>
{
    class NoOpDisposable : IDisposable
    {
        public static NoOpDisposable Instance { get; } = new();

        public void Dispose() { }
    }

    static readonly bool s_isDebugBuild = OS.IsDebugBuild();

    static readonly string s_format = $"[{typeof(T).Name}] {{0}}: {{1}}";
    static readonly string s_formatShort = $"[{typeof(T).Name}]: {{0}}";

    public IDisposable BeginScope<TState>(TState state)
        where TState : notnull => NoOpDisposable.Instance;

    public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Information || s_isDebugBuild;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception exception,
        Func<TState, Exception, string> formatter
    )
    {
        string message = formatter(state, exception);

        if (logLevel >= LogLevel.Error)
            GD.PushError(string.Format(s_format, GetLevelString(logLevel), message));
        else if (logLevel >= LogLevel.Warning)
            GD.PushWarning(string.Format(s_format, GetLevelString(logLevel), message));
        else if (logLevel == LogLevel.Information)
            GD.Print(string.Format(s_formatShort, message));
        else
            GD.Print(string.Format(s_format, GetLevelString(logLevel), message));
    }

    static string GetLevelString(LogLevel level) =>
        level switch
        {
            LogLevel.Trace => "TRACE",
            LogLevel.Debug => "DEBUG",
            LogLevel.Information => "INFO",
            LogLevel.Warning => "WARN",
            LogLevel.Error => "ERROR",
            LogLevel.Critical => "CRITICAL",
            _ => level.ToString(),
        };
}
