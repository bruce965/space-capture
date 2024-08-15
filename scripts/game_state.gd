class_name GameState
extends Resource

class PlayerData:
	var commands: Array[Command] = []

class PlanetData:
	var position: Vector2i
	var grow_every_ticks: int
	var player_id: int
	var population: int

class FleetData:
	var count: int
	var player_id: int
	var to_planet_id: int
	var arrives_at_tick: int

enum CommandType {
	DISPATCH_FLEET,
}

class Command:
	var type: CommandType
	var wait_tick: int

class DispatchFleedCommand extends Command:
	var from_planet_id: int
	var to_planet_id: int
	var max_count: int
	func _init():
		type = CommandType.DISPATCH_FLEET

const NEUTRAL_PLAYER_ID := 0
const FLEET_TAKEOFF_PLUS_LANDING_TIME := 20 # ticks
const FLEET_SPEED := 2 # pixels/tick

signal game_ticked(tick: int)
signal planet_population_changed(planet_id: int, population: int)
signal planet_player_changed(planet_id: int, player_id: int)
signal fleet_dispatched(from_planet_id: int, to_planet_id: int, count: int, player_id: int, departed_at_tick: int, arrives_at_tick: int)

var _players: Array[PlayerData] = []
var _planets: Array[PlanetData] = []
var _fleets: Array[FleetData] = []

var current_tick: int

## Add a player to the game.
## The first player is expected to be the game master / neutral player,
## who does not play and for whom the population never increases.
func add_player(data := PlayerData.new()) -> int:
	_players.push_back(data)
	return _players.size() - 1

## Add a planet to the game.
func add_planet(data := PlanetData.new()) -> int:
	_planets.push_back(data)
	return _planets.size() - 1

## Advance game status by one tick.
func tick() -> void:
	current_tick += 1

	# Handle planets growth.
	for planet_id in range(_planets.size()):
		var planet := _planets[planet_id]
		if planet.player_id != NEUTRAL_PLAYER_ID and current_tick % planet.grow_every_ticks == 0:
			planet.population += 1
			planet_population_changed.emit(planet_id, planet.population)

	# Handle player commands.
	for player_id in range(_players.size()):
		var player := _players[player_id]
		if player.commands.size() > 0 && current_tick >= player.commands[0].wait_tick:
			var command: Command = player.commands.pop_back()
			match command.type:
				CommandType.DISPATCH_FLEET:
					_handle_dispatch_fleet(player_id, command)
				_:
					assert(false, "Unknown command: %s" % [command])

	# Handle fleets arrival.
	while _fleets.size() > 0 && current_tick >= _fleets[0].arrives_at_tick:
		var fleet: FleetData = _fleets.pop_back()
		_handle_fleet_arrival(fleet)

	game_ticked.emit(current_tick)

## Dispatch a fleet.
func dispatch_fleet(player_id: int, from_planet_id: int, to_planet_id: int, max_count: int) -> void:
	var dispatch := DispatchFleedCommand.new()
	dispatch.from_planet_id = from_planet_id
	dispatch.to_planet_id = to_planet_id
	dispatch.max_count = max_count
	_issue_command(player_id, dispatch)

func _issue_command(player_id: int, command: Command) -> void:
	var commands := _players[player_id].commands
	var index := commands.map(func (x): return x.wait_tick).bsearch(command.wait_tick, false)
	commands.insert(index, command)

func _handle_dispatch_fleet(player_id: int, dispatch: DispatchFleedCommand) -> void:
	var from = _planets[dispatch.from_planet_id]
	if from.player_id == player_id:
		var to = _planets[dispatch.to_planet_id]
		var count = min(dispatch.max_count, from.population)
		from.population -= count
		planet_population_changed.emit(dispatch.from_planet_id, from.population)

		var fleet := FleetData.new()
		fleet.count = count
		fleet.player_id = player_id
		fleet.to_planet_id = dispatch.to_planet_id
		fleet.arrives_at_tick = current_tick + FLEET_TAKEOFF_PLUS_LANDING_TIME + ceili(from.position.distance_to(to.position) / FLEET_SPEED)

		var index := _fleets.map(func (x): return x.arrives_at_tick).bsearch(fleet.arrives_at_tick, false)
		_fleets.insert(index, fleet)

		fleet_dispatched.emit(dispatch.from_planet_id, fleet.to_planet_id, fleet.count, fleet.player_id, current_tick, fleet.arrives_at_tick)

func _handle_fleet_arrival(fleet: FleetData) -> void:
	var to = _planets[fleet.to_planet_id]
	if fleet.player_id == to.player_id:
		to.population += fleet.count
	else:
		to.population -= fleet.count
		if to.population <= 0:
			to.player_id = fleet.player_id
			to.population = abs(to.population)
			planet_player_changed.emit(fleet.to_planet_id, to.player_id)
		planet_population_changed.emit(fleet.to_planet_id, to.population)
