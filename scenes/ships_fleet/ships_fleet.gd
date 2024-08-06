extends Node2D

var instances: Array[Node2D] = []

@export var ship_template: PackedScene

@export var count: int :
	set(value):
		count = value

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
