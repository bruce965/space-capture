shader_type canvas_item;

float rand(vec2 co){
    return fract(sin(dot(co, vec2(12.9898, 78.233))) * 43758.5453);
}

void fragment() {
	if (rand(SCREEN_UV) > COLOR.a - abs(UV.y * 2. - 1.))
		discard;
	
	COLOR = vec4(1., 1., 1., COLOR.a);
}
