// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: AGPL-3.0-only

namespace SpaceCapture.Shared.Logic;

public abstract record class GameAction(long Tick);

public record class BuildStructureAction(long Tick, int CelestialBody, StructureType Structure) : GameAction(Tick);
