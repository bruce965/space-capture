extends Node2D
class_name ShipsFleet

const RANK_SIZE = 7
const RANKS_DISTANCE = 12.
const SHOULDER_DISTANCE = 8.
const DISPERSE_AT_DISTANCE = 100.

@export var departed_at: int # Time.get_ticks_msec()

@export var arrives_at: int # Time.get_ticks_msec()

@export var from: Vector2

@export var to: Vector2

@export var player: Player

@export var count: int :
	set(value):
		count = value
		_update_ships_count()

@export var trail: Trail

var _game: GameState

func register_game(game: GameState) -> void:
	_game = game
	_game.game_ticked.connect(_update_position)
	_update_position(game.current_tick)

func _exit_tree() -> void:
	if _game != null:
		_game.game_ticked.disconnect(_update_position)
		_game = null

#region Graphics

var _ships: Array[Ship] = []

@export var ship_template: PackedScene

func _update_ships_count() -> void:
	var diff = count - _ships.size()

	for i in range(diff):
		var ship: Ship = ship_template.instantiate()
		ship.color = player.color
		add_child(ship)
		ship.position = from
		ship.rotation = randf_range(0, 2 * PI)
		ship.velocity = randf_range(60., 120.) * Vector2.from_angle(ship.rotation)
		_ships.push_back(ship)

	for i in range(-diff):
		var ship: Ship = _ships.pop_back()
		remove_child(ship)

var _arrived := false

func _update_position(tick: int) -> void:
	if not _arrived:
		var clamped_tick := clampi(tick, departed_at, arrives_at)
		var progress := float(clamped_tick - departed_at) / float(arrives_at - departed_at)
		var fleet_position: Vector2 = lerp(from, to, clampf(progress, 0., 1.))
		var dispersiveness := clampf(sqrt(minf(fleet_position.distance_squared_to(from), fleet_position.distance_squared_to(to))) / DISPERSE_AT_DISTANCE, 0., 1.)

		var angle = from.angle_to_point(to)
		var ranks_count := ceili(float(_ships.size()) / RANK_SIZE)
		for i in range(_ships.size()):
			var rank := floori(float(i) / RANK_SIZE)
			var position_in_rank := i % RANK_SIZE
			var ships_in_rank := RANK_SIZE if rank != ranks_count - 1 else (_ships.size() - (ranks_count - 1) * RANK_SIZE)
			var assigned_position := Vector2(rank * -RANKS_DISTANCE - abs(position_in_rank - ships_in_rank / 2.) * 5., float(position_in_rank - (ships_in_rank - 1) * .5) * SHOULDER_DISTANCE)
			_ships[i].target_position = fleet_position + lerp(Vector2.ZERO, assigned_position.rotated(angle) + Vector2.from_angle(sin(i + tick / 10.)) * 2., dispersiveness)

		if tick >= arrives_at:
			_arrived = true
			trail.show_trail = false

			# TODO: fade out ships.

			await get_tree().create_timer(5.).timeout
			queue_free()

#endregion
