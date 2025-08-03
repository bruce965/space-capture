shader_type canvas_item;
render_mode blend_add;

uniform vec3 color: source_color = vec3(1., 0., 1.);

void fragment() {
	COLOR = vec4(COLOR.r * color + COLOR.ggg, 1.);
}
