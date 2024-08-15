extends Node2D
class_name ShipsFleet

@export var departed_at: float # Time.get_ticks_msec()

@export var arrives_at: float # Time.get_ticks_msec()

@export var from: Vector2

@export var to: Vector2

@export var player: Player

@export var count: int :
	set(value):
		count = value
		_update__instances_count()

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

var _instances: Array[Node2D] = []

@export var ship_template: PackedScene

func _update__instances_count() -> void:
	var diff = count - _instances.size()

	for i in range(diff):
		var j = float(1 + _instances.size())
		var ship := ship_template.instantiate()
		add_child(ship)
		ship.position = Vector2.from_angle(j) * j * 2.
		_instances.push_back(ship)

	for i in range(-diff):
		var ship: Node2D = _instances.pop_back()
		remove_child(ship)

var _arrived := false

func _update_position(tick: int) -> void:
	if not _arrived:
		var clamped_tick := clampi(tick, departed_at, arrives_at)
		var progress := float(clamped_tick - departed_at) / float(arrives_at - departed_at)
		position = lerp(from, to, smoothstep(0., 1., progress))
		if tick >= arrives_at:
			_arrived = true
			trail.show = false

			for ship in _instances:
				ship.emitting = false

			await get_tree().create_timer(5.).timeout
			queue_free()

#endregion
