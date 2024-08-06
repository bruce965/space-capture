extends Node2D

@export_group("Planet")

## Size of the planet in pixels.
@export_range(30, 500, .001, "or_greater", "or_less") var size: float = 100 :
	set(value):
		scale = value * Vector2.ONE
		material.set_shader_parameter('size', value)

# Rotation speed of the planet in rad/sec.
@export_range(0, 1, .001, "or_greater", "or_less") var rotation_speed: float = 0.05 :
	set(value):
		material.set_shader_parameter('rotationSpeed', value)

@export_group("Weather")

# Size of clouds between 0 (no clouds) and 1 (covered in clouds completely).
@export_range(0, 1, .001) var clouds_size: float = 0.05 :
	set(value):
		material.set_shader_parameter('cloudsSize', value)

# Density of clouds between 0 (very thin) and 1 (very thick).
@export_range(0, 1, .001) var cloud_density: float = 0.22 :
	set(value):
		material.set_shader_parameter('cloudsDensity', value)

# How often clouds change shape.
@export_range(0, 1, .001, "or_greater") var cloud_turbulence: float = 0.01 :
	set(value):
		material.set_shader_parameter('cloudsTurbulence', value)

# Wind speed in rad/sec.
@export_range(0, 1, .001, "or_greater") var wind_speed: float = 0.22 :
	set(value):
		material.set_shader_parameter('windSpeed', value)

@export_group("Atmosphere")

## Size of the atmosphere halo around the planet.
@export_range(0, 1, .001) var atmosphere_size: float = .3 :
	set(value):
		material.set_shader_parameter('atmosphereSize', value)

## Color of the atmosphere halo around the planet.
@export var atmosphere_color: Color = Color(0., .3, 1., .3) :
	set(value):
		material.set_shader_parameter('atmosphereColor', value)
