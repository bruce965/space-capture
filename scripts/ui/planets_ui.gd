class_name PlanetsUI
extends Control

## Container for trails.
@export var trails_container: Node2D

var _trail: Trail

signal fleet_dispatched(from: ControlPlanet, to: ControlPlanet)

func register_planet(control: ControlPlanet) -> void:
	control.selected.connect(_on_planet_selected)
	control.pointer_entered.connect(_on_planet_pointer_entered)
	control.pointer_exited.connect(_on_planet_pointer_exited)

func _ready() -> void:
	_trail = SceneTemplates.scenes.trail.instantiate()
	_trail.visible = false
	trails_container.add_child(_trail)

func _process(delta: float) -> void:
	_update_trail(delta, false)

func _unhandled_input(event: InputEvent) -> void:
	if event is InputEventMouse:
		_handle_mouse_input(event)

#region Mouse input

var _last_cursor_position: Vector2

func _handle_mouse_input(event: InputEventMouse) -> void:
	if event is InputEventMouseMotion:
		# The trail follows the cursor.
		_last_cursor_position = event.global_position

	if event is InputEventMouseButton and event.button_index == MOUSE_BUTTON_LEFT and event.is_released():
		# The trail disappears when no longer dragging.
		_dragging_from_planet = false
		
		# Detect mouse released on a planet and spawn a fleet.
		if _selected_planet != null and _selected_planet.player is LocalPlayer and _pointed_planet != null:
			fleet_dispatched.emit(_selected_planet, _pointed_planet)

#endregion

#region Planets

var _selected_planet: ControlPlanet
var _pointed_planet: ControlPlanet
var _dragging_from_planet: bool

func _on_planet_selected(planet: ControlPlanet) -> void:
	if _selected_planet != null:
		_selected_planet.is_selected = false

	_selected_planet = planet
	_selected_planet.is_selected = true

	_dragging_from_planet = true
	_update_trail(0., true)

func _on_planet_pointer_entered(planet: ControlPlanet) -> void:
	_pointed_planet = planet

func _on_planet_pointer_exited(planet: ControlPlanet) -> void:
	if _pointed_planet == planet:
		_pointed_planet = null

#endregion

#region Trail

func _update_trail(delta: float, snap_position: bool) -> void:
	_trail.show = _dragging_from_planet and _selected_planet != null and _selected_planet.player is LocalPlayer
	if _trail.show:
		_trail.color = _selected_planet.player.color
		var target_position = _last_cursor_position if _pointed_planet == null else _pointed_planet.global_position
		_trail.start_position = _selected_planet.global_position
		_trail.end_position = target_position if snap_position else Utils.damp(_trail.end_position, target_position, 1e-20, delta)

#endregion
