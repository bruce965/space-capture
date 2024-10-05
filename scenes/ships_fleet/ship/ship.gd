class_name Ship
extends Node2D

const ACCELERATION = 100.
const ROTATION_SPEED = 3.
const MAX_SPEED = 100.

@export var color: Color :
	set(value):
		color = value
		_update_color(color)

@export var target_position: Vector2

var velocity: Vector2

func _process(delta: float) -> void:
	var target_velocity := (target_position - global_position).normalized() * MAX_SPEED
	var adjusted_target_velocity := target_velocity - velocity
	var acceleration := Vector2.ZERO if adjusted_target_velocity == Vector2.ZERO else adjusted_target_velocity.normalized() * ACCELERATION

	var decelerate_at_distance := velocity.length_squared() / (2. * ACCELERATION)
	var distance_squared := global_position.distance_squared_to(target_position)
	var slow_down := distance_squared < decelerate_at_distance*decelerate_at_distance
	if slow_down:
		acceleration = velocity.normalized() * -ACCELERATION

	velocity += acceleration * delta

	var speed_squared := velocity.length_squared()
	if speed_squared > MAX_SPEED*MAX_SPEED:
		velocity = velocity / sqrt(speed_squared) * MAX_SPEED

	global_position += velocity * delta

	rotation = rotate_toward(rotation, global_position.angle_to_point(target_position), min(1., delta) * ROTATION_SPEED)

func _update_color(ship_color: Color) -> void:
	%LineTrail.material.set_shader_parameter('color', ship_color)
