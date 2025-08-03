class_name LineTrail2D
extends Line2D

## Leave a trail behind this node.
@export var tracking: Node2D

## Minimum distance before adding a new point to the trail line.
## 0 = always add a new point on each frame.
@export var min_distance: float = 1.

## Maximum length of the trail before starting to trim.
## 0 = no length limit.
@export var max_length: float = 0.

## Maximum lifetime of the trail (in seconds) before starting to trim.
## 0 = no lifetime limit.
@export var max_lifetime: float = 3.

# Each index represents the remaining time in seconds before each point is
# removed from the line, starting to count after the previous point is removed.
# As an example: [1, 2, 2] means that the first point will be removed in 1
# second; the second point 2 seconds after the first (so 3 seconds from now);
# and the third point 2 seconds after the second (so 5 seconds from now).
var _points_life := PackedFloat32Array()

func _ready() -> void:
	clear_points()

func _process(delta: float) -> void:
	global_transform = Transform2D.IDENTITY

	var new_position := tracking.global_position

	# Add new points.
	var maybe_exceeds_max_length := false
	if _points_life.size() == 0:
		_add_point(new_position)
	else:
		var last_point := get_point_position(_points_life.size() - 1)
		var distance_sqr := (new_position - last_point).length_squared()
		if distance_sqr >= min_distance * min_distance:
			_add_point(new_position)
			maybe_exceeds_max_length = true

	# Reduce life and remove expired points.
	var remove_count := 0
	if max_lifetime != 0.:
		_decrease_life(delta)
		remove_count += _count_expired_points(remove_count)

	# Check if line length exceeds max length.
	if maybe_exceeds_max_length and max_length != 0.:
		remove_count += _count_overlength_points(remove_count)

	# Remove expired or overlength points.
	if remove_count > 0:
		_remove_oldest_points(remove_count)

func _add_point(point_position: Vector2) -> void:
	add_point(point_position)
	
	var last_point_points_life := 0.
	for time in _points_life:
		last_point_points_life += time
	
	_points_life.append(max_lifetime - last_point_points_life)

func _remove_oldest_points(remove_count: int) -> void:
	if remove_count > 0:
		points = points.slice(remove_count)
		_points_life = _points_life.slice(remove_count)

func _decrease_life(delta: float, start_at := 0) -> void:
	var i := start_at
	while _points_life.size() > i and delta > 0.:
		var sub = minf(delta, _points_life[i])
		_points_life[i] -= sub
		delta -= sub
		i += 1

func _count_expired_points(start_at := 0) -> int:
	var i := start_at
	while i < _points_life.size() and _points_life[i] <= 0.:
		i += 1

	return i - start_at

func _count_overlength_points(start_at := 0) -> int:
	var length := 0.

	var all_points := points
	var previous_point := all_points[start_at]
	for i in range(start_at + 1, all_points.size()):
		var point := all_points[i]
		length += (point - previous_point).length()
		previous_point = point

	var overlength_count := 0
	while length > max_length and overlength_count < all_points.size() - 1:
		var removed_point = all_points[overlength_count]
		var next_point = all_points[overlength_count + 1]
		length -= (removed_point - next_point).length()
		overlength_count += 1

	return overlength_count
