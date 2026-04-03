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

func _build_props() -> void:
	for i in range(9):
		var fence := MeshInstance3D.new()
		var mesh := BoxMesh.new()
		mesh.size = Vector3(2.2, 0.2, 0.16)
		fence.mesh = mesh
		fence.position = Vector3(-14 + float(i) * 3.3, 0.55, 17 + sin(float(i) * 0.6) * 2.0)
		var mat := StandardMaterial3D.new()
		mat.albedo_color = Color(0.37, 0.24, 0.16, 1.0)
		mat.roughness = 0.95
		fence.material_override = mat
		add_child(fence)

	for i in range(5):
		var lantern_pole := MeshInstance3D.new()
		var pole_mesh := CylinderMesh.new()
		pole_mesh.top_radius = 0.06
		pole_mesh.bottom_radius = 0.08
		pole_mesh.height = 2.6
		lantern_pole.mesh = pole_mesh
		lantern_pole.position = Vector3(-8 + float(i) * 4.0, 1.3, 29 + cos(float(i) * 0.9) * 2.7)
		var pole_mat := StandardMaterial3D.new()
		pole_mat.albedo_color = Color(0.32, 0.21, 0.14, 1.0)
		pole_mat.roughness = 0.96
		lantern_pole.material_override = pole_mat
		add_child(lantern_pole)

		var lantern := OmniLight3D.new()
		lantern.light_color = Color(1.0, 0.66, 0.44, 1.0)
		lantern.light_energy = 0.55
		lantern.omni_range = 7.0
		lantern.position = lantern_pole.position + Vector3(0, 0.95, 0)
		add_child(lantern)

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

	for _i in range(35):
		var hay := MeshInstance3D.new()
		var mesh := CylinderMesh.new()
		mesh.top_radius = rng.randf_range(0.18, 0.34)
		mesh.bottom_radius = mesh.top_radius
		mesh.height = rng.randf_range(0.16, 0.34)
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
