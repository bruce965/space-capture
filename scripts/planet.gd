class_name Planet
extends Resource

enum PlanetType {
	TERRESTRIAL,
}

@export var location: Vector2

@export var type: PlanetType
