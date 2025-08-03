shader_type canvas_item;

// Max 1 star for each cell, star size cannot be bigger than cell size.
const float CELL_SIZE = 8.;
const float MIN_STAR_SIZE = 1.;
const float MAX_STAR_SIZE = 6.;
const float START_DENSITY = .15; // Between 0 and 1.

const float MIN_STAR_BRIGHTNESS = .10;
const float MAX_STAR_BRIGHTNESS = 1.;

const bool ALIGN_TO_PIXEL = true;

const float INV_CELL_SIZE = 1. / CELL_SIZE;
const float INV_MIN_STAR_SIZE = 1. / MIN_STAR_SIZE;
const float INV_MAX_STAR_SIZE = 1. / MAX_STAR_SIZE;

vec3 srgb_to_linear(vec3 col) {
	return pow(col, vec3(2.2));
}

vec3 linear_to_srgb(vec3 col) {
	return pow(col, vec3(1. / 2.2));
}

vec4 blend_additive(vec4 bg, vec4 fg) {
	vec4 dst = vec4(srgb_to_linear(bg.rgb), bg.a);
	vec4 src = vec4(srgb_to_linear(fg.rgb), fg.a);
	// glBlendEquation(GL_FUNC_ADD)
	// glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA)
	vec4 res = src * src.a + dst * (1. - src.a);
	return vec4(linear_to_srgb(res.rgb), res.a);
}

float rand(vec2 co){
	return fract(sin(dot(co, vec2(12.9898, 78.233))) * 43758.5453);
}

void fragment() {
	vec2 center_cell;
	vec2 pos_inside_center_cell = modf(FRAGCOORD.xy * INV_CELL_SIZE, center_cell);

	vec4 color = vec4(0., 0., 0., 0.);

	for (float y = -1.; y < 1.5; y++) {
		for (float x = -1.; x < 1.5; x++) {
			vec2 cell = center_cell + vec2(x, y);
			vec2 pos_inside_cell = pos_inside_center_cell - vec2(x, y);
			
			bool cell_has_star = rand(cell * .0101) < START_DENSITY;

			if (!cell_has_star)
				continue;

			vec2 star_pos_inside_cell = vec2(rand(cell * .0112), rand(cell * .0123));
			if (ALIGN_TO_PIXEL)
				star_pos_inside_cell = floor(star_pos_inside_cell * CELL_SIZE) * INV_CELL_SIZE + INV_CELL_SIZE/2.; // Align to pixel.

			vec2 relative_pos = pos_inside_cell - star_pos_inside_cell;
			float dist_to_star = (cell_has_star ? 0. : 999.) + length(relative_pos);
			float inv_star_size = mix(INV_MIN_STAR_SIZE, INV_MAX_STAR_SIZE, rand(cell * .0134));

			float adjusted_shape = dist_to_star + min(abs(relative_pos.x), abs(relative_pos.y));
			float pos_brightness = max(0., 1. - adjusted_shape * CELL_SIZE * inv_star_size);
			float star_brightness = mix(MIN_STAR_BRIGHTNESS, MAX_STAR_BRIGHTNESS, rand(cell * 0.145));

			vec2 rg = vec2(rand(cell * .0156), rand(cell * .0167));
			vec3 star_color = vec3(rg, max(0., 1. - rg.r - rg.g));
			star_color = mix(vec3(1.), star_color, .3);

			color = blend_additive(color, vec4(star_color, pos_brightness * star_brightness));
		}
	}

	COLOR = vec4(color.rgb, min(1., color.a));
}
