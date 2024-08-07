extends Node
class_name GameLogic

const LOGIC_TICKS_PER_SECOND: float = 20.;
const LOGIC_SECONDS_PER_TICK = 1. / LOGIC_TICKS_PER_SECOND;

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
		game_start.connect(planet._on_game_start)
		game_tick.connect(planet._on_game_tick)

func _process(delta: float) -> void:
	extra_time += delta
	
	while extra_time > LOGIC_SECONDS_PER_TICK:
		extra_time -= LOGIC_SECONDS_PER_TICK
		game_tick.emit(self, ticks_count)
		ticks_count += 1
