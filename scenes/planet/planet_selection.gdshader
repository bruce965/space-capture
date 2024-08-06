shader_type canvas_item;

// Size in pixels.
uniform float size = 100.;

// Zoom level assuming a planet of normalized size.
varying float zoom;

void vertex() {
	zoom = 8. / (size * CANVAS_MATRIX[0][0]);
}

void fragment() {
	float r = length(UV - .5) * -8. + 2.;
	float d = abs(r);
	
	COLOR *= vec4(1., 1., 1., smoothstep(zoom, 0., d));
}
