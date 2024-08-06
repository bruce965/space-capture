extends Player
class_name LocalPlayer

@export var ui: GameUI

func _on_game_start(game: GameLogic) -> void:
	ui._on_game_started(game)

func _on_game_tick(game: GameLogic, tick: int) -> void:
	pass
