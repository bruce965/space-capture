extends Node2D
class_name Planet

## The player currenly controlling this planet.
var player: Player :
	set(value):
		player = value
		var color := Color.MAGENTA if player == null else player.color
		%Selection.color = color
		%PopulationLabel.modulate = color
		%PopulationIcon.modulate = color
		%PopulationIcon.texture = null if player == null else player.icon

## Number of units currently stationed at the planet.
@export var population: int :
	set(value):
		population = value
		%PopulationLabel.text = "%s" % value

func _ready() -> void:
	is_selected = false

@export_category("UI")

signal selected(planet: Planet)
signal pointer_entered(planet: Planet)
signal pointer_exited(planet: Planet)

var is_selected: bool :
	get:
		return %Selection.visible
	set(value):
		%Selection.visible = value

func _on_input_listener_input_event(viewport: Node, event: InputEvent, shape_idx: int) -> void:
	# TODO: handle touch appropriately (InputEventScreenTouch).
	if event is InputEventMouseButton and event.button_index == MOUSE_BUTTON_LEFT and event.is_pressed():
		selected.emit(self)

func _on_input_listener_mouse_entered() -> void:
	pointer_entered.emit(self)

func _on_input_listener_mouse_exited() -> void:
	pointer_exited.emit(self)
