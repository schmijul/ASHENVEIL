extends Node3D

const ForestScene := preload("res://scenes/world/forest.tscn")
const VillageScene := preload("res://scenes/world/village.tscn")
const TERRAIN_SHADER := preload("res://shaders/terrain_surface.gdshader")
const TERRAIN_SIZE := 170.0
const TERRAIN_RESOLUTION := 96
const TERRAIN_HEIGHT := 3.8

func _ready() -> void:
	_build_environment()
	_build_ground()
	_build_distant_silhouette()
	_attach_world_blocks()

func _build_environment() -> void:
	var environment := WorldEnvironment.new()
	var env := Environment.new()
	env.background_mode = Environment.BG_SKY
	env.sky = _build_sky()
	env.ambient_light_source = Environment.AMBIENT_SOURCE_SKY
	env.ambient_light_color = Color(0.70, 0.68, 0.61, 1.0)
	env.ambient_light_energy = 0.95
	env.fog_enabled = true
	env.fog_light_color = Color(0.84, 0.79, 0.71, 1.0)
	env.fog_density = 0.0068
	env.volumetric_fog_enabled = true
	env.volumetric_fog_density = 0.052
	env.glow_enabled = true
	env.glow_strength = 0.34
	env.adjustment_enabled = true
	env.adjustment_brightness = 0.96
	env.adjustment_contrast = 1.12
	env.adjustment_saturation = 1.06
	env.tonemap_mode = Environment.TONE_MAPPER_ACES
	env.sdfgi_enabled = true
	environment.environment = env
	add_child(environment)

	var sun := DirectionalLight3D.new()
	sun.light_energy = 1.55
	sun.light_color = Color(1.0, 0.84, 0.64, 1.0)
	sun.rotation_degrees = Vector3(-37, 26, 0)
	sun.shadow_enabled = true
	sun.shadow_bias = 0.02
	sun.shadow_normal_bias = 1.1
	add_child(sun)

	var fill := DirectionalLight3D.new()
	fill.light_energy = 0.16
	fill.light_color = Color(0.52, 0.61, 0.69, 1.0)
	fill.rotation_degrees = Vector3(-22, -142, 0)
	add_child(fill)

func _build_sky() -> Sky:
	var sky := Sky.new()
	var material := ProceduralSkyMaterial.new()
	material.sky_top_color = Color(0.54, 0.66, 0.78, 1.0)
	material.sky_horizon_color = Color(0.89, 0.84, 0.74, 1.0)
	material.ground_horizon_color = Color(0.40, 0.35, 0.28, 1.0)
	material.ground_bottom_color = Color(0.18, 0.20, 0.16, 1.0)
	material.sun_angle_max = 40.0
	material.sun_curve = 0.13
	sky.sky_material = material
	return sky

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
	material.set_shader_parameter("roughness_value", 0.96)
	material.set_shader_parameter("noise_scale", 0.048)
	material.set_shader_parameter("slope_sharpness", 2.5)
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
			var ridge := sin(vx * 0.07) * 0.35 + cos(vz * 0.06) * 0.25
			var n := noise.get_noise_2d(vx, vz)
			var h := n * TERRAIN_HEIGHT + ridge
			vertices.append(Vector3(vx, h, vz))
			normals.append(Vector3.UP)
			var path_mask := float(clamp(1.0 - abs(vx - sin(vz * 0.06) * 4.0) / 7.0, 0.0, 1.0))
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
	for i in range(28):
		var m := MeshInstance3D.new()
		var hill := SphereMesh.new()
		hill.radius = rng.randf_range(9.0, 16.0)
		hill.height = rng.randf_range(10.0, 18.0)
		m.mesh = hill
		var a := float(i) / 28.0 * TAU
		var r := rng.randf_range(85.0, 110.0)
		m.position = Vector3(cos(a) * r, rng.randf_range(2.0, 7.0), sin(a) * r - 8.0)
		m.scale = Vector3(rng.randf_range(0.9, 1.5), rng.randf_range(0.5, 1.0), rng.randf_range(0.9, 1.5))
		var mat := StandardMaterial3D.new()
		mat.albedo_color = Color(
			rng.randf_range(0.26, 0.34),
			rng.randf_range(0.32, 0.40),
			rng.randf_range(0.30, 0.37),
			1.0
		)
		mat.roughness = 1.0
		m.material_override = mat
		ring.add_child(m)

func _attach_world_blocks() -> void:
	var forest := ForestScene.instantiate()
	forest.position = Vector3(0, 0, -4)
	add_child(forest)

	var village := VillageScene.instantiate()
	village.position = Vector3(0, 0, 0)
	add_child(village)
