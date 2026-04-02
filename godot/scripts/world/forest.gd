extends Node3D

const BOAR_SCENE := preload("res://scenes/entities/boar.tscn")
const TREE_POSITIONS := [
	Vector3(-24, 0, -18),
	Vector3(-18, 0, -12),
	Vector3(16, 0, -20),
	Vector3(22, 0, -8),
	Vector3(-10, 0, 14),
	Vector3(10, 0, 18),
]

func _ready() -> void:
	_build_canopy_line()
	_build_forest_floor()
	_spawn_boars()

func _build_canopy_line() -> void:
	for position in TREE_POSITIONS:
		var tree := Node3D.new()
		tree.position = position
		add_child(tree)

		var trunk := MeshInstance3D.new()
		var trunk_mesh := CylinderMesh.new()
		trunk_mesh.top_radius = 0.12
		trunk_mesh.bottom_radius = 0.18
		trunk_mesh.height = 3.2
		trunk_mesh.radial_segments = 12
		trunk.mesh = trunk_mesh
		trunk.position = Vector3(0, 1.6, 0)
		var trunk_material := StandardMaterial3D.new()
		trunk_material.albedo_color = Color(0.38, 0.25, 0.16, 1.0)
		trunk_material.roughness = 0.98
		trunk.material_override = trunk_material
		tree.add_child(trunk)

		var canopy := MeshInstance3D.new()
		var canopy_mesh := SphereMesh.new()
		canopy_mesh.radius = 1.5
		canopy_mesh.height = 2.2
		canopy.mesh = canopy_mesh
		canopy.position = Vector3(0, 3.7, 0)
		var canopy_material := StandardMaterial3D.new()
		canopy_material.albedo_color = Color(0.23, 0.36, 0.2, 1.0)
		canopy_material.roughness = 1.0
		canopy.material_override = canopy_material
		tree.add_child(canopy)

func _build_forest_floor() -> void:
	var path := MeshInstance3D.new()
	var path_mesh := BoxMesh.new()
	path_mesh.size = Vector3(6, 0.06, 28)
	path.mesh = path_mesh
	path.position = Vector3(0, 0.02, -2)
	var path_material := StandardMaterial3D.new()
	path_material.albedo_color = Color(0.48, 0.35, 0.18, 1.0)
	path_material.roughness = 1.0
	path.material_override = path_material
	add_child(path)

func _spawn_boars() -> void:
	for enemy_id in ["boar", "scarred_boar"]:
		var enemy: Dictionary = AshenveilDataStore.get_enemy(enemy_id)
		var boar := BOAR_SCENE.instantiate()
		boar.set("enemy_id", enemy_id)
		var mesh_data: Dictionary = enemy.get("mesh", {})
		var scale_data: Array = mesh_data.get("scale", [1.2, 0.6, 0.7])
		boar.scale = Vector3(float(scale_data[0]), float(scale_data[1]), float(scale_data[2]))
		boar.position = Vector3(-6 if enemy_id == "boar" else 7, 0, -8 if enemy_id == "boar" else -14)
		add_child(boar)
