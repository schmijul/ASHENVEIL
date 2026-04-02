extends Node3D

@export var enemy_id := "boar"

func _ready() -> void:
	_build_visual()
	_build_hit_area()

func _build_visual() -> void:
	var boar := MeshInstance3D.new()
	var body_mesh := BoxMesh.new()
	body_mesh.size = Vector3(1.5, 0.75, 0.85)
	boar.mesh = body_mesh
	boar.position = Vector3(0, 0.65, 0)
	var material := StandardMaterial3D.new()
	material.albedo_color = Color(0.34, 0.22, 0.16, 1.0)
	material.roughness = 1.0
	boar.material_override = material
	add_child(boar)

func _build_hit_area() -> void:
	var area := Area3D.new()
	add_child(area)

	var shape_node := CollisionShape3D.new()
	var shape := BoxShape3D.new()
	shape.size = Vector3(1.6, 0.8, 1.0)
	shape_node.shape = shape
	shape_node.position = Vector3(0, 0.6, 0)
	area.add_child(shape_node)
