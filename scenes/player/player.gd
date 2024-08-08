extends Node
class_name Player

@export var color: Color = Color.MAGENTA

@export var icon: Texture2D

var pending_operations: Array[GameOperation] = []

func _on_game_start(game: GameLogic) -> void:
	pass

func _on_game_tick(game: GameLogic, tick: int) -> void:
	pass
