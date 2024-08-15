extends Node2D
class_name ShipFleet

@export var departed_at: int

@export var arrives_at: int

@export var from: Vector2

@export var to: Vector2

@export var count: int :
	set(value):
		count = value
		_update_instances_count()

#region Graphics

var instances: Array[Node2D] = []

@export var ship_template: PackedScene

func _update_instances_count() -> void :
	var diff = count - instances.size()

	for i in range(diff):
		var j = float(1 + instances.size())
		var ship := ship_template.instantiate()
		add_child(ship)
		ship.position = Vector2.from_angle(j) * j * 2.
		instances.push_back(ship)

	for i in range(-diff):
		var ship: Node2D = instances.pop_back()
		remove_child(ship)

#endregion
