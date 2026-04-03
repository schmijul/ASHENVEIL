shader_type spatial;
render_mode cull_back, diffuse_burley, specular_schlick_ggx;

uniform vec4 grass_color : source_color = vec4(0.28, 0.38, 0.22, 1.0);
uniform vec4 moss_color : source_color = vec4(0.22, 0.31, 0.18, 1.0);
uniform vec4 soil_color : source_color = vec4(0.42, 0.33, 0.23, 1.0);
uniform vec4 rock_color : source_color = vec4(0.37, 0.36, 0.34, 1.0);
uniform float roughness_value : hint_range(0.0, 1.0) = 0.94;
uniform float noise_scale = 0.06;
uniform float slope_sharpness = 2.2;
uniform float tex_scale = 0.075;
uniform sampler2D tex_ground_diff;
uniform sampler2D tex_ground_rough;
uniform sampler2D tex_ground_norm;
uniform sampler2D tex_leaves_diff;
uniform sampler2D tex_leaves_rough;
uniform sampler2D tex_dirt_diff;
uniform sampler2D tex_dirt_rough;
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
	float broad_noise = noise2d(world_pos.xz * 0.012);
	float breakup_noise = noise2d(world_pos.xz * 0.24);
	float valley_mask = clamp(1.0 - h, 0.0, 1.0);
	float path_center = sin(world_pos.z * 0.06) * 4.0;
	float path_mask = clamp(1.0 - abs(world_pos.x - path_center) / 4.9, 0.0, 1.0);
	float shoulder_mask = clamp(1.0 - abs(abs(world_pos.x - path_center) - 4.8) / 2.2, 0.0, 1.0);
	vec2 uv = world_pos.xz * tex_scale;

	vec3 ground_diff = texture(tex_ground_diff, uv).rgb;
	float ground_rough = texture(tex_ground_rough, uv).r;
	vec3 ground_nrm = texture(tex_ground_norm, uv).rgb * 2.0 - 1.0;
	vec3 leaves_diff = texture(tex_leaves_diff, uv * 0.8 + vec2(0.15, -0.22)).rgb;
	float leaves_rough = texture(tex_leaves_rough, uv * 0.8 + vec2(0.15, -0.22)).r;
	vec3 dirt_diff = texture(tex_dirt_diff, uv * 0.9).rgb;
	float dirt_rough = texture(tex_dirt_rough, uv * 0.9).r;

	vec3 base = mix(moss_color.rgb * 0.82, grass_color.rgb, clamp(h * 0.92 + broad_noise * 0.22, 0.0, 1.0));
	base = mix(base, soil_color.rgb * 0.94, clamp((noise - 0.38) * 1.45 + valley_mask * 0.22, 0.0, 1.0));
	base = mix(base, rock_color.rgb * 0.86, clamp(slope * 1.25, 0.0, 1.0));
	base += (macro_noise - 0.5) * 0.055;
	base += (breakup_noise - 0.5) * 0.022;
	base *= mix(vec3(0.82, 0.86, 0.78), vec3(1.06, 1.02, 0.97), h);
	base = mix(base, ground_diff * vec3(0.86, 0.90, 0.82), 0.76);
	base = mix(base, leaves_diff * vec3(0.84, 0.80, 0.72), clamp((macro_noise - 0.30) * 1.35 + shoulder_mask * 0.18, 0.0, 0.62));
	base = mix(base, dirt_diff * vec3(0.92, 0.84, 0.74), path_mask * 0.94);
	base = mix(base, soil_color.rgb * 0.88, shoulder_mask * 0.22);
	base *= 1.0 - slope * 0.16;

	ALBEDO = clamp(base, vec3(0.0), vec3(1.0));
	ROUGHNESS = clamp(mix(mix(ground_rough, leaves_rough, 0.34), dirt_rough, path_mask) * roughness_value, 0.0, 1.0);
	METALLIC = 0.0;
	NORMAL = normalize(mix(vec3(0.0, 0.0, 1.0), ground_nrm, 0.62));
}
