extends Node3D

const NPC_SCENE := preload("res://scenes/entities/npc.tscn")

func _ready() -> void:
	_build_center_square()
	_build_houses()
	_spawn_npcs()

func _build_center_square() -> void:
	var square := MeshInstance3D.new()
	var square_mesh := PlaneMesh.new()
	square_mesh.size = Vector2(18, 18)
	square.mesh = square_mesh
	square.rotation_degrees = Vector3(-90, 0, 0)
	square.position = Vector3(0, 0.02, 32)
	var square_material := StandardMaterial3D.new()
	square_material.albedo_color = Color(0.56, 0.43, 0.22, 1.0)
	square_material.roughness = 1.0
	square.material_override = square_material
	add_child(square)

	var forge_light := OmniLight3D.new()
	forge_light.light_color = Color(1.0, 0.56, 0.28, 1.0)
	forge_light.light_energy = 1.4
	forge_light.omni_range = 12.0
	forge_light.position = Vector3(0, 2.8, 32)
	add_child(forge_light)

func _build_houses() -> void:
	for offset in [-10, 10]:
		var house := Node3D.new()
		house.position = Vector3(offset, 0, 28)
		add_child(house)

		var body := MeshInstance3D.new()
		var body_mesh := BoxMesh.new()
		body_mesh.size = Vector3(5.5, 3.2, 4.2)
		body.mesh = body_mesh
		body.position = Vector3(0, 1.6, 0)
		var body_material := StandardMaterial3D.new()
		body_material.albedo_color = Color(0.8, 0.74, 0.62, 1.0)
		body_material.roughness = 0.9
		body.material_override = body_material
		house.add_child(body)

		var roof := MeshInstance3D.new()
		var roof_mesh := BoxMesh.new()
		roof_mesh.size = Vector3(6.1, 1.0, 4.6)
		roof.mesh = roof_mesh
		roof.position = Vector3(0, 3.45, 0)
		roof.rotation_degrees = Vector3(0, 0, 45)
		var roof_material := StandardMaterial3D.new()
		roof_material.albedo_color = Color(0.43, 0.27, 0.17, 1.0)
		roof_material.roughness = 1.0
		roof.material_override = roof_material
		house.add_child(roof)

func _spawn_npcs() -> void:
	var location_positions := {
		"marens_house": Vector3(-4, 0, 34),
		"market_square": Vector3(4, 0, 32),
		"forge": Vector3(10, 0, 30),
		"lottes_house": Vector3(-10, 0, 30),
		"forest_edge": Vector3(0, 0, 22),
	}

	for npc in AshenveilDataStore.get_all_npcs():
		var npc_id := str(npc.get("id", ""))
		var location := str(npc.get("location", ""))
		var npc_node := NPC_SCENE.instantiate()
		npc_node.call("configure_npc", npc_id)
		npc_node.position = location_positions.get(location, Vector3.ZERO)
		add_child(npc_node)
