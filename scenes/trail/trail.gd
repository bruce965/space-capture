extends Sprite2D

@export var color: Color :
	set(value):
		material.set_shader_parameter('color', value)
