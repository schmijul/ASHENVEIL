extends Node3D

@export var npc_id := "maren"

var _label: Label3D

func _ready() -> void:
	_build_visual()
	_build_interaction_area()
	_refresh_label()

func configure_npc(id: String) -> void:
	npc_id = id
	if is_inside_tree():
		_refresh_label()

func _build_visual() -> void:
	if _label == null:
		_label = Label3D.new()
		_label.billboard = BaseMaterial3D.BILLBOARD_ENABLED
		_label.position = Vector3(0, 2.35, 0)
		add_child(_label)

	var model_root := _try_load_character_model()
	if model_root != null:
		add_child(model_root)
		return

	add_child(_build_fallback_body())

func _try_load_character_model() -> Node3D:
	var model: Dictionary = AshenveilDataStore.get_character_model(npc_id)
	if model.is_empty():
		return null

	var raw_path := str(model.get("path", "")).strip_edges()
	if raw_path.is_empty():
		return null

	var candidate_paths := _build_model_candidates(raw_path)
	for candidate in candidate_paths:
		if not ResourceLoader.exists(candidate):
			continue
		var packed := ResourceLoader.load(candidate) as PackedScene
		if packed == null:
			continue
		var instance := packed.instantiate() as Node3D
		if instance == null:
			continue
		instance.scale = Vector3.ONE * float(model.get("scale", 1.0))
		instance.position.y = float(model.get("yOffset", 0.0))
		instance.rotation.y = float(model.get("rotationY", 0.0))
		return instance

	return null

func _build_model_candidates(raw_path: String) -> Array[String]:
	if raw_path.begins_with("res://"):
		return [raw_path]
	if raw_path.begins_with("/models/"):
		var suffix := raw_path.trim_prefix("/models/")
		return [
			"res://../legacy_web/public/models/characters/%s" % suffix,
			"res://assets/models/%s" % suffix,
		]
	return [
		"res://../legacy_web/public/models/characters/%s" % raw_path,
		"res://assets/models/%s" % raw_path,
	]

func _build_fallback_body() -> Node3D:
	var npc := AshenveilDataStore.get_npc(npc_id)
	var role := str(npc.get("role", "")).to_lower()
	var accent := _role_color(role)
	var root := Node3D.new()

	var torso := MeshInstance3D.new()
	var torso_mesh := CylinderMesh.new()
	torso_mesh.top_radius = 0.32
	torso_mesh.bottom_radius = 0.42
	torso_mesh.height = 1.25
	torso_mesh.radial_segments = 18
	torso.mesh = torso_mesh
	torso.position = Vector3(0, 1.02, 0)
	torso.material_override = _make_material(Color(0.46, 0.36, 0.28, 1.0), 0.93)
	root.add_child(torso)

	var coat := MeshInstance3D.new()
	var coat_mesh := CylinderMesh.new()
	coat_mesh.top_radius = 0.40
	coat_mesh.bottom_radius = 0.56
	coat_mesh.height = 1.55
	coat_mesh.radial_segments = 18
	coat.mesh = coat_mesh
	coat.position = Vector3(0, 0.84, 0)
	coat.material_override = _make_material(accent, 0.96)
	root.add_child(coat)

	var head := MeshInstance3D.new()
	var head_mesh := SphereMesh.new()
	head_mesh.radius = 0.24
	head_mesh.height = 0.48
	head.mesh = head_mesh
	head.position = Vector3(0, 1.92, 0)
	head.material_override = _make_material(Color(0.72, 0.58, 0.46, 1.0), 0.86)
	root.add_child(head)

	var shoulders := MeshInstance3D.new()
	var shoulders_mesh := BoxMesh.new()
	shoulders_mesh.size = Vector3(0.9, 0.22, 0.52)
	shoulders.mesh = shoulders_mesh
	shoulders.position = Vector3(0, 1.56, 0)
	shoulders.material_override = _make_material(_shade_color(accent, 1.08), 0.91)
	root.add_child(shoulders)

	var left_arm := _build_arm(Vector3(-0.48, 1.35, 0), -0.34, accent)
	var right_arm := _build_arm(Vector3(0.48, 1.35, 0), 0.34, accent)
	root.add_child(left_arm)
	root.add_child(right_arm)

	var prop := _build_role_prop(role)
	if prop != null:
		root.add_child(prop)

	return root

func _build_arm(offset: Vector3, tilt: float, accent: Color) -> MeshInstance3D:
	var arm := MeshInstance3D.new()
	var arm_mesh := CylinderMesh.new()
	arm_mesh.top_radius = 0.08
	arm_mesh.bottom_radius = 0.10
	arm_mesh.height = 0.84
	arm_mesh.radial_segments = 10
	arm.mesh = arm_mesh
	arm.position = offset
	arm.rotation = Vector3(0.0, 0.0, tilt)
	arm.material_override = _make_material(_shade_color(accent, 0.82), 0.95)
	return arm

func _build_role_prop(role: String) -> Node3D:
	var prop := Node3D.new()
	if role.find("elder") != -1 or role.find("maren") != -1 or role.find("sage") != -1:
		var staff := MeshInstance3D.new()
		var staff_mesh := CylinderMesh.new()
		staff_mesh.top_radius = 0.05
		staff_mesh.bottom_radius = 0.07
		staff_mesh.height = 1.7
		staff_mesh.radial_segments = 10
		staff.mesh = staff_mesh
		staff.position = Vector3(-0.78, 1.0, 0.08)
		staff.rotation_degrees = Vector3(0, 0, 4)
		staff.material_override = _make_material(Color(0.33, 0.22, 0.15, 1.0), 0.96)
		prop.add_child(staff)
	elif role.find("merchant") != -1 or role.find("trade") != -1 or role.find("trader") != -1:
		var pouch := MeshInstance3D.new()
		var pouch_mesh := BoxMesh.new()
		pouch_mesh.size = Vector3(0.2, 0.28, 0.16)
		pouch.mesh = pouch_mesh
		pouch.position = Vector3(0.72, 1.02, 0.12)
		pouch.material_override = _make_material(Color(0.47, 0.30, 0.16, 1.0), 0.94)
		prop.add_child(pouch)
	elif role.find("hunter") != -1 or role.find("guard") != -1 or role.find("soldier") != -1 or role.find("blacksmith") != -1:
		var bundle := MeshInstance3D.new()
		var bundle_mesh := CylinderMesh.new()
		bundle_mesh.top_radius = 0.06
		bundle_mesh.bottom_radius = 0.09
		bundle_mesh.height = 1.4
		bundle_mesh.radial_segments = 10
		bundle.mesh = bundle_mesh
		bundle.position = Vector3(0.76, 1.1, -0.12)
		bundle.rotation_degrees = Vector3(0, 0, -16)
		bundle.material_override = _make_material(Color(0.26, 0.18, 0.12, 1.0), 0.94)
		prop.add_child(bundle)
	elif role.find("herbalist") != -1 or role.find("healer") != -1:
		var basket := MeshInstance3D.new()
		var basket_mesh := CylinderMesh.new()
		basket_mesh.top_radius = 0.15
		basket_mesh.bottom_radius = 0.20
		basket_mesh.height = 0.26
		basket_mesh.radial_segments = 12
		basket.mesh = basket_mesh
		basket.position = Vector3(0.54, 0.72, 0.18)
		basket.material_override = _make_material(Color(0.45, 0.33, 0.18, 1.0), 0.95)
		prop.add_child(basket)
	return prop

func _make_material(color: Color, roughness: float) -> StandardMaterial3D:
	var material := StandardMaterial3D.new()
	material.albedo_color = color
	material.roughness = roughness
	return material

func _shade_color(color: Color, factor: float) -> Color:
	return Color(clamp(color.r * factor, 0.0, 1.0), clamp(color.g * factor, 0.0, 1.0), clamp(color.b * factor, 0.0, 1.0), color.a)

func _role_color(role: String) -> Color:
	if role.find("merchant") != -1:
		return Color(0.56, 0.41, 0.25, 1.0)
	if role.find("hunter") != -1:
		return Color(0.26, 0.35, 0.20, 1.0)
	if role.find("guard") != -1 or role.find("soldier") != -1:
		return Color(0.35, 0.30, 0.24, 1.0)
	if role.find("herb") != -1 or role.find("healer") != -1:
		return Color(0.34, 0.42, 0.22, 1.0)
	return Color(0.44, 0.33, 0.24, 1.0)

func _build_interaction_area() -> void:
	var area := Area3D.new()
	add_child(area)

	var shape_node := CollisionShape3D.new()
	var shape := SphereShape3D.new()
	shape.radius = 1.5
	shape_node.shape = shape
	shape_node.position = Vector3(0, 1.0, 0)
	area.add_child(shape_node)

func _refresh_label() -> void:
	var npc: Dictionary = AshenveilDataStore.get_npc(npc_id)
	var display_name: String = str(npc.get("name", npc_id))
	var role: String = str(npc.get("role", ""))
	var model: Dictionary = AshenveilDataStore.get_character_model(npc_id)
	var model_path: String = str(model.get("path", ""))
	var model_tag: String = ""
	if model_path.is_empty():
		model_tag = "procedural fallback"
	else:
		model_tag = model_path.get_file()
	_label.text = "%s\n%s\n%s" % [display_name, role, model_tag]
