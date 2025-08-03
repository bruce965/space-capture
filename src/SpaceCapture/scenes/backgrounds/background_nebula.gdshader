shader_type canvas_item;

uniform sampler2D noise: repeat_enable;

void fragment() {
	float r = texture(noise, FRAGCOORD.xy * .0005 + .0).r;
	float g = texture(noise, FRAGCOORD.xy * .0005 + .1).r;
	float b = texture(noise, FRAGCOORD.xy * .0005 + .2).r;
	float a = texture(noise, FRAGCOORD.xy * .0002 + .3).r * .35;
	COLOR = vec4(r, g, b, a);
}
