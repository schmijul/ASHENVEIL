extends Node3D

const ForestScene := preload("res://scenes/world/forest.tscn")
const VillageScene := preload("res://scenes/world/village.tscn")
const TERRAIN_SHADER := preload("res://shaders/terrain_surface.gdshader")
const SKY_HDR_PATH := "res://assets/hdri/forest_slope_2k.hdr"
const GROUND_DIFFUSE_PATH := "res://assets/textures/ground/forest_ground_04/diffuse.jpg"
const GROUND_ROUGHNESS_PATH := "res://assets/textures/ground/forest_ground_04/roughness.jpg"
const GROUND_NORMAL_PATH := "res://assets/textures/ground/forest_ground_04/normal_gl.jpg"
const LEAVES_DIFFUSE_PATH := "res://assets/textures/ground/leaves_forest_ground/diffuse.jpg"
const LEAVES_ROUGHNESS_PATH := "res://assets/textures/ground/leaves_forest_ground/roughness.jpg"
const DIRT_DIFFUSE_PATH := "res://assets/textures/ground/dirt_aerial_03/diffuse.jpg"
const DIRT_ROUGHNESS_PATH := "res://assets/textures/ground/dirt_aerial_03/roughness.jpg"
const TERRAIN_SIZE := 170.0
const TERRAIN_RESOLUTION := 96
const TERRAIN_HEIGHT := 4.8

func _ready() -> void:
	_build_environment()
	_build_ground()
	_build_distant_silhouette()
	_build_light_shafts()
	_build_ground_haze()
	_attach_world_blocks()

func _build_environment() -> void:
	var environment := WorldEnvironment.new()
	var env := Environment.new()
	env.background_mode = Environment.BG_SKY
	env.sky = _build_sky()
	env.ambient_light_source = Environment.AMBIENT_SOURCE_SKY
	env.ambient_light_color = Color(0.54, 0.52, 0.46, 1.0)
	env.ambient_light_energy = 0.64
	env.fog_enabled = true
	env.fog_light_color = Color(0.79, 0.73, 0.65, 1.0)
	env.fog_density = 0.0062
	env.fog_depth_begin = 14.0
	env.fog_depth_end = 120.0
	env.fog_height = -0.6
	env.fog_height_density = 0.05
	env.fog_sky_affect = 0.84
	env.volumetric_fog_enabled = true
	env.volumetric_fog_density = 0.052
	env.glow_enabled = true
	env.glow_strength = 0.30
	env.adjustment_enabled = true
	env.adjustment_brightness = 0.94
	env.adjustment_contrast = 1.18
	env.adjustment_saturation = 1.08
	env.tonemap_mode = Environment.TONE_MAPPER_ACES
	env.sdfgi_enabled = true
	environment.environment = env
	add_child(environment)

	var sun := DirectionalLight3D.new()
	sun.light_energy = 1.75
	sun.light_color = Color(1.0, 0.83, 0.60, 1.0)
	sun.rotation_degrees = Vector3(-24, 34, 0)
	sun.shadow_enabled = true
	sun.shadow_bias = 0.015
	sun.shadow_normal_bias = 0.85
	add_child(sun)

	var fill := DirectionalLight3D.new()
	fill.light_energy = 0.08
	fill.light_color = Color(0.47, 0.58, 0.66, 1.0)
	fill.rotation_degrees = Vector3(-16, -138, 0)
	add_child(fill)

	var bounce := OmniLight3D.new()
	bounce.light_color = Color(0.98, 0.75, 0.54, 1.0)
	bounce.light_energy = 0.26
	bounce.omni_range = 32.0
	bounce.position = Vector3(4.0, 6.5, 10.0)
	add_child(bounce)

func _build_sky() -> Sky:
	var sky := Sky.new()
	var hdr_texture := _load_texture_from_image(SKY_HDR_PATH)
	if hdr_texture != null:
		var pano := PanoramaSkyMaterial.new()
		pano.panorama = hdr_texture
		pano.energy_multiplier = 1.04
		sky.sky_material = pano
		return sky

	var procedural := ProceduralSkyMaterial.new()
	procedural.sky_top_color = Color(0.47, 0.61, 0.74, 1.0)
	procedural.sky_horizon_color = Color(0.93, 0.83, 0.69, 1.0)
	procedural.ground_horizon_color = Color(0.37, 0.31, 0.24, 1.0)
	procedural.ground_bottom_color = Color(0.16, 0.18, 0.14, 1.0)
	procedural.sun_angle_max = 40.0
	procedural.sun_curve = 0.18
	sky.sky_material = procedural
	return sky

func _load_texture_from_image(path: String) -> Texture2D:
	if not FileAccess.file_exists(path):
		return null
	var image := Image.new()
	var error := image.load(path)
	if error != OK:
		return null
	return ImageTexture.create_from_image(image)

func _build_ground() -> void:
	var ground_body := StaticBody3D.new()
	add_child(ground_body)

	var collision := CollisionShape3D.new()
	var shape := BoxShape3D.new()
	shape.size = Vector3(140, 1.0, 140)
	collision.shape = shape
	collision.position = Vector3(0, -0.5, 0)
	ground_body.add_child(collision)

	var mesh_instance := MeshInstance3D.new()
	mesh_instance.mesh = _build_terrain_mesh()
	var material := ShaderMaterial.new()
	material.shader = TERRAIN_SHADER
	material.set_shader_parameter("grass_color", Color(0.24, 0.38, 0.20, 1.0))
	material.set_shader_parameter("moss_color", Color(0.19, 0.28, 0.16, 1.0))
	material.set_shader_parameter("soil_color", Color(0.39, 0.29, 0.19, 1.0))
	material.set_shader_parameter("rock_color", Color(0.35, 0.35, 0.33, 1.0))
	material.set_shader_parameter("roughness_value", 0.98)
	material.set_shader_parameter("noise_scale", 0.055)
	material.set_shader_parameter("slope_sharpness", 2.9)
	material.set_shader_parameter("tex_scale", 0.072)
	material.set_shader_parameter("tex_ground_diff", _load_texture_from_image(GROUND_DIFFUSE_PATH))
	material.set_shader_parameter("tex_ground_rough", _load_texture_from_image(GROUND_ROUGHNESS_PATH))
	material.set_shader_parameter("tex_ground_norm", _load_texture_from_image(GROUND_NORMAL_PATH))
	material.set_shader_parameter("tex_leaves_diff", _load_texture_from_image(LEAVES_DIFFUSE_PATH))
	material.set_shader_parameter("tex_leaves_rough", _load_texture_from_image(LEAVES_ROUGHNESS_PATH))
	material.set_shader_parameter("tex_dirt_diff", _load_texture_from_image(DIRT_DIFFUSE_PATH))
	material.set_shader_parameter("tex_dirt_rough", _load_texture_from_image(DIRT_ROUGHNESS_PATH))
	mesh_instance.material_override = material
	ground_body.add_child(mesh_instance)

func _build_terrain_mesh() -> ArrayMesh:
	var noise := FastNoiseLite.new()
	noise.seed = 77
	noise.noise_type = FastNoiseLite.TYPE_SIMPLEX
	noise.frequency = 0.019
	noise.fractal_octaves = 4
	noise.fractal_gain = 0.55

	var mesh := ArrayMesh.new()
	var arrays: Array = []
	var vertices := PackedVector3Array()
	var colors := PackedColorArray()
	var normals := PackedVector3Array()
	var indices := PackedInt32Array()
	var step := TERRAIN_SIZE / float(TERRAIN_RESOLUTION)
	var half := TERRAIN_SIZE * 0.5

	for z in range(TERRAIN_RESOLUTION + 1):
		for x in range(TERRAIN_RESOLUTION + 1):
			var vx := -half + float(x) * step
			var vz := -half + float(z) * step
			var ridge := sin(vx * 0.065) * 0.55 + cos(vz * 0.055) * 0.34
			var n := noise.get_noise_2d(vx, vz)
			var path_center := sin(vz * 0.06) * 4.0
			var path_mask := float(clamp(1.0 - abs(vx - path_center) / 6.8, 0.0, 1.0))
			var path_depression := -path_mask * 0.42
			var shoulder_lift := exp(-pow((abs(vx - path_center) - 5.4) * 0.22, 2.0)) * 0.18
			var h := n * TERRAIN_HEIGHT + ridge + path_depression + shoulder_lift
			vertices.append(Vector3(vx, h, vz))
			normals.append(Vector3.UP)
			var forest_color := Color(0.24 + n * 0.02, 0.34 + n * 0.03, 0.20 + n * 0.01, 1.0)
			var path_color := Color(0.44, 0.34, 0.20, 1.0)
			colors.append(forest_color.lerp(path_color, path_mask * 0.9))

	for z in range(TERRAIN_RESOLUTION):
		for x in range(TERRAIN_RESOLUTION):
			var tl := z * (TERRAIN_RESOLUTION + 1) + x
			var tr := tl + 1
			var bl := tl + TERRAIN_RESOLUTION + 1
			var br := bl + 1
			indices.append_array([tl, bl, tr, tr, bl, br])

	arrays.resize(Mesh.ARRAY_MAX)
	arrays[Mesh.ARRAY_VERTEX] = vertices
	arrays[Mesh.ARRAY_NORMAL] = normals
	arrays[Mesh.ARRAY_COLOR] = colors
	arrays[Mesh.ARRAY_INDEX] = indices
	mesh.add_surface_from_arrays(Mesh.PRIMITIVE_TRIANGLES, arrays)
	return mesh

func _build_distant_silhouette() -> void:
	var ring := Node3D.new()
	add_child(ring)
	var rng := RandomNumberGenerator.new()
	rng.seed = 917
	for i in range(38):
		var m := MeshInstance3D.new()
		var hill := SphereMesh.new()
		hill.radius = rng.randf_range(10.0, 19.0)
		hill.height = rng.randf_range(12.0, 24.0)
		m.mesh = hill
		var a := float(i) / 38.0 * TAU
		var r := rng.randf_range(88.0, 122.0)
		m.position = Vector3(cos(a) * r, rng.randf_range(3.0, 10.0), sin(a) * r - 8.0)
		m.scale = Vector3(rng.randf_range(1.0, 1.8), rng.randf_range(0.45, 1.1), rng.randf_range(1.0, 1.8))
		var mat := StandardMaterial3D.new()
		mat.albedo_color = Color(
			rng.randf_range(0.18, 0.27),
			rng.randf_range(0.24, 0.33),
			rng.randf_range(0.23, 0.30),
			1.0
		)
		mat.roughness = 1.0
		mat.shading_mode = BaseMaterial3D.SHADING_MODE_PER_PIXEL
		m.material_override = mat
		ring.add_child(m)

func _build_light_shafts() -> void:
	for i in range(4):
		var shaft := MeshInstance3D.new()
		var quad := QuadMesh.new()
		quad.size = Vector2(12.0 + float(i) * 2.0, 28.0 + float(i) * 4.0)
		shaft.mesh = quad
		shaft.position = Vector3(-10.0 + float(i) * 7.5, 11.0 + float(i) * 1.2, -6.0 + float(i) * 10.0)
		shaft.rotation_degrees = Vector3(-62.0, 26.0, 0.0)
		var mat := StandardMaterial3D.new()
		mat.transparency = BaseMaterial3D.TRANSPARENCY_ALPHA
		mat.cull_mode = BaseMaterial3D.CULL_DISABLED
		mat.shading_mode = BaseMaterial3D.SHADING_MODE_UNSHADED
		mat.no_depth_test = true
		mat.albedo_color = Color(1.0, 0.83, 0.58, 0.055 - float(i) * 0.008)
		mat.emission_enabled = true
		mat.emission = Color(1.0, 0.80, 0.56, 1.0)
		mat.emission_energy_multiplier = 0.20
		shaft.material_override = mat
		add_child(shaft)

func _build_ground_haze() -> void:
	var haze := GPUParticles3D.new()
	haze.amount = 140
	haze.lifetime = 10.0
	haze.preprocess = 6.0
	haze.position = Vector3(0.0, 0.8, 8.0)
	var draw_pass := QuadMesh.new()
	draw_pass.size = Vector2(0.7, 0.7)
	haze.draw_pass_1 = draw_pass
	var process := ParticleProcessMaterial.new()
	process.emission_shape = ParticleProcessMaterial.EMISSION_SHAPE_BOX
	process.emission_box_extents = Vector3(26.0, 1.2, 36.0)
	process.direction = Vector3(0.02, 0.18, 0.0)
	process.spread = 18.0
	process.gravity = Vector3(0.0, 0.02, 0.0)
	process.initial_velocity_min = 0.02
	process.initial_velocity_max = 0.12
	process.scale_min = 0.35
	process.scale_max = 1.15
	process.color = Color(0.93, 0.84, 0.72, 0.12)
	haze.process_material = process
	var haze_mat := StandardMaterial3D.new()
	haze_mat.transparency = BaseMaterial3D.TRANSPARENCY_ALPHA
	haze_mat.cull_mode = BaseMaterial3D.CULL_DISABLED
	haze_mat.shading_mode = BaseMaterial3D.SHADING_MODE_UNSHADED
	haze_mat.albedo_color = Color(0.92, 0.84, 0.74, 0.08)
	haze_mat.emission_enabled = true
	haze_mat.emission = Color(0.98, 0.84, 0.65, 1.0)
	haze_mat.emission_energy_multiplier = 0.06
	haze.material_override = haze_mat
	add_child(haze)

func _attach_world_blocks() -> void:
	var forest := ForestScene.instantiate()
	forest.position = Vector3(0, 0, -4)
	add_child(forest)

	var village := VillageScene.instantiate()
	village.position = Vector3(0, 0, 0)
	add_child(village)
