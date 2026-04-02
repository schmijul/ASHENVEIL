extends Node3D

const ForestScene := preload("res://scenes/world/forest.tscn")
const VillageScene := preload("res://scenes/world/village.tscn")
const FOREST_COLOR := Color(0.22, 0.34, 0.2, 1.0)

func _ready() -> void:
	_build_environment()
	_build_ground()
	_attach_world_blocks()

func _build_environment() -> void:
	var environment := WorldEnvironment.new()
	var env := Environment.new()
	env.background_mode = Environment.BG_COLOR
	env.background_color = Color(0.76, 0.72, 0.64, 1.0)
	env.ambient_light_source = Environment.AMBIENT_SOURCE_SKY
	env.ambient_light_color = Color(0.79, 0.74, 0.62, 1.0)
	env.ambient_light_energy = 1.15
	env.fog_enabled = true
	env.fog_light_color = Color(0.8, 0.75, 0.67, 1.0)
	env.fog_density = 0.015
	environment.environment = env
	add_child(environment)

	var sun := DirectionalLight3D.new()
	sun.light_energy = 1.5
	sun.light_color = Color(1.0, 0.84, 0.63, 1.0)
	sun.rotation_degrees = Vector3(-45, 35, 0)
	sun.shadow_enabled = true
	sun.shadow_bias = -0.03
	sun.shadow_normal_bias = 0.8
	add_child(sun)

func _build_ground() -> void:
	var ground_body := StaticBody3D.new()
	add_child(ground_body)

	var collision := CollisionShape3D.new()
	var shape := BoxShape3D.new()
	shape.size = Vector3(140, 1.0, 140)
	collision.shape = shape
	collision.position = Vector3(0, -0.5, 0)
	ground_body.add_child(collision)

	var mesh_instance := MeshInstance3D.new()
	var mesh := PlaneMesh.new()
	mesh.size = Vector2(140, 140)
	mesh.subdivide_width = 1
	mesh.subdivide_depth = 1
	mesh_instance.mesh = mesh
	mesh_instance.position = Vector3(0, 0.0, 0)
	mesh_instance.rotation_degrees = Vector3(-90, 0, 0)
	var material := StandardMaterial3D.new()
	material.albedo_color = FOREST_COLOR
	material.roughness = 0.95
	mesh_instance.material_override = material
	ground_body.add_child(mesh_instance)

func _attach_world_blocks() -> void:
	var forest := ForestScene.instantiate()
	forest.position = Vector3(0, 0, -4)
	add_child(forest)

	var village := VillageScene.instantiate()
	village.position = Vector3(0, 0, 0)
	add_child(village)
