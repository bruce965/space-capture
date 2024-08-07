extends Control
class_name GameUI

@export var trail: Trail

var game: GameLogic

var selected_planet: Planet
var pointed_planet: Planet
var dragging_from_planet: bool
var last_cursor_position: Vector2

func _on_game_started(game: GameLogic) -> void:
	self.game = game

	for planet in game.planets:
		planet.selected.connect(_on_planet_selected)
		planet.pointer_entered.connect(_on_planet_pointer_entered)
		planet.pointer_exited.connect(_on_planet_pointer_exited)

	#trail.color = game.player

func _on_planet_selected(planet: Planet) -> void:
	if selected_planet != null:
		selected_planet.is_selected = false

	selected_planet = planet
	selected_planet.is_selected = true

	dragging_from_planet = true
	_update_trail(0., true)

func _on_planet_pointer_entered(planet: Planet) -> void:
	pointed_planet = planet

func _on_planet_pointer_exited(planet: Planet) -> void:
	if pointed_planet == planet:
		pointed_planet = null

func _unhandled_input(event: InputEvent) -> void:
	if event is InputEventMouseMotion:
		# The trail follows the cursor.
		last_cursor_position = event.global_position

	if event is InputEventMouseButton and event.button_index == MOUSE_BUTTON_LEFT and event.is_released():
		# The trail disappears when no longer dragging.
		dragging_from_planet = false
		
		# Detect mouse released on a planet and spawn a fleet.
		if selected_planet != null and selected_planet.player is LocalPlayer and pointed_planet != null:
			var count = ceil(selected_planet.population / 2)
			selected_planet.population -= count
			if selected_planet.player == pointed_planet.player:
				pointed_planet.population += count
			else:
				pointed_planet.population -= count
				if pointed_planet.population <= 0:
					pointed_planet.player = selected_planet.player
					pointed_planet.population = abs(pointed_planet.population)

#region Trail

var trail_opacity := 0.

func _update_trail(delta: float, snap_position: bool) -> void:
	var show_trail := dragging_from_planet and selected_planet != null and selected_planet.player is LocalPlayer
	if show_trail or trail.visible:
		trail_opacity = Util.damp(trail_opacity, 1. if show_trail else 0., 1e-4, delta)
		trail.color = Color(selected_planet.player.color, trail_opacity)
		trail.visible = trail_opacity > .01
		
		if show_trail:
			var target_position = last_cursor_position if pointed_planet == null else pointed_planet.global_position
			trail.start_position = selected_planet.global_position
			trail.end_position = target_position if snap_position else Util.damp(trail.end_position, target_position, 1e-20, delta)

#endregion

func _process(delta: float) -> void:
	_update_trail(delta, false)
