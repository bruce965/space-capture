extends Sprite2D
class_name Trail

static var _inv_texture_width := NAN

@export var color: Color :
	set(value):
		color = value
		_update_color(color)

@export var show: bool = true :
	set(value):
		show = value
		visible = true

@export var auto_free: bool

var start_position: Vector2 :
	set(value):
		start_position = value
		_update_transform(value, end_position)

var end_position: Vector2 :
	set(value):
		end_position = value
		_update_transform(start_position, value)

var _opacity := 0.

func _update_transform(start: Vector2, end: Vector2) -> void:
	global_position = (start + end) * .5
	look_at(end)
	
	if is_nan(_inv_texture_width):
		_inv_texture_width = 1. / texture.get_width()
		
	scale = Vector2((end - start).length() * _inv_texture_width, 1.)

func _update_color(color: Color) -> void:
	var c := Color(color.r, color.g, color.b, color.a * _opacity)
	material.set_shader_parameter('color', c)

func _process(delta: float) -> void:
	var prev_opacity := _opacity
	_opacity = Utils.damp(_opacity, 1. if show else 0., 1e-4, delta)
	if _opacity != prev_opacity:
		if _opacity < .01:
			_opacity = 0.
			visible = false
			if auto_free:
				queue_free()
		elif _opacity > .99:
			_opacity = 1.

		_update_color(color)
