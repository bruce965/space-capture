shader_type canvas_item;

uniform sampler2D noise: repeat_enable;

float rand(vec2 co){
    return fract(sin(dot(co, vec2(12.9898, 78.233))) * 43758.5453);
}

void fragment() {
	vec3 nebula = vec3(texture(noise, FRAGCOORD.xy * .0005).r, texture(noise, FRAGCOORD.xy * .0005 + 0.1).r, texture(noise, FRAGCOORD.xy * .0005 + 0.2).r) * 0.05;
	vec3 star = (rand(FRAGCOORD.xy * .01) < 0.1 && rand(FRAGCOORD.xy * .02) < .01) ? vec3(rand(FRAGCOORD.xy * .03)) : vec3(0.);

	COLOR = vec4(nebula + star, 1.);
}
