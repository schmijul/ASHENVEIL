extends RefCounted
class_name CharacterVisualFactory

func create_character_visual(character_id: String, variant: String = "") -> Node3D:
	var model: Dictionary = AshenveilDataStore.get_character_model(character_id)
	var root := Node3D.new()
	var loaded: Node3D = _try_build_model_instance(model)
	if loaded != null:
		loaded.name = "CharacterModel"
		root.add_child(loaded)
		return root

	root.add_child(_build_fallback_body(character_id, variant))
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

func _build_fallback_body(character_id: String, variant: String) -> Node3D:
	var root := Node3D.new()
	var style := _resolve_style(character_id, variant)
	var palette := _style_palette(style)

	var legs := MeshInstance3D.new()
	var legs_mesh := BoxMesh.new()
	legs_mesh.size = Vector3(0.34, 0.76, 0.26)
	legs.mesh = legs_mesh
	legs.position = Vector3(0.0, 0.42, 0.0)
	legs.material_override = _material(palette["legs"], 0.98)
	root.add_child(legs)

	var torso := MeshInstance3D.new()
	var torso_mesh := CylinderMesh.new()
	torso_mesh.top_radius = 0.24
	torso_mesh.bottom_radius = 0.34
	torso_mesh.height = 0.98
	torso_mesh.radial_segments = 20
	torso.mesh = torso_mesh
	torso.position = Vector3(0.0, 1.10, 0.0)
	torso.rotation.x = 0.04
	torso.material_override = _material(palette["body"], 0.93)
	root.add_child(torso)

	var shoulders := MeshInstance3D.new()
	var shoulders_mesh := BoxMesh.new()
	shoulders_mesh.size = Vector3(0.88, 0.20, 0.50)
	shoulders.mesh = shoulders_mesh
	shoulders.position = Vector3(0.0, 1.46, 0.0)
	shoulders.material_override = _material(palette["accent"], 0.92)
	root.add_child(shoulders)

	var left_arm := _build_arm(Vector3(-0.42, 1.20, 0.0), -0.30, palette["accent"])
	var right_arm := _build_arm(Vector3(0.42, 1.20, 0.0), 0.30, palette["accent"])
	root.add_child(left_arm)
	root.add_child(right_arm)

	var head := MeshInstance3D.new()
	var head_mesh := SphereMesh.new()
	head_mesh.radius = 0.22
	head_mesh.height = 0.44
	head.mesh = head_mesh
	head.position = Vector3(0.0, 1.78, 0.04)
	head.material_override = _material(palette["skin"], 0.88)
	root.add_child(head)

	var belt := MeshInstance3D.new()
	var belt_mesh := BoxMesh.new()
	belt_mesh.size = Vector3(0.44, 0.1, 0.26)
	belt.mesh = belt_mesh
	belt.position = Vector3(0.0, 1.18, 0.0)
	belt.material_override = _material(palette["belt"], 0.96)
	root.add_child(belt)

	var clothing := _build_clothing_layer(style, palette)
	if clothing != null:
		root.add_child(clothing)

	var prop := _build_role_prop(style)
	if prop != null:
		root.add_child(prop)

	return root

func _build_arm(offset: Vector3, tilt: float, color: Color) -> MeshInstance3D:
	var arm := MeshInstance3D.new()
	var arm_mesh := CylinderMesh.new()
	arm_mesh.top_radius = 0.08
	arm_mesh.bottom_radius = 0.10
	arm_mesh.height = 0.80
	arm_mesh.radial_segments = 10
	arm.mesh = arm_mesh
	arm.position = offset
	arm.rotation = Vector3(0.0, 0.0, tilt)
	arm.material_override = _material(_shade_color(color, 0.82), 0.95)
	return arm

func _build_clothing_layer(style: String, palette: Dictionary) -> Node3D:
	var root := Node3D.new()

	if style == "player":
		var tunic := MeshInstance3D.new()
		var tunic_mesh := CylinderMesh.new()
		tunic_mesh.top_radius = 0.32
		tunic_mesh.bottom_radius = 0.42
		tunic_mesh.height = 1.12
		tunic_mesh.radial_segments = 18
		tunic.mesh = tunic_mesh
		tunic.position = Vector3(0, 0.96, 0)
		tunic.material_override = _material(palette["tunic"], 0.94)
		root.add_child(tunic)
		return root

	if style == "merchant" or style == "innkeeper":
		var apron := MeshInstance3D.new()
		var apron_mesh := BoxMesh.new()
		apron_mesh.size = Vector3(0.64, 0.86, 0.20)
		apron.mesh = apron_mesh
		apron.position = Vector3(0, 1.06, 0.18)
		apron.material_override = _material(palette["accent"], 0.96)
		root.add_child(apron)
		return root

	if style == "hunter" or style == "guard" or style == "soldier":
		var cloak := MeshInstance3D.new()
		var cloak_mesh := CylinderMesh.new()
		cloak_mesh.top_radius = 0.30
		cloak_mesh.bottom_radius = 0.50
		cloak_mesh.height = 1.18
		cloak_mesh.radial_segments = 16
		cloak.mesh = cloak_mesh
		cloak.position = Vector3(0, 1.02, -0.02)
		cloak.material_override = _material(palette["cloak"], 0.97)
		root.add_child(cloak)
		return root

	if style == "healer" or style == "herbalist":
		var shawl := MeshInstance3D.new()
		var shawl_mesh := CylinderMesh.new()
		shawl_mesh.top_radius = 0.31
		shawl_mesh.bottom_radius = 0.47
		shawl_mesh.height = 1.10
		shawl_mesh.radial_segments = 16
		shawl.mesh = shawl_mesh
		shawl.position = Vector3(0, 1.00, 0.0)
		shawl.material_override = _material(palette["cloak"], 0.96)
		root.add_child(shawl)
		return root

	if style == "elder":
		var robe := MeshInstance3D.new()
		var robe_mesh := CylinderMesh.new()
		robe_mesh.top_radius = 0.33
		robe_mesh.bottom_radius = 0.46
		robe_mesh.height = 1.22
		robe_mesh.radial_segments = 18
		robe.mesh = robe_mesh
		robe.position = Vector3(0, 0.98, 0)
		robe.material_override = _material(palette["cloak"], 0.97)
		root.add_child(robe)
		return root

	return root

func _build_role_prop(role: String) -> Node3D:
	var prop := Node3D.new()
	if role == "player":
		return null

	if role == "elder":
		var staff := MeshInstance3D.new()
		var staff_mesh := CylinderMesh.new()
		staff_mesh.top_radius = 0.05
		staff_mesh.bottom_radius = 0.07
		staff_mesh.height = 1.7
		staff_mesh.radial_segments = 10
		staff.mesh = staff_mesh
		staff.position = Vector3(-0.78, 1.0, 0.08)
		staff.rotation_degrees = Vector3(0, 0, 4)
		staff.material_override = _material(Color(0.33, 0.22, 0.15, 1.0), 0.96)
		prop.add_child(staff)
	elif role == "merchant" or role == "innkeeper":
		var pouch := MeshInstance3D.new()
		var pouch_mesh := BoxMesh.new()
		pouch_mesh.size = Vector3(0.20, 0.28, 0.16)
		pouch.mesh = pouch_mesh
		pouch.position = Vector3(0.72, 1.02, 0.12)
		pouch.material_override = _material(Color(0.47, 0.30, 0.16, 1.0), 0.94)
		prop.add_child(pouch)
	elif role == "hunter" or role == "guard" or role == "soldier" or role == "blacksmith":
		var bundle := MeshInstance3D.new()
		var bundle_mesh := CylinderMesh.new()
		bundle_mesh.top_radius = 0.06
		bundle_mesh.bottom_radius = 0.09
		bundle_mesh.height = 1.4
		bundle_mesh.radial_segments = 10
		bundle.mesh = bundle_mesh
		bundle.position = Vector3(0.76, 1.1, -0.12)
		bundle.rotation_degrees = Vector3(0, 0, -16)
		bundle.material_override = _material(Color(0.26, 0.18, 0.12, 1.0), 0.94)
		prop.add_child(bundle)
	elif role == "healer" or role == "herbalist":
		var basket := MeshInstance3D.new()
		var basket_mesh := CylinderMesh.new()
		basket_mesh.top_radius = 0.15
		basket_mesh.bottom_radius = 0.20
		basket_mesh.height = 0.26
		basket_mesh.radial_segments = 12
		basket.mesh = basket_mesh
		basket.position = Vector3(0.54, 0.72, 0.18)
		basket.material_override = _material(Color(0.45, 0.33, 0.18, 1.0), 0.95)
		prop.add_child(basket)

	return prop

func _resolve_style(character_id: String, variant: String) -> String:
	var text := variant if not variant.is_empty() else character_id
	text = text.to_lower()
	if text.is_empty():
		return "generic"
	if text == "player":
		return "player"
	if text.find("merchant") != -1 or text.find("trade") != -1 or text.find("trader") != -1:
		return "merchant"
	if text.find("hunter") != -1 or text.find("guard") != -1 or text.find("soldier") != -1:
		return "hunter"
	if text.find("healer") != -1 or text.find("herb") != -1:
		return "healer"
	if text.find("elder") != -1 or text.find("sage") != -1 or text.find("maren") != -1:
		return "elder"
	if text.find("smith") != -1 or text.find("forge") != -1:
		return "blacksmith"
	if text.find("inn") != -1:
		return "innkeeper"
	return "generic"

func _style_palette(style: String) -> Dictionary:
	match style:
		"player":
			return {
				"legs": Color(0.31, 0.28, 0.22, 1.0),
				"body": Color(0.42, 0.35, 0.29, 1.0),
				"skin": Color(0.72, 0.58, 0.46, 1.0),
				"belt": Color(0.21, 0.16, 0.10, 1.0),
				"accent": Color(0.54, 0.40, 0.26, 1.0),
				"cloak": Color(0.22, 0.20, 0.18, 1.0),
				"tunic": Color(0.36, 0.29, 0.24, 1.0),
			}
		"merchant":
			return {
				"legs": Color(0.36, 0.28, 0.20, 1.0),
				"body": Color(0.49, 0.39, 0.28, 1.0),
				"skin": Color(0.73, 0.58, 0.45, 1.0),
				"belt": Color(0.26, 0.19, 0.12, 1.0),
				"accent": Color(0.61, 0.48, 0.25, 1.0),
				"cloak": Color(0.52, 0.36, 0.19, 1.0),
				"tunic": Color(0.46, 0.34, 0.23, 1.0),
			}
		"hunter":
			return {
				"legs": Color(0.22, 0.27, 0.19, 1.0),
				"body": Color(0.31, 0.36, 0.24, 1.0),
				"skin": Color(0.70, 0.55, 0.43, 1.0),
				"belt": Color(0.18, 0.14, 0.09, 1.0),
				"accent": Color(0.24, 0.31, 0.19, 1.0),
				"cloak": Color(0.20, 0.24, 0.15, 1.0),
				"tunic": Color(0.28, 0.33, 0.22, 1.0),
			}
		"healer":
			return {
				"legs": Color(0.34, 0.31, 0.25, 1.0),
				"body": Color(0.43, 0.41, 0.32, 1.0),
				"skin": Color(0.73, 0.58, 0.46, 1.0),
				"belt": Color(0.25, 0.18, 0.13, 1.0),
				"accent": Color(0.38, 0.42, 0.24, 1.0),
				"cloak": Color(0.33, 0.39, 0.22, 1.0),
				"tunic": Color(0.38, 0.42, 0.27, 1.0),
			}
		"elder":
			return {
				"legs": Color(0.38, 0.33, 0.25, 1.0),
				"body": Color(0.48, 0.43, 0.34, 1.0),
				"skin": Color(0.68, 0.54, 0.42, 1.0),
				"belt": Color(0.20, 0.16, 0.11, 1.0),
				"accent": Color(0.35, 0.31, 0.26, 1.0),
				"cloak": Color(0.33, 0.28, 0.24, 1.0),
				"tunic": Color(0.38, 0.34, 0.28, 1.0),
			}
		"innkeeper":
			return {
				"legs": Color(0.34, 0.27, 0.21, 1.0),
				"body": Color(0.47, 0.36, 0.27, 1.0),
				"skin": Color(0.72, 0.57, 0.46, 1.0),
				"belt": Color(0.22, 0.16, 0.11, 1.0),
				"accent": Color(0.50, 0.34, 0.20, 1.0),
				"cloak": Color(0.42, 0.30, 0.20, 1.0),
				"tunic": Color(0.40, 0.31, 0.24, 1.0),
			}
		_:
			return {
				"legs": Color(0.33, 0.27, 0.21, 1.0),
				"body": Color(0.43, 0.35, 0.28, 1.0),
				"skin": Color(0.72, 0.58, 0.46, 1.0),
				"belt": Color(0.21, 0.17, 0.11, 1.0),
				"accent": Color(0.40, 0.31, 0.24, 1.0),
				"cloak": Color(0.24, 0.22, 0.20, 1.0),
				"tunic": Color(0.38, 0.30, 0.24, 1.0),
			}

func _material(color: Color, roughness: float) -> StandardMaterial3D:
	var mat := StandardMaterial3D.new()
	mat.albedo_color = color
	mat.roughness = roughness
	return mat

func _shade_color(color: Color, factor: float) -> Color:
	return Color(
		clamp(color.r * factor, 0.0, 1.0),
		clamp(color.g * factor, 0.0, 1.0),
		clamp(color.b * factor, 0.0, 1.0),
		color.a
	)
