shader_type canvas_item;

// Noise texture.
uniform sampler3D noise: repeat_enable;

// Planet.
uniform float size = 100.; // Size in pixels, not including atmosphere.
uniform float rotationSpeed = 0.05;
uniform sampler2D albedo; // Left side = water color (deep to shallow), right size = terrain color (low to high).

// Atmosphere.
uniform vec4 atmosphereColor: source_color = vec4(0., .3, 1., .3);
uniform float atmosphereSize = .3; // Range: [0, 1]

// Weather.
uniform float cloudsSize = 0.05;
uniform float cloudsDensity = 0.22;
uniform float cloudsTurbulence = 0.01;
uniform float windSpeed = 0.03;

// Zoom level assuming a planet of normalized size.
varying float zoom;

void vertex() {
	zoom = 4. / (size * CANVAS_MATRIX[0][0]);
}

vec4 blend(vec4 bg, vec4 fg) {
	float alpha = fg.a + bg.a * (1. - fg.a);
	vec3 color = (fg.rgb * fg.a + bg.rgb * bg.a * (1. - fg.a)) / alpha;
	return vec4(color, alpha);
}

vec4 drawPlanet(float r, vec3 p) {
	float angle = TIME * rotationSpeed;
	//mat3 rotX = mat3(vec3(1., 0., 0.), vec3(0., cos(angle), -sin(angle)), vec3(0., sin(angle), cos(angle)));
	mat3 rotY = mat3(vec3(cos(angle), 0., -sin(angle)), vec3(0., 1., 0.), vec3(sin(angle), 0., cos(angle)));
	//mat3 rotZ = mat3(vec3(cos(angle), -sin(angle), 0.), vec3(sin(angle), cos(angle), 0.), vec3(0., 0., 1.));
	
	float height = texture(noise, p * 0.3 * rotY).r;
	float rand = texture(noise, p * 0.3 * rotY + vec3(0.1, 0.2, 0.3)).r * .5 + .5;
	//float rand = texture(noise, p + vec3(n + TIME * 0.02)).r;
	
	vec3 color = texture(albedo, vec2(height, ((p.y + rand * sign(p.y)) * .5 + 1.) * .5)).rgb;
	vec3 lit = max(vec3(0.), color * dot(p, vec3(0., 0., 1.)));
	
	return vec4(lit, smoothstep(0., zoom, r));
}

vec4 drawClouds(float r, vec3 p) {
	float angle = TIME * 0.08;
	mat3 rotY = mat3(vec3(cos(angle), 0., -sin(angle)), vec3(0., 1., 0.), vec3(sin(angle), 0., cos(angle)));

	float angle2 = TIME * (rotationSpeed - windSpeed);
	mat3 rotY2 = mat3(vec3(cos(angle2), 0., -sin(angle2)), vec3(0., 1., 0.), vec3(sin(angle2), 0., cos(angle2)));

	float rand = texture(noise, p * 0.5 * rotY2 + vec3(0., 0., TIME * cloudsTurbulence)).r * .5 + .5;
	float rand2 = texture(noise, p * 0.8 * rotY2 + vec3(0., 0., rand)).r * .5 + .5;
	
	return vec4(vec3(1.), smoothstep(1. - cloudsDensity - cloudsSize, 1. - cloudsSize, rand2) * smoothstep(0., .3, r * 3.));
}

vec4 drawAtmosphere(float r) {
	return vec4(atmosphereColor.rgb, smoothstep(1.-atmosphereSize, 1., r + 1.) * atmosphereColor.a);
}

void fragment() {
	float r = length(UV - .25) * -8. + 1.;
	vec2 xy = UV * 8. - 2.;
	float z = sqrt(1. - xy.x*xy.x - xy.y*xy.y);
	vec3 p = vec3(xy, z);
	
	vec4 atmosphere = drawAtmosphere(r);
	vec4 planet = drawPlanet(r, p);
	vec4 clouds = drawClouds(r, p);
	COLOR = blend(blend(atmosphere, planet), clouds);
	
	// DEBUG
	//COLOR = vec4(p, smoothstep(0., zoom, r));
}
