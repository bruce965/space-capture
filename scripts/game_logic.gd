class_name GameLogic
extends Node

const LOGIC_TICKS_PER_SECOND := 20.
const LOGIC_SECONDS_PER_TICK := 1. / LOGIC_TICKS_PER_SECOND

## Container for planets.
@export var planets_container: Node2D
## Container for trails.
@export var trails_container: Node2D
## Container for fleets.
@export var fleets_container: Node2D

## UI to interact with planets.
@export var planets_ui: PlanetsUI

## Game players.
@export var players: Array[Player]

## Game planets.
@export var planets: Array[Planet]

var _game: GameState
var _planet_controls: Array[ControlPlanet]

var _extra_time: float

func _ready() -> void:
	# New game.
	_game = GameState.new()

	# Add players.
	for i in range(players.size()):
		_game.add_player()

	# Add planets.
	for i in range(planets.size()):
		# Data.
		var data := GameState.PlanetData.new()
		data.position = Vector2i(planets[i].location)
		data.grow_every_ticks = 10
		data.player_id = (i + 1) if (i < players.size() - 1) else 0
		data.population = 0
		_game.add_planet(data)

		# UI control.
		var control: ControlPlanet = SceneTemplates.scenes.control_planet.instantiate()
		control.position = planets[i].location
		control.player = players[data.player_id]
		control.population = data.population
		_planet_controls.push_back(control)
		planets_container.add_child(control)
		planets_ui.register_planet(control)

	# Register UI signals.
	_game.planet_player_changed.connect(_set_planet_player)
	_game.planet_population_changed.connect(_set_planet_population)
	_game.fleet_dispatched.connect(_show_dispatched_fleet)
	planets_ui.fleet_dispatched.connect(_dispatch_fleet)

func _process(delta: float) -> void:
	_extra_time += delta
	
	while _extra_time > LOGIC_SECONDS_PER_TICK:
		_extra_time -= LOGIC_SECONDS_PER_TICK
		_game.tick()

#region Graphics

func _set_planet_player(planet_id: int, player_id: int) -> void:
	_planet_controls[planet_id].player = players[player_id]

func _set_planet_population(planet_id: int, population: int) -> void:
	_planet_controls[planet_id].population = population

func _show_dispatched_fleet(from_planet_id: int, to_planet_id: int, count: int, player_id: int, departed_at_tick: int, arrives_at_tick: int) -> void:
	var trail: Trail = SceneTemplates.scenes.trail.instantiate()
	trail.start_position = planets[from_planet_id].location
	trail.end_position = planets[to_planet_id].location
	trail.color = players[player_id].color
	trail.auto_free = true
	trails_container.add_child(trail)

	var fleet: ShipsFleet = SceneTemplates.scenes.ships_fleet.instantiate()
	fleet.departed_at = departed_at_tick
	fleet.arrives_at = arrives_at_tick
	fleet.from = planets[from_planet_id].location
	fleet.to = planets[to_planet_id].location
	fleet.player = players[player_id]
	fleet.count = count
	fleet.trail = trail
	fleets_container.add_child(fleet)

	fleet.register_game(_game)

#endregion

#region Commands

func _dispatch_fleet(from: ControlPlanet, to: ControlPlanet) -> void:
	var player_id := players.find(from.player)
	var from_planet_id := _planet_controls.find(from)
	var to_planet_id := _planet_controls.find(to)
	var max_count: int = ceil(from.population / 2.)
	_game.dispatch_fleet(player_id, from_planet_id, to_planet_id, max_count)

#endregion
