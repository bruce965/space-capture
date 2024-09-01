class_name Ship
extends Node2D

@export var color: Color :
	set(value):
		color = value
		_update_color(color)

func _update_color(ship_color: Color) -> void:
	%LineTrail.material.set_shader_parameter('color', ship_color)
