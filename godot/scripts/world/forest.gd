extends Node3D

const BOAR_SCENE := preload("res://scenes/entities/boar.tscn")
const TREE_MODEL_PATH := "res://assets/models/tree1.glb"
const TREE_MODEL_BASE_SCALE := 0.38
const FOREST_RADIUS := 58.0
const TREE_COUNT := 130
const BUSH_COUNT := 260
const ROCK_COUNT := 55
const GRASS_COUNT := 3200
const FLOWER_COUNT := 420
const LOG_COUNT := 75
const MUSHROOM_COUNT := 240

func _ready() -> void:
	_build_tree_layers()
	_build_bush_layer()
	_build_rocks()
	_build_logs()
	_build_forest_floor()
	_build_grass_layer()
	_build_flower_layer()
	_build_mushrooms()
	_spawn_boars()

func _build_tree_layers() -> void:
	var tree_scene_resource: Resource = load(TREE_MODEL_PATH)
	var tree_scene: PackedScene = tree_scene_resource as PackedScene
	var rng := RandomNumberGenerator.new()
	rng.seed = 4207
	for i in range(TREE_COUNT):
		var angle := rng.randf() * TAU
		var distance := sqrt(rng.randf()) * FOREST_RADIUS
		var position := Vector3(cos(angle) * distance, 0.0, sin(angle) * distance - 8.0)
		if abs(position.x - sin(position.z * 0.06) * 4.0) < 6.0 and position.z > -28.0 and position.z < 45.0:
			continue

		var tree: Node3D
		if tree_scene != null:
			var instance: Node = tree_scene.instantiate()
			tree = instance as Node3D
			if tree == null:
				tree = Node3D.new()
		else:
			tree = Node3D.new()
		tree.position = position
		tree.rotation.y = rng.randf() * TAU
		if tree_scene != null:
			tree.scale = Vector3.ONE * rng.randf_range(TREE_MODEL_BASE_SCALE * 0.9, TREE_MODEL_BASE_SCALE * 1.3)
		else:
			tree.scale = Vector3.ONE * rng.randf_range(0.9, 1.45)
		add_child(tree)

		if tree_scene != null:
			continue

		var trunk := MeshInstance3D.new()
		var trunk_mesh := CylinderMesh.new()
		trunk_mesh.top_radius = 0.14
		trunk_mesh.bottom_radius = 0.24
		trunk_mesh.height = rng.randf_range(3.0, 4.8)
		trunk_mesh.radial_segments = 14
		trunk.mesh = trunk_mesh
		trunk.position = Vector3(0, trunk_mesh.height * 0.5, 0)
		var trunk_material := StandardMaterial3D.new()
		trunk_material.albedo_color = Color(0.35, 0.24, 0.16, 1.0)
		trunk_material.roughness = 0.93
		trunk.material_override = trunk_material
		tree.add_child(trunk)

		if i % 5 == 0:
			_add_oak_canopy(tree, trunk_mesh.height, rng)
		else:
			_add_pine_canopy(tree, trunk_mesh.height)

func _add_pine_canopy(tree: Node3D, trunk_height: float) -> void:
	var canopy_material := StandardMaterial3D.new()
	canopy_material.albedo_color = Color(0.21, 0.34, 0.19, 1.0)
	canopy_material.roughness = 0.98
	for tier in range(3):
		var canopy := MeshInstance3D.new()
		var cone := CylinderMesh.new()
		cone.top_radius = 0.0
		cone.bottom_radius = 1.6 - float(tier) * 0.32
		cone.height = 1.7 - float(tier) * 0.22
		cone.radial_segments = 10
		canopy.mesh = cone
		canopy.position = Vector3(0, trunk_height - 0.4 + float(tier) * 0.82, 0)
		canopy.material_override = canopy_material
		tree.add_child(canopy)

func _add_oak_canopy(tree: Node3D, trunk_height: float, rng: RandomNumberGenerator) -> void:
	var canopy_material := StandardMaterial3D.new()
	canopy_material.albedo_color = Color(0.27, 0.40, 0.22, 1.0)
	canopy_material.roughness = 0.95
	for part in range(3):
		var canopy := MeshInstance3D.new()
		var sphere := SphereMesh.new()
		sphere.radius = rng.randf_range(0.9, 1.4)
		sphere.height = sphere.radius * 1.45
		canopy.mesh = sphere
		canopy.position = Vector3(
			rng.randf_range(-0.6, 0.6),
			trunk_height + 0.7 + float(part) * 0.45,
			rng.randf_range(-0.6, 0.6)
		)
		canopy.material_override = canopy_material
		tree.add_child(canopy)

func _build_bush_layer() -> void:
	var rng := RandomNumberGenerator.new()
	rng.seed = 9931
	for _i in range(BUSH_COUNT):
		var angle := rng.randf() * TAU
		var distance := sqrt(rng.randf()) * (FOREST_RADIUS + 10.0)
		var pos := Vector3(cos(angle) * distance, 0.0, sin(angle) * distance - 8.0)
		if abs(pos.x - sin(pos.z * 0.06) * 4.0) < 4.7 and pos.z > -30.0 and pos.z < 45.0:
			continue
		var bush := MeshInstance3D.new()
		var bush_mesh := SphereMesh.new()
		bush_mesh.radius = rng.randf_range(0.20, 0.48)
		bush_mesh.height = bush_mesh.radius * 1.2
		bush.mesh = bush_mesh
		bush.position = pos + Vector3(0.16, 0.16, 0.0)
		var mat := StandardMaterial3D.new()
		mat.albedo_color = Color(
			rng.randf_range(0.19, 0.31),
			rng.randf_range(0.33, 0.47),
			rng.randf_range(0.16, 0.28),
			1.0
		)
		mat.roughness = 0.97
		bush.material_override = mat
		add_child(bush)

func _build_rocks() -> void:
	var rng := RandomNumberGenerator.new()
	rng.seed = 3141
	for _i in range(ROCK_COUNT):
		var pos := Vector3(rng.randf_range(-58.0, 58.0), 0.0, rng.randf_range(-54.0, 48.0))
		var rock := MeshInstance3D.new()
		var mesh := SphereMesh.new()
		mesh.radius = rng.randf_range(0.35, 1.1)
		mesh.height = mesh.radius * rng.randf_range(0.85, 1.3)
		rock.mesh = mesh
		rock.position = pos + Vector3(0.22, 0.10, 0.0)
		rock.scale = Vector3(rng.randf_range(0.9, 1.4), rng.randf_range(0.6, 1.0), rng.randf_range(0.8, 1.5))
		var mat := StandardMaterial3D.new()
		mat.albedo_color = Color(
			rng.randf_range(0.30, 0.40),
			rng.randf_range(0.30, 0.37),
			rng.randf_range(0.28, 0.34),
			1.0
		)
		mat.roughness = 1.0
		rock.material_override = mat
		add_child(rock)

func _build_logs() -> void:
	var rng := RandomNumberGenerator.new()
	rng.seed = 2401
	for _i in range(LOG_COUNT):
		var log := MeshInstance3D.new()
		var mesh := CylinderMesh.new()
		mesh.top_radius = rng.randf_range(0.12, 0.22)
		mesh.bottom_radius = mesh.top_radius * rng.randf_range(0.95, 1.15)
		mesh.height = rng.randf_range(1.2, 3.4)
		mesh.radial_segments = 8
		log.mesh = mesh
		log.position = Vector3(rng.randf_range(-55.0, 55.0), 0.16, rng.randf_range(-56.0, 48.0))
		log.rotation = Vector3(PI * 0.5 + rng.randf_range(-0.2, 0.2), rng.randf() * TAU, rng.randf_range(-0.2, 0.2))
		var mat := StandardMaterial3D.new()
		mat.albedo_color = Color(
			rng.randf_range(0.27, 0.36),
			rng.randf_range(0.19, 0.25),
			rng.randf_range(0.14, 0.19),
			1.0
		)
		mat.roughness = 0.98
		log.material_override = mat
		add_child(log)

func _build_forest_floor() -> void:
	var path := MeshInstance3D.new()
	var path_mesh := PlaneMesh.new()
	path_mesh.size = Vector2(7.6, 70.0)
	path.mesh = path_mesh
	path.rotation_degrees = Vector3(-90, 0, 0)
	path.position = Vector3(0, 0.05, 6)
	var path_material := StandardMaterial3D.new()
	path_material.albedo_color = Color(0.44, 0.33, 0.21, 1.0)
	path_material.roughness = 0.98
	path.material_override = path_material
	add_child(path)

	for i in range(8):
		var patch := MeshInstance3D.new()
		var patch_mesh := PlaneMesh.new()
		patch_mesh.size = Vector2(3.0, 10.0)
		patch.mesh = patch_mesh
		patch.rotation_degrees = Vector3(-90, 0, 0)
		patch.position = Vector3(sin(float(i) * 0.8) * 2.6, 0.051, -27.0 + float(i) * 9.5)
		var patch_material := StandardMaterial3D.new()
		patch_material.albedo_color = Color(0.35, 0.48, 0.27, 1.0)
		patch_material.roughness = 1.0
		patch.material_override = patch_material
		add_child(patch)

func _build_grass_layer() -> void:
	var rng := RandomNumberGenerator.new()
	rng.seed = 781
	var quad := QuadMesh.new()
	quad.size = Vector2(0.15, 0.75)
	var material := StandardMaterial3D.new()
	material.albedo_color = Color(0.33, 0.47, 0.24, 1.0)
	material.roughness = 1.0
	material.cull_mode = BaseMaterial3D.CULL_DISABLED

	var mm := MultiMesh.new()
	mm.transform_format = MultiMesh.TRANSFORM_3D
	mm.use_colors = true
	mm.mesh = quad
	mm.instance_count = GRASS_COUNT

	for i in range(GRASS_COUNT):
		var angle := rng.randf() * TAU
		var distance := sqrt(rng.randf()) * (FOREST_RADIUS + 18.0)
		var x := cos(angle) * distance
		var z := sin(angle) * distance - 8.0
		if abs(x - sin(z * 0.06) * 4.0) < 4.0 and z > -30.0 and z < 45.0:
			x += rng.randf_range(-5.0, 5.0)
			z += rng.randf_range(-5.0, 5.0)
		var t := Transform3D.IDENTITY
		t = t.rotated(Vector3.UP, rng.randf() * TAU)
		t = t.scaled(Vector3(rng.randf_range(0.75, 1.35), rng.randf_range(0.6, 1.6), 1.0))
		t.origin = Vector3(x, 0.35, z)
		mm.set_instance_transform(i, t)
		mm.set_instance_color(i, Color(
			rng.randf_range(0.24, 0.41),
			rng.randf_range(0.42, 0.62),
			rng.randf_range(0.18, 0.31),
			1.0
		))

	var instance := MultiMeshInstance3D.new()
	instance.multimesh = mm
	instance.material_override = material
	add_child(instance)

func _build_flower_layer() -> void:
	var rng := RandomNumberGenerator.new()
	rng.seed = 801
	var quad := QuadMesh.new()
	quad.size = Vector2(0.1, 0.1)
	var mat := StandardMaterial3D.new()
	mat.albedo_color = Color(0.94, 0.89, 0.70, 1.0)
	mat.emission_enabled = true
	mat.emission = Color(0.32, 0.28, 0.18, 1.0)
	mat.emission_energy_multiplier = 0.2
	mat.cull_mode = BaseMaterial3D.CULL_DISABLED

	var mm := MultiMesh.new()
	mm.transform_format = MultiMesh.TRANSFORM_3D
	mm.use_colors = true
	mm.mesh = quad
	mm.instance_count = FLOWER_COUNT

	for i in range(FLOWER_COUNT):
		var x := rng.randf_range(-34.0, 34.0)
		var z := rng.randf_range(-40.0, 56.0)
		var t := Transform3D.IDENTITY
		t = t.rotated(Vector3.UP, rng.randf() * TAU)
		t = t.scaled(Vector3.ONE * rng.randf_range(0.6, 1.5))
		t.origin = Vector3(x, 0.08, z)
		mm.set_instance_transform(i, t)
		mm.set_instance_color(i, Color(
			rng.randf_range(0.85, 1.0),
			rng.randf_range(0.70, 0.94),
			rng.randf_range(0.38, 0.72),
			1.0
		))

	var instance := MultiMeshInstance3D.new()
	instance.multimesh = mm
	instance.material_override = mat
	add_child(instance)

func _build_mushrooms() -> void:
	var rng := RandomNumberGenerator.new()
	rng.seed = 5012
	for _i in range(MUSHROOM_COUNT):
		var node := Node3D.new()
		node.position = Vector3(rng.randf_range(-47.0, 47.0), 0.04, rng.randf_range(-52.0, 44.0))
		add_child(node)

		var stem := MeshInstance3D.new()
		var stem_mesh := CylinderMesh.new()
		stem_mesh.top_radius = 0.02
		stem_mesh.bottom_radius = 0.03
		stem_mesh.height = rng.randf_range(0.08, 0.16)
		stem.mesh = stem_mesh
		stem.position = Vector3(0, stem_mesh.height * 0.5, 0)
		var stem_mat := StandardMaterial3D.new()
		stem_mat.albedo_color = Color(0.82, 0.77, 0.66, 1.0)
		stem_mat.roughness = 0.92
		stem.material_override = stem_mat
		node.add_child(stem)

		var cap := MeshInstance3D.new()
		var cap_mesh := SphereMesh.new()
		cap_mesh.radius = rng.randf_range(0.05, 0.11)
		cap_mesh.height = cap_mesh.radius * 0.9
		cap.mesh = cap_mesh
		cap.position = Vector3(0, stem_mesh.height + cap_mesh.height * 0.22, 0)
		cap.scale = Vector3(1.2, 0.45, 1.2)
		var cap_mat := StandardMaterial3D.new()
		cap_mat.albedo_color = Color(
			rng.randf_range(0.62, 0.84),
			rng.randf_range(0.20, 0.44),
			rng.randf_range(0.16, 0.30),
			1.0
		)
		cap_mat.roughness = 0.94
		cap.material_override = cap_mat
		node.add_child(cap)

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
