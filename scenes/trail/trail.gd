extends Sprite2D
class_name Trail

@onready var inv_texture_width = 1. / texture.get_width()

@export var color: Color :
	set(value):
		material.set_shader_parameter('color', value)

var start_position: Vector2 :
	set(value):
		start_position = value
		_update_transform(value, end_position)

var end_position: Vector2 :
	set(value):
		end_position = value
		_update_transform(start_position, value)

func _update_transform(start: Vector2, end: Vector2) -> void:
	global_position = (start + end) * .5
	look_at(end) 
	scale = Vector2((end - start).length() * inv_texture_width, 1.)
