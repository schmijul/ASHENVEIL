extends Node3D

const NPC_SCENE := preload("res://scenes/entities/npc.tscn")
const HOUSE_POSITIONS := [
	Vector3(-12, 0, 23),
	Vector3(-7, 0, 35),
	Vector3(8, 0, 25),
	Vector3(13, 0, 36),
	Vector3(-2, 0, 43),
]

func _ready() -> void:
	_build_center_square()
	_build_paths()
	_build_houses()
	_build_props()
	_build_ground_details()
	_build_chimney_smoke()
	_build_window_lights()
	_spawn_npcs()

func _build_center_square() -> void:
	var square := MeshInstance3D.new()
	var square_mesh := PlaneMesh.new()
	square_mesh.size = Vector2(24, 22)
	square.mesh = square_mesh
	square.rotation_degrees = Vector3(-90, 0, 0)
	square.position = Vector3(0, 0.04, 32)
	var square_material := StandardMaterial3D.new()
	square_material.albedo_color = Color(0.55, 0.43, 0.24, 1.0)
	square_material.roughness = 0.94
	square.material_override = square_material
	add_child(square)

	var worn_ring := MeshInstance3D.new()
	var ring_mesh := PlaneMesh.new()
	ring_mesh.size = Vector2(18, 16)
	worn_ring.mesh = ring_mesh
	worn_ring.rotation_degrees = Vector3(-90, 0, 0)
	worn_ring.position = Vector3(0, 0.044, 32)
	var ring_material := StandardMaterial3D.new()
	ring_material.albedo_color = Color(0.45, 0.33, 0.20, 1.0)
	ring_material.roughness = 0.99
	worn_ring.material_override = ring_material
	add_child(worn_ring)

	var forge_light := OmniLight3D.new()
	forge_light.light_color = Color(1.0, 0.55, 0.30, 1.0)
	forge_light.light_energy = 2.1
	forge_light.omni_range = 17.0
	forge_light.position = Vector3(2, 3.1, 30)
	add_child(forge_light)

func _build_paths() -> void:
	var path_mesh := PlaneMesh.new()
	path_mesh.size = Vector2(8, 38)
	var path := MeshInstance3D.new()
	path.mesh = path_mesh
	path.rotation_degrees = Vector3(-90, 0, 0)
	path.position = Vector3(0, 0.041, 19)
	var mat := StandardMaterial3D.new()
	mat.albedo_color = Color(0.47, 0.35, 0.22, 1.0)
	mat.roughness = 0.97
	path.material_override = mat
	add_child(path)

func _build_houses() -> void:
	var rng := RandomNumberGenerator.new()
	rng.seed = 9017
	for offset in HOUSE_POSITIONS:
		var house := Node3D.new()
		house.position = offset
		house.rotation.y = rng.randf_range(-0.16, 0.16)
		add_child(house)

		var body := MeshInstance3D.new()
		var body_mesh := BoxMesh.new()
		var width := rng.randf_range(4.6, 6.6)
		var depth := rng.randf_range(3.8, 5.3)
		var height := rng.randf_range(2.9, 4.1)
		body_mesh.size = Vector3(width, height, depth)
		body.mesh = body_mesh
		body.position = Vector3(0, height * 0.5, 0)
		var body_material := StandardMaterial3D.new()
		body_material.albedo_color = Color(0.82, 0.76, 0.66, 1.0)
		body_material.roughness = 0.86
		body.material_override = body_material
		house.add_child(body)

		var foundation := MeshInstance3D.new()
		var foundation_mesh := BoxMesh.new()
		foundation_mesh.size = Vector3(width + 0.3, 0.24, depth + 0.3)
		foundation.mesh = foundation_mesh
		foundation.position = Vector3(0, 0.12, 0)
		var foundation_material := StandardMaterial3D.new()
		foundation_material.albedo_color = Color(0.47, 0.45, 0.41, 1.0)
		foundation_material.roughness = 1.0
		foundation.material_override = foundation_material
		house.add_child(foundation)

		var roof := MeshInstance3D.new()
		var roof_mesh := PrismMesh.new()
		roof_mesh.size = Vector3(width + 0.65, 1.45, depth + 0.65)
		roof.mesh = roof_mesh
		roof.position = Vector3(0, height + 0.72, 0)
		roof.rotation_degrees = Vector3(0, 90, 0)
		var roof_material := StandardMaterial3D.new()
		roof_material.albedo_color = Color(0.42, 0.28, 0.17, 1.0)
		roof_material.roughness = 0.98
		roof.material_override = roof_material
		house.add_child(roof)

		var chimney := MeshInstance3D.new()
		var chimney_mesh := BoxMesh.new()
		chimney_mesh.size = Vector3(0.55, rng.randf_range(1.1, 1.5), 0.55)
		chimney.mesh = chimney_mesh
		chimney.position = Vector3(rng.randf_range(-width * 0.2, width * 0.2), height + 1.0, rng.randf_range(-depth * 0.18, depth * 0.18))
		var chimney_material := StandardMaterial3D.new()
		chimney_material.albedo_color = Color(0.31, 0.29, 0.27, 1.0)
		chimney_material.roughness = 0.98
		chimney.material_override = chimney_material
		house.add_child(chimney)

		for beam in [-1.0, 1.0]:
			var post := MeshInstance3D.new()
			var post_mesh := BoxMesh.new()
			post_mesh.size = Vector3(0.18, height, 0.22)
			post.mesh = post_mesh
			post.position = Vector3(beam * (width * 0.42), height * 0.5, depth * 0.42)
			var post_mat := StandardMaterial3D.new()
			post_mat.albedo_color = Color(0.36, 0.24, 0.16, 1.0)
			post_mat.roughness = 0.94
			post.material_override = post_mat
			house.add_child(post)

		var door := MeshInstance3D.new()
		var door_mesh := BoxMesh.new()
		door_mesh.size = Vector3(0.88, 1.62, 0.12)
		door.mesh = door_mesh
		door.position = Vector3(0.0, 0.81, depth * 0.51 + 0.03)
		var door_material := StandardMaterial3D.new()
		door_material.albedo_color = Color(0.29, 0.20, 0.14, 1.0)
		door_material.roughness = 0.97
		door.material_override = door_material
		house.add_child(door)

func _build_props() -> void:
	var rng := RandomNumberGenerator.new()
	rng.seed = 811
	for i in range(6):
		_add_bench(Vector3(-12 + float(i) * 4.8, 0.2, 24 + sin(float(i) * 0.7) * 2.1), rng.randf_range(-0.3, 0.3))

	for i in range(4):
		_add_crate_stack(Vector3(-9 + float(i) * 5.0, 0.1, 27 + cos(float(i) * 0.65) * 2.3), i)

	for i in range(4):
		_add_lantern_post(Vector3(-9 + float(i) * 4.6, 1.3, 29 + cos(float(i) * 0.9) * 2.7))

	_add_market_stall(Vector3(-4.5, 0.0, 28.4), 0.18)
	_add_market_stall(Vector3(5.0, 0.0, 29.6), -0.12)
	_add_wagon(Vector3(-1.8, 0.02, 37.8), 0.42)
	_add_firewood_stack(Vector3(9.0, 0.05, 40.2))
	_add_firewood_stack(Vector3(-9.4, 0.05, 40.8))

func _build_ground_details() -> void:
	var rng := RandomNumberGenerator.new()
	rng.seed = 404

	for _i in range(120):
		var pebble := MeshInstance3D.new()
		var mesh := SphereMesh.new()
		mesh.radius = rng.randf_range(0.04, 0.11)
		mesh.height = mesh.radius * rng.randf_range(0.7, 1.3)
		pebble.mesh = mesh
		pebble.position = Vector3(rng.randf_range(-15.0, 15.0), 0.05, rng.randf_range(14.0, 46.0))
		pebble.scale = Vector3(rng.randf_range(0.8, 1.5), rng.randf_range(0.6, 1.1), rng.randf_range(0.8, 1.5))
		var mat := StandardMaterial3D.new()
		mat.albedo_color = Color(
			rng.randf_range(0.34, 0.43),
			rng.randf_range(0.30, 0.36),
			rng.randf_range(0.25, 0.31),
			1.0
		)
		mat.roughness = 1.0
		pebble.material_override = mat
		add_child(pebble)

	for _i in range(22):
		var hay := MeshInstance3D.new()
		var mesh := CylinderMesh.new()
		mesh.top_radius = rng.randf_range(0.12, 0.24)
		mesh.bottom_radius = mesh.top_radius
		mesh.height = rng.randf_range(0.12, 0.26)
		hay.mesh = mesh
		hay.position = Vector3(rng.randf_range(-13.0, 13.0), 0.12, rng.randf_range(16.0, 45.0))
		hay.rotation.y = rng.randf() * TAU
		var mat := StandardMaterial3D.new()
		mat.albedo_color = Color(
			rng.randf_range(0.70, 0.83),
			rng.randf_range(0.59, 0.71),
			rng.randf_range(0.30, 0.42),
			1.0
		)
		mat.roughness = 0.96
		hay.material_override = mat
		add_child(hay)

	for _i in range(14):
		var moss := MeshInstance3D.new()
		var moss_mesh := PlaneMesh.new()
		moss_mesh.size = Vector2(rng.randf_range(0.5, 1.5), rng.randf_range(0.4, 1.0))
		moss.mesh = moss_mesh
		moss.rotation_degrees = Vector3(-90, 0, 0)
		moss.position = Vector3(rng.randf_range(-14.0, 14.0), 0.043, rng.randf_range(15.0, 44.0))
		var moss_mat := StandardMaterial3D.new()
		moss_mat.albedo_color = Color(0.32, 0.42, 0.25, 1.0)
		moss_mat.roughness = 1.0
		moss.material_override = moss_mat
		add_child(moss)

func _build_chimney_smoke() -> void:
	var chimney_positions := [
		Vector3(-11.5, 3.9, 23.4),
		Vector3(-6.8, 3.9, 35.4),
		Vector3(8.1, 3.9, 25.1),
		Vector3(12.8, 3.9, 36.0),
	]
	for pos in chimney_positions:
		var particles := GPUParticles3D.new()
		particles.amount = 36
		particles.lifetime = 2.7
		particles.one_shot = false
		particles.preprocess = 0.8
		particles.position = pos
		var material := ParticleProcessMaterial.new()
		material.direction = Vector3(0.0, 1.0, 0.0)
		material.initial_velocity_min = 0.4
		material.initial_velocity_max = 1.0
		material.gravity = Vector3(0.0, 0.25, 0.0)
		material.scale_min = 0.18
		material.scale_max = 0.44
		material.color = Color(0.58, 0.56, 0.54, 0.55)
		material.turbulence_enabled = true
		material.turbulence_noise_scale = 0.7
		particles.process_material = material
		add_child(particles)

func _build_window_lights() -> void:
	var rng := RandomNumberGenerator.new()
	rng.seed = 2201
	for house_pos in HOUSE_POSITIONS:
		for side in [-1.0, 1.0]:
			var window := MeshInstance3D.new()
			var mesh := QuadMesh.new()
			mesh.size = Vector2(0.55, 0.75)
			window.mesh = mesh
			window.position = house_pos + Vector3(side * 2.45, 1.9, rng.randf_range(-1.3, 1.3))
			window.rotation.y = PI * 0.5
			var mat := StandardMaterial3D.new()
			mat.albedo_color = Color(0.98, 0.82, 0.55, 1.0)
			mat.emission_enabled = true
			mat.emission = Color(0.96, 0.68, 0.42, 1.0)
			mat.emission_energy_multiplier = 0.32
			mat.cull_mode = BaseMaterial3D.CULL_DISABLED
			window.material_override = mat
			add_child(window)

	for pos in [Vector3(-4.5, 2.7, 28.4), Vector3(5.1, 2.7, 29.6)]:
		var torch := OmniLight3D.new()
		torch.light_color = Color(1.0, 0.60, 0.35, 1.0)
		torch.light_energy = 0.28
		torch.omni_range = 4.2
		torch.position = pos
		add_child(torch)

func _add_lantern_post(pos: Vector3) -> void:
	var post := MeshInstance3D.new()
	var pole_mesh := CylinderMesh.new()
	pole_mesh.top_radius = 0.05
	pole_mesh.bottom_radius = 0.07
	pole_mesh.height = 2.6
	post.mesh = pole_mesh
	post.position = pos
	var pole_mat := StandardMaterial3D.new()
	pole_mat.albedo_color = Color(0.31, 0.21, 0.14, 1.0)
	pole_mat.roughness = 0.97
	post.material_override = pole_mat
	add_child(post)

	var lantern := MeshInstance3D.new()
	var lantern_mesh := BoxMesh.new()
	lantern_mesh.size = Vector3(0.18, 0.28, 0.18)
	lantern.mesh = lantern_mesh
	lantern.position = pos + Vector3(0, 1.45, 0)
	var lantern_mat := StandardMaterial3D.new()
	lantern_mat.albedo_color = Color(0.96, 0.78, 0.48, 1.0)
	lantern_mat.emission_enabled = true
	lantern_mat.emission = Color(1.0, 0.66, 0.42, 1.0)
	lantern_mat.emission_energy_multiplier = 0.28
	lantern.material_override = lantern_mat
	add_child(lantern)

func _add_bench(pos: Vector3, rotation_y: float) -> void:
	var bench := Node3D.new()
	bench.position = pos
	bench.rotation.y = rotation_y
	add_child(bench)

	var seat := MeshInstance3D.new()
	var seat_mesh := BoxMesh.new()
	seat_mesh.size = Vector3(1.7, 0.12, 0.38)
	seat.mesh = seat_mesh
	seat.position = Vector3(0, 0.5, 0)
	seat.material_override = _wood_material(Color(0.39, 0.25, 0.16, 1.0))
	bench.add_child(seat)

	for side in [-1.0, 1.0]:
		var leg := MeshInstance3D.new()
		var leg_mesh := BoxMesh.new()
		leg_mesh.size = Vector3(0.1, 0.5, 0.1)
		leg.mesh = leg_mesh
		leg.position = Vector3(side * 0.65, 0.24, 0)
		leg.material_override = _wood_material(Color(0.31, 0.20, 0.14, 1.0))
		bench.add_child(leg)

func _add_crate_stack(pos: Vector3, variant: int) -> void:
	var stack := Node3D.new()
	stack.position = pos
	add_child(stack)
	for i in range(2 + variant % 2):
		var crate := MeshInstance3D.new()
		var crate_mesh := BoxMesh.new()
		crate_mesh.size = Vector3(0.68 + float(variant) * 0.06, 0.5, 0.68)
		crate.mesh = crate_mesh
		crate.position = Vector3(0, 0.25 + float(i) * 0.5, 0)
		crate.rotation.y = (float(variant) * 0.4) + float(i) * 0.15
		crate.material_override = _wood_material(Color(0.43, 0.29, 0.18, 1.0))
		stack.add_child(crate)

func _add_market_stall(pos: Vector3, tilt: float) -> void:
	var stall := Node3D.new()
	stall.position = pos
	stall.rotation.y = tilt
	add_child(stall)

	var base := MeshInstance3D.new()
	var base_mesh := BoxMesh.new()
	base_mesh.size = Vector3(1.7, 0.12, 1.2)
	base.mesh = base_mesh
	base.position = Vector3(0, 0.18, 0)
	base.material_override = _wood_material(Color(0.37, 0.24, 0.15, 1.0))
	stall.add_child(base)

	for x in [-0.72, 0.72]:
		for z in [-0.46, 0.46]:
			var post := MeshInstance3D.new()
			var post_mesh := CylinderMesh.new()
			post_mesh.top_radius = 0.05
			post_mesh.bottom_radius = 0.06
			post_mesh.height = 1.45
			post.mesh = post_mesh
			post.position = Vector3(x, 0.9, z)
			post.material_override = _wood_material(Color(0.31, 0.20, 0.14, 1.0))
			stall.add_child(post)

	var roof := MeshInstance3D.new()
	var roof_mesh := PrismMesh.new()
	roof_mesh.size = Vector3(1.95, 0.75, 1.45)
	roof.mesh = roof_mesh
	roof.position = Vector3(0, 1.55, 0)
	roof.material_override = _cloth_material(Color(0.72, 0.54, 0.30, 1.0))
	stall.add_child(roof)

func _add_wagon(pos: Vector3, rotation_y: float) -> void:
	var wagon := Node3D.new()
	wagon.position = pos
	wagon.rotation.y = rotation_y
	add_child(wagon)

	var body := MeshInstance3D.new()
	var body_mesh := BoxMesh.new()
	body_mesh.size = Vector3(2.0, 0.55, 1.1)
	body.mesh = body_mesh
	body.position = Vector3(0, 0.58, 0)
	body.material_override = _wood_material(Color(0.40, 0.26, 0.16, 1.0))
	wagon.add_child(body)

	for side in [-1.0, 1.0]:
		var wheel := MeshInstance3D.new()
		var wheel_mesh := CylinderMesh.new()
		wheel_mesh.top_radius = 0.45
		wheel_mesh.bottom_radius = 0.45
		wheel_mesh.height = 0.18
		wheel_mesh.radial_segments = 12
		wheel.mesh = wheel_mesh
		wheel.position = Vector3(side * 0.95, 0.26, 0)
		wheel.rotation_degrees = Vector3(90, 0, 0)
		wheel.material_override = _wood_material(Color(0.24, 0.16, 0.12, 1.0))
		wagon.add_child(wheel)

func _add_firewood_stack(pos: Vector3) -> void:
	var stack := Node3D.new()
	stack.position = pos
	add_child(stack)
	for i in range(6):
		var log := MeshInstance3D.new()
		var log_mesh := CylinderMesh.new()
		log_mesh.top_radius = 0.08 + float(i % 2) * 0.015
		log_mesh.bottom_radius = log_mesh.top_radius * 1.05
		log_mesh.height = 0.8 + float(i % 3) * 0.12
		log_mesh.radial_segments = 8
		log.mesh = log_mesh
		log.position = Vector3(float(i % 3) * 0.22 - 0.22, 0.12 + float(i / 3) * 0.16, float(i % 2) * 0.18 - 0.08)
		log.rotation = Vector3(PI * 0.5, 0.0, float(i) * 0.3)
		log.material_override = _wood_material(Color(0.32, 0.22, 0.15, 1.0))
		stack.add_child(log)

func _wood_material(color: Color) -> StandardMaterial3D:
	var material := StandardMaterial3D.new()
	material.albedo_color = color
	material.roughness = 0.96
	return material

func _cloth_material(color: Color) -> StandardMaterial3D:
	var material := StandardMaterial3D.new()
	material.albedo_color = color
	material.roughness = 0.92
	return material

func _spawn_npcs() -> void:
	var location_positions := {
		"marens_house": Vector3(-4, 0, 34),
		"market_square": Vector3(4, 0, 32),
		"forge": Vector3(10, 0, 30),
		"lottes_house": Vector3(-10, 0, 30),
		"forest_edge": Vector3(0, 0, 22),
	}

	for npc in AshenveilDataStore.get_all_npcs():
		var npc_data: Dictionary = npc
		var npc_id: String = str(npc_data.get("id", ""))
		var location: String = str(npc_data.get("location", ""))
		var npc_node := NPC_SCENE.instantiate()
		npc_node.call("configure_npc", npc_id)
		npc_node.position = location_positions.get(location, Vector3.ZERO)
		add_child(npc_node)
