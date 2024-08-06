extends Control
class_name GameUI

var game: GameLogic

var selected_planet: Planet
var pointed_planet: Planet

func _on_game_started(game: GameLogic) -> void:
	self.game = game

	for planet in game.planets:
		planet.selected.connect(_on_planet_selected)
		planet.pointer_entered.connect(_on_planet_pointer_entered)
		planet.pointer_exited.connect(_on_planet_pointer_exited)

func _on_planet_selected(planet: Planet) -> void:
	if selected_planet != null:
		selected_planet.is_selected = false

	selected_planet = planet
	selected_planet.is_selected = true

func _on_planet_pointer_entered(planet: Planet) -> void:
	pointed_planet = planet

func _on_planet_pointer_exited(planet: Planet) -> void:
	if pointed_planet == planet:
		pointed_planet = null

func _unhandled_input(event: InputEvent) -> void:
	if event is InputEventMouseButton and event.button_index == MOUSE_BUTTON_LEFT and event.is_released():
		if selected_planet != null and pointed_planet != null:
			var count = ceil(selected_planet.population / 2)
			selected_planet.population -= count
			if selected_planet.player == pointed_planet.player:
				pointed_planet.population += count
			else:
				pointed_planet.population -= count
				if pointed_planet.population <= 0:
					pointed_planet.player = selected_planet.player
					pointed_planet.population = abs(pointed_planet.population)
