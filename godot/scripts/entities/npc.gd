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

	var body := MeshInstance3D.new()
	var body_mesh := CapsuleMesh.new()
	body_mesh.radius = 0.35
	body_mesh.mid_height = 0.85
	body.mesh = body_mesh
	body.position = Vector3(0, 1.1, 0)
	var material := StandardMaterial3D.new()
	material.albedo_color = Color(0.58, 0.48, 0.38, 1.0)
	material.roughness = 0.95
	body.material_override = material
	add_child(body)

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
	var npc := AshenveilDataStore.get_npc(npc_id)
	var display_name := npc.get("name", npc_id)
	var role := npc.get("role", "")
	var model := AshenveilDataStore.get_character_model(npc_id)
	var model_tag := ""
	if str(model.get("path", "")).is_empty():
		model_tag = "prototype capsule"
	else:
		model_tag = model.get("path", "").get_file()
	_label.text = "%s\n%s\n%s" % [display_name, role, model_tag]
