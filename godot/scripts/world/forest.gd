extends Node3D

const BOAR_SCENE := preload("res://scenes/entities/boar.tscn")
const TREE_MODEL_PATH := "res://assets/models/tree1.glb"
const TREE_BARK_DIFFUSE_PATH := "res://assets/textures/wood/bark_brown_02/diffuse.jpg"
const TREE_BARK_ROUGHNESS_PATH := "res://assets/textures/wood/bark_brown_02/roughness.jpg"
const TREE_BARK_NORMAL_PATH := "res://assets/textures/wood/bark_brown_02/normal_gl.jpg"
const TREE_MODEL_BASE_SCALE := 0.38
const FOREST_RADIUS := 58.0
const TREE_COUNT := 130
const BUSH_COUNT := 260
const ROCK_COUNT := 55
const GRASS_COUNT := 1800
const FLOWER_COUNT := 420
const LOG_COUNT := 75
const MUSHROOM_COUNT := 240
const FERN_CLUSTER_COUNT := 240
const STUMP_COUNT := 42
const ROOT_CLUSTER_COUNT := 58
const FIREFLY_SWARM_COUNT := 10

func _ready() -> void:
	_build_tree_layers()
	_build_bush_layer()
	_build_rocks()
	_build_logs()
	_build_forest_floor()
	_build_fern_clusters()
	_build_stumps()
	_build_root_clusters()
	_build_grass_layer()
	_build_flower_layer()
	_build_mushrooms()
	_build_fireflies()
	_spawn_boars()

func _build_tree_layers() -> void:
	var tree_scene: PackedScene = _load_tree_scene()
	var rng := RandomNumberGenerator.new()
	rng.seed = 4207
	for i in range(TREE_COUNT):
		var angle: float = rng.randf() * TAU
		var distance: float = sqrt(rng.randf()) * FOREST_RADIUS
		var position: Vector3 = Vector3(cos(angle) * distance, 0.0, sin(angle) * distance - 8.0)
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
		var trunk_material := _tree_bark_material()
		trunk.material_override = trunk_material
		tree.add_child(trunk)

		if i % 5 == 0:
			_add_oak_canopy(tree, trunk_mesh.height, rng)
		else:
			_add_pine_canopy(tree, trunk_mesh.height)

func _load_tree_scene() -> PackedScene:
	if not ResourceLoader.exists(TREE_MODEL_PATH):
		return null
	var tree_scene_resource: Resource = ResourceLoader.load(TREE_MODEL_PATH)
	return tree_scene_resource as PackedScene

func _tree_bark_material() -> StandardMaterial3D:
	var mat := StandardMaterial3D.new()
	mat.albedo_color = Color(0.35, 0.24, 0.16, 1.0)
	mat.roughness = 0.93
	var diff := _load_texture_from_image(TREE_BARK_DIFFUSE_PATH)
	var rough := _load_texture_from_image(TREE_BARK_ROUGHNESS_PATH)
	var normal := _load_texture_from_image(TREE_BARK_NORMAL_PATH)
	if diff != null:
		mat.albedo_texture = diff
	if rough != null:
		mat.roughness_texture = rough
	if normal != null:
		mat.normal_enabled = true
		mat.normal_texture = normal
		mat.normal_scale = 0.7
	return mat

func _load_texture_from_image(path: String) -> Texture2D:
	if not FileAccess.file_exists(path):
		return null
	var image := Image.new()
	var error := image.load(path)
	if error != OK:
		return null
	return ImageTexture.create_from_image(image)

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
		var angle: float = rng.randf() * TAU
		var distance: float = sqrt(rng.randf()) * (FOREST_RADIUS + 10.0)
		var pos: Vector3 = Vector3(cos(angle) * distance, 0.0, sin(angle) * distance - 8.0)
		if abs(pos.x - sin(pos.z * 0.06) * 4.0) < 4.7 and pos.z > -30.0 and pos.z < 45.0:
			continue
		var cluster := Node3D.new()
		cluster.position = pos + Vector3(0.0, 0.04, 0.0)
		cluster.rotation.y = rng.randf() * TAU
		add_child(cluster)
		var lobe_count := rng.randi_range(2, 4)
		for lobe_index in range(lobe_count):
			var bush := MeshInstance3D.new()
			var bush_mesh := SphereMesh.new()
			bush_mesh.radius = rng.randf_range(0.18, 0.44)
			bush_mesh.height = bush_mesh.radius * rng.randf_range(1.0, 1.45)
			bush.mesh = bush_mesh
			bush.position = Vector3(
				rng.randf_range(-0.28, 0.28),
				bush_mesh.height * 0.35,
				rng.randf_range(-0.28, 0.28)
			)
			var mat := StandardMaterial3D.new()
			mat.albedo_color = Color(
				rng.randf_range(0.18, 0.30),
				rng.randf_range(0.31, 0.45),
				rng.randf_range(0.15, 0.25),
				1.0
			)
			mat.roughness = 0.98
			bush.material_override = mat
			cluster.add_child(bush)

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

func _build_stumps() -> void:
	var rng := RandomNumberGenerator.new()
	rng.seed = 1811
	for _i in range(STUMP_COUNT):
		var stump := MeshInstance3D.new()
		var mesh := CylinderMesh.new()
		mesh.top_radius = rng.randf_range(0.16, 0.32)
		mesh.bottom_radius = mesh.top_radius * rng.randf_range(1.05, 1.22)
		mesh.height = rng.randf_range(0.22, 0.58)
		mesh.radial_segments = 10
		stump.mesh = mesh
		stump.position = Vector3(rng.randf_range(-52.0, 52.0), mesh.height * 0.5 - 0.02, rng.randf_range(-52.0, 46.0))
		stump.rotation.y = rng.randf() * TAU
		var mat := _tree_bark_material()
		stump.material_override = mat
		add_child(stump)

func _build_root_clusters() -> void:
	var rng := RandomNumberGenerator.new()
	rng.seed = 6613
	for _i in range(ROOT_CLUSTER_COUNT):
		var cluster := Node3D.new()
		cluster.position = Vector3(rng.randf_range(-50.0, 50.0), 0.03, rng.randf_range(-50.0, 44.0))
		cluster.rotation.y = rng.randf() * TAU
		add_child(cluster)
		for branch_index in range(rng.randi_range(2, 4)):
			var root := MeshInstance3D.new()
			var mesh := CylinderMesh.new()
			mesh.top_radius = 0.035
			mesh.bottom_radius = rng.randf_range(0.06, 0.1)
			mesh.height = rng.randf_range(0.8, 1.7)
			mesh.radial_segments = 6
			root.mesh = mesh
			root.position = Vector3(rng.randf_range(-0.18, 0.18), 0.07 + float(branch_index) * 0.015, rng.randf_range(-0.18, 0.18))
			root.rotation = Vector3(PI * 0.5 + rng.randf_range(-0.18, 0.18), rng.randf() * TAU, rng.randf_range(-0.15, 0.15))
			root.material_override = _tree_bark_material()
			cluster.add_child(root)

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

	for side in [-1.0, 1.0]:
		for i in range(16):
			var edge_cluster := Node3D.new()
			edge_cluster.position = Vector3(side * (4.8 + sin(float(i) * 0.8) * 1.5), 0.04, -30.0 + float(i) * 5.4)
			edge_cluster.rotation.y = float(i) * 0.35
			add_child(edge_cluster)
			for branch_index in range(3):
				var branch := MeshInstance3D.new()
				var branch_mesh := CylinderMesh.new()
				branch_mesh.top_radius = 0.03
				branch_mesh.bottom_radius = 0.055
				branch_mesh.height = 0.65 + float(branch_index) * 0.16
				branch_mesh.radial_segments = 5
				branch.mesh = branch_mesh
				branch.position = Vector3(0.0, 0.08 + float(branch_index) * 0.03, float(branch_index) * 0.12 - 0.12)
				branch.rotation = Vector3(PI * 0.5 + 0.12 * float(branch_index), 0.18 * float(branch_index), 0.14 * side)
				branch.material_override = _tree_bark_material()
				edge_cluster.add_child(branch)

func _build_fern_clusters() -> void:
	var rng := RandomNumberGenerator.new()
	rng.seed = 9811
	for _i in range(FERN_CLUSTER_COUNT):
		var cluster := Node3D.new()
		cluster.position = Vector3(rng.randf_range(-55.0, 55.0), 0.02, rng.randf_range(-52.0, 46.0))
		if abs(cluster.position.x - sin(cluster.position.z * 0.06) * 4.0) < 3.6 and cluster.position.z > -30.0 and cluster.position.z < 45.0:
			continue
		cluster.rotation.y = rng.randf() * TAU
		add_child(cluster)
		var frond_count := rng.randi_range(3, 5)
		for frond_index in range(frond_count):
			var frond := MeshInstance3D.new()
			var frond_mesh := PrismMesh.new()
			frond_mesh.size = Vector3(rng.randf_range(0.12, 0.22), rng.randf_range(0.4, 0.8), rng.randf_range(0.5, 0.95))
			frond.mesh = frond_mesh
			frond.position = Vector3(
				rng.randf_range(-0.14, 0.14),
				frond_mesh.size.y * 0.38,
				rng.randf_range(-0.14, 0.14)
			)
			frond.rotation = Vector3(
				rng.randf_range(-0.18, 0.16),
				float(frond_index) / float(frond_count) * TAU + rng.randf_range(-0.18, 0.18),
				rng.randf_range(-0.68, -0.22)
			)
			var mat := StandardMaterial3D.new()
			mat.albedo_color = Color(
				rng.randf_range(0.18, 0.27),
				rng.randf_range(0.34, 0.47),
				rng.randf_range(0.13, 0.22),
				1.0
			)
			mat.roughness = 0.97
			frond.material_override = mat
			cluster.add_child(frond)

func _build_grass_layer() -> void:
	var rng := RandomNumberGenerator.new()
	rng.seed = 781
	var tuft := _build_grass_tuft_mesh()
	var material := StandardMaterial3D.new()
	material.albedo_color = Color(0.28, 0.42, 0.21, 1.0)
	material.roughness = 1.0
	material.cull_mode = BaseMaterial3D.CULL_DISABLED

	var mm := MultiMesh.new()
	mm.transform_format = MultiMesh.TRANSFORM_3D
	mm.use_colors = true
	mm.mesh = tuft
	mm.instance_count = GRASS_COUNT

	for i in range(GRASS_COUNT):
		var angle: float = rng.randf() * TAU
		var distance: float = sqrt(rng.randf()) * (FOREST_RADIUS + 18.0)
		var x: float = cos(angle) * distance
		var z: float = sin(angle) * distance - 8.0
		if abs(x - sin(z * 0.06) * 4.0) < 4.0 and z > -30.0 and z < 45.0:
			x += rng.randf_range(-5.0, 5.0)
			z += rng.randf_range(-5.0, 5.0)
		var t := Transform3D.IDENTITY
		t = t.rotated(Vector3.UP, rng.randf() * TAU)
		t = t.scaled(Vector3(rng.randf_range(0.65, 1.15), rng.randf_range(0.7, 1.5), 0.9))
		t.origin = Vector3(x, 0.35, z)
		mm.set_instance_transform(i, t)
		mm.set_instance_color(i, Color(
			rng.randf_range(0.19, 0.36),
			rng.randf_range(0.38, 0.57),
			rng.randf_range(0.15, 0.27),
			1.0
		))

	var instance := MultiMeshInstance3D.new()
	instance.multimesh = mm
	instance.material_override = material
	add_child(instance)

func _build_grass_tuft_mesh() -> ArrayMesh:
	var mesh := ArrayMesh.new()
	var vertices := PackedVector3Array()
	var normals := PackedVector3Array()
	var uvs := PackedVector2Array()
	var indices := PackedInt32Array()
	var blade_width := 0.045
	var blade_height := 0.78
	var blade_angles := [0.0, PI * 0.33, PI * 0.66, PI * 1.02]

	for blade_index in range(blade_angles.size()):
		var angle: float = float(blade_angles[blade_index])
		var forward: Vector3 = Vector3(cos(angle), 0.0, sin(angle))
		var side: Vector3 = Vector3(-sin(angle), 0.0, cos(angle)) * blade_width
		var bend: Vector3 = forward * 0.10
		var base_index: int = vertices.size()

		vertices.append((-side) + Vector3(0.0, 0.0, 0.0))
		vertices.append(side + Vector3(0.0, 0.0, 0.0))
		vertices.append((side * 0.35) + Vector3(0.0, blade_height, 0.0) + bend)
		vertices.append((-side * 0.35) + Vector3(0.0, blade_height, 0.0) + bend)

		for _i in range(4):
			normals.append(forward)

		uvs.append(Vector2(0.0, 1.0))
		uvs.append(Vector2(1.0, 1.0))
		uvs.append(Vector2(1.0, 0.0))
		uvs.append(Vector2(0.0, 0.0))
		indices.append_array([base_index, base_index + 1, base_index + 2, base_index, base_index + 2, base_index + 3])

	var arrays: Array = []
	arrays.resize(Mesh.ARRAY_MAX)
	arrays[Mesh.ARRAY_VERTEX] = vertices
	arrays[Mesh.ARRAY_NORMAL] = normals
	arrays[Mesh.ARRAY_TEX_UV] = uvs
	arrays[Mesh.ARRAY_INDEX] = indices
	mesh.add_surface_from_arrays(Mesh.PRIMITIVE_TRIANGLES, arrays)
	return mesh

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

func _build_fireflies() -> void:
	for swarm_index in range(FIREFLY_SWARM_COUNT):
		var particles := GPUParticles3D.new()
		particles.amount = 22
		particles.lifetime = 3.4
		particles.one_shot = false
		particles.preprocess = 1.6
		particles.position = Vector3(
			-20.0 + float(swarm_index % 5) * 9.0,
			1.2 + float(swarm_index % 3) * 0.35,
			-24.0 + float(swarm_index / 2) * 8.0
		)
		var process := ParticleProcessMaterial.new()
		process.direction = Vector3(0.0, 0.12, 0.0)
		process.initial_velocity_min = 0.02
		process.initial_velocity_max = 0.08
		process.angular_velocity_min = -0.6
		process.angular_velocity_max = 0.6
		process.scale_min = 0.02
		process.scale_max = 0.05
		process.color = Color(1.0, 0.92, 0.66, 0.72)
		process.turbulence_enabled = true
		process.turbulence_noise_scale = 0.6
		process.orbit_velocity_min = 0.4
		process.orbit_velocity_max = 0.9
		particles.process_material = process
		add_child(particles)

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
