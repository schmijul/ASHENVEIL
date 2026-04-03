extends RefCounted
class_name CharacterVisualFactory

func create_character_visual(character_id: String) -> Node3D:
	var model: Dictionary = AshenveilDataStore.get_character_model(character_id)
	var root := Node3D.new()
	var loaded: Node3D = _try_build_model_instance(model)
	if loaded != null:
		loaded.name = "CharacterModel"
		root.add_child(loaded)
		return root

	root.add_child(_build_fallback_body())
	return root

func _try_build_model_instance(model: Dictionary) -> Node3D:
	if model.is_empty():
		return null

	var raw_path := str(model.get("path", ""))
	if raw_path.is_empty():
		return null

	for candidate in _candidate_paths(raw_path):
		if not ResourceLoader.exists(candidate):
			continue

		var resource: Resource = ResourceLoader.load(candidate)
		if resource == null:
			continue

		if resource is PackedScene:
			var packed: PackedScene = resource as PackedScene
			var instance: Node = packed.instantiate()
			var node: Node3D = instance as Node3D
			if node == null:
				continue
			node.scale = Vector3.ONE * float(model.get("scale", 1.0))
			node.position.y = float(model.get("yOffset", 0.0))
			node.rotation.y = float(model.get("rotationY", 0.0))
			return node

		if resource is Mesh:
			var mesh_node := MeshInstance3D.new()
			mesh_node.mesh = resource as Mesh
			mesh_node.scale = Vector3.ONE * float(model.get("scale", 1.0))
			mesh_node.position.y = float(model.get("yOffset", 0.0))
			mesh_node.rotation.y = float(model.get("rotationY", 0.0))
			mesh_node.material_override = _material(Color(0.55, 0.44, 0.34, 1.0), 0.82)
			return mesh_node

	return null

func _candidate_paths(raw_path: String) -> Array[String]:
	var trimmed := raw_path.strip_edges()
	if trimmed.begins_with("res://"):
		return [trimmed]
	if trimmed.begins_with("/models/"):
		var suffix := trimmed.trim_prefix("/models/")
		return [
			"res://assets/models/%s" % suffix,
			"res://../legacy_web/public/models/%s" % suffix,
		]
	return ["res://assets/models/%s" % trimmed]

func _build_fallback_body() -> Node3D:
	var root := Node3D.new()

	var body := MeshInstance3D.new()
	var body_mesh := CylinderMesh.new()
	body_mesh.top_radius = 0.24
	body_mesh.bottom_radius = 0.34
	body_mesh.height = 1.06
	body_mesh.radial_segments = 22
	body.mesh = body_mesh
	body.position = Vector3(0.0, 1.0, 0.0)
	body.rotation.x = 0.05
	body.material_override = _material(Color(0.41, 0.34, 0.28, 1.0), 0.93)
	root.add_child(body)

	var head := MeshInstance3D.new()
	var head_mesh := SphereMesh.new()
	head_mesh.radius = 0.22
	head_mesh.height = 0.44
	head.mesh = head_mesh
	head.position = Vector3(0.0, 1.78, 0.04)
	head.material_override = _material(Color(0.72, 0.58, 0.46, 1.0), 0.88)
	root.add_child(head)

	var belt := MeshInstance3D.new()
	var belt_mesh := BoxMesh.new()
	belt_mesh.size = Vector3(0.44, 0.1, 0.26)
	belt.mesh = belt_mesh
	belt.position = Vector3(0.0, 1.18, 0.0)
	belt.material_override = _material(Color(0.24, 0.18, 0.12, 1.0), 0.96)
	root.add_child(belt)

	return root

func _material(color: Color, roughness: float) -> StandardMaterial3D:
	var mat := StandardMaterial3D.new()
	mat.albedo_color = color
	mat.roughness = roughness
	return mat
