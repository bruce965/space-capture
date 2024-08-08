extends Node
class_name GameLogic

const LOGIC_TICKS_PER_SECOND: float = 20.
const LOGIC_SECONDS_PER_TICK = 1. / LOGIC_TICKS_PER_SECOND

## Neutral player owning all non-conquered planets.
@export var neutral_player: Player

## Game players.
@export var players: Array[Player] = []

## Game planets.
@export var planets: Array[Planet] = []

var ticks_count: int
var extra_time: float

signal game_start(game: GameLogic)
signal game_tick(game: GameLogic, tick: int)

func _ready() -> void:
	_game_init()
	game_start.emit(self)

func _process(delta: float) -> void:
	extra_time += delta
	
	while extra_time > LOGIC_SECONDS_PER_TICK:
		extra_time -= LOGIC_SECONDS_PER_TICK
		_game_tick(ticks_count)
		ticks_count += 1

func _game_init() -> void:
	ticks_count = 0
	extra_time = 0.

	for player in players:
		game_start.connect(player._on_game_start)
		game_tick.connect(player._on_game_tick)

	for i in range(planets.size()):
		var planet := planets[i]
		planet.player = players[i] if i < players.size() else neutral_player
		planet.is_selected = false

func _game_tick(tick: int) -> void:
	for planet in planets:
		if planet.player != neutral_player and tick % 10 == 0:
			planet.population += 1

	for player in players:
		for op in player.pending_operations:
			if op is GameOperationSendFleet:
				var count = min(op.max_count, op.from.population)
				op.from.population -= count
				if op.from.player == op.to.player:
					op.to.population += count
				else:
					op.to.population -= count
					if op.to.population <= 0:
						op.to.player = op.from.player
						op.to.population = abs(op.from.population)
			else:
				assert(false, "Unknown GameOperation: %s" % [op])

		player.pending_operations.clear()

	game_tick.emit(self, tick)
