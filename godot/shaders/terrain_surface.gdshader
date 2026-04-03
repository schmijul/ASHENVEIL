shader_type spatial;
render_mode cull_back, diffuse_burley, specular_schlick_ggx;

uniform vec4 grass_color : source_color = vec4(0.28, 0.38, 0.22, 1.0);
uniform vec4 moss_color : source_color = vec4(0.22, 0.31, 0.18, 1.0);
uniform vec4 soil_color : source_color = vec4(0.42, 0.33, 0.23, 1.0);
uniform vec4 rock_color : source_color = vec4(0.37, 0.36, 0.34, 1.0);
uniform float roughness_value : hint_range(0.0, 1.0) = 0.94;
uniform float noise_scale = 0.06;
uniform float slope_sharpness = 2.2;
varying vec3 world_pos;

float hash12(vec2 p) {
	return fract(sin(dot(p, vec2(127.1, 311.7))) * 43758.5453123);
}

float noise2d(vec2 p) {
	vec2 i = floor(p);
	vec2 f = fract(p);
	float a = hash12(i);
	float b = hash12(i + vec2(1.0, 0.0));
	float c = hash12(i + vec2(0.0, 1.0));
	float d = hash12(i + vec2(1.0, 1.0));
	vec2 u = f * f * (3.0 - 2.0 * f);
	return mix(mix(a, b, u.x), mix(c, d, u.x), u.y);
}

void vertex() {
	world_pos = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xyz;
}

void fragment() {
	vec3 n = normalize(NORMAL);
	float slope = pow(clamp(1.0 - n.y, 0.0, 1.0), slope_sharpness);
	float h = clamp((world_pos.y + 4.0) / 8.0, 0.0, 1.0);
	float noise = noise2d(world_pos.xz * noise_scale);
	float macro_noise = noise2d(world_pos.xz * noise_scale * 0.35);

	vec3 base = mix(moss_color.rgb, grass_color.rgb, h);
	base = mix(base, soil_color.rgb, clamp((noise - 0.45) * 1.8, 0.0, 1.0));
	base = mix(base, rock_color.rgb, slope);
	base += (macro_noise - 0.5) * 0.05;

	ALBEDO = clamp(base, vec3(0.0), vec3(1.0));
	ROUGHNESS = roughness_value;
	METALLIC = 0.0;
}
