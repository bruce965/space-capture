shader_type canvas_item;

uniform vec4 color: source_color = vec4(1., 0., 1., 1.);

varying float len;

void vertex() {
	len = length(MODEL_MATRIX[0]);
}

void fragment() {
	float col = texture(TEXTURE, UV * vec2(len, 1.) + vec2(TIME * -.2, 0.)).r;
	float fade = min(1., min(UV.x, 1. - UV.x) * len);
	
	COLOR = color * vec4(vec3(1.), col * fade);
}
