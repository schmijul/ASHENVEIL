extends Node3D

const CharacterVisualFactoryScript := preload("res://scripts/entities/character_visual_factory.gd")

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
		_label.visible = false
		add_child(_label)

	var npc: Dictionary = AshenveilDataStore.get_npc(npc_id)
	var role: String = str(npc.get("role", ""))
	var factory := CharacterVisualFactoryScript.new()
	var model_root: Node3D = factory.create_character_visual(npc_id, role)
	add_child(model_root)

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
