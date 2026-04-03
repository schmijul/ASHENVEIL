extends CharacterBody3D

const CharacterVisualFactoryScript := preload("res://scripts/entities/character_visual_factory.gd")

@export var walk_speed := 4.6
@export var sprint_speed := 7.1
@export var ground_acceleration := 16.0
@export var ground_deceleration := 22.0
@export var air_acceleration := 3.5
@export var gravity := 24.0
@export var mouse_sensitivity := 0.0025
@export var camera_yaw_sensitivity := 0.85
@export var camera_pitch_min := -0.48
@export var camera_pitch_max := 0.18

@onready var visual: Node3D = $Visual
@onready var head_pivot: Node3D = $HeadPivot

var _pitch := -0.1
var _target_visual_yaw := 0.0

func _ready() -> void:
	_ensure_character_visual()
	_apply_camera_pitch()
	GameState.register_player(self)

func _unhandled_input(event: InputEvent) -> void:
	if event is InputEventMouseMotion and Input.get_mouse_mode() == Input.MOUSE_MODE_CAPTURED:
		var yaw_delta: float = -event.relative.x * mouse_sensitivity * camera_yaw_sensitivity
		head_pivot.rotate_y(yaw_delta)
		_pitch = clamp(_pitch - event.relative.y * mouse_sensitivity, camera_pitch_min, camera_pitch_max)
		_apply_camera_pitch()
	elif event.is_action_pressed("pause"):
		if Input.get_mouse_mode() == Input.MOUSE_MODE_CAPTURED:
			Input.set_mouse_mode(Input.MOUSE_MODE_VISIBLE)
		else:
			Input.set_mouse_mode(Input.MOUSE_MODE_CAPTURED)

func _physics_process(delta: float) -> void:
	var input_vector := Input.get_vector("move_left", "move_right", "move_forward", "move_backward")
	var movement := Vector3.ZERO

	if input_vector != Vector2.ZERO:
		var camera_basis := head_pivot.global_transform.basis
		var forward := -camera_basis.z
		forward.y = 0.0
		forward = forward.normalized()
		var right := camera_basis.x
		right.y = 0.0
		right = right.normalized()
		movement = (right * input_vector.x + forward * input_vector.y).normalized()

	var target_speed := sprint_speed if Input.is_action_pressed("sprint") else walk_speed
	var target_velocity := movement * target_speed
	var acceleration := ground_acceleration if is_on_floor() else air_acceleration
	var deceleration := ground_deceleration if is_on_floor() else air_acceleration

	velocity.x = move_toward(velocity.x, target_velocity.x, (acceleration if abs(target_velocity.x) > abs(velocity.x) else deceleration) * delta)
	velocity.z = move_toward(velocity.z, target_velocity.z, (acceleration if abs(target_velocity.z) > abs(velocity.z) else deceleration) * delta)

	if not is_on_floor():
		velocity.y -= gravity * delta
	else:
		velocity.y = 0.0

	move_and_slide()

	if movement != Vector3.ZERO:
		var target_yaw: float = atan2(-movement.x, -movement.z)
		rotation.y = lerp_angle(rotation.y, target_yaw, delta * 10.0)
		_target_visual_yaw = target_yaw - rotation.y
	else:
		_target_visual_yaw = 0.0

	visual.rotation.y = lerp_angle(visual.rotation.y, _target_visual_yaw, delta * 8.0)
	visual.position.y = lerp(visual.position.y, 0.88 if is_on_floor() else 0.94, delta * 10.0)

	GameState.set_player_position(global_position)

func _apply_camera_pitch() -> void:
	head_pivot.rotation.x = _pitch

func _ensure_character_visual() -> void:
	for child in visual.get_children():
		child.queue_free()
	var factory := CharacterVisualFactoryScript.new()
	var character_visual: Node3D = factory.create_character_visual("player")
	character_visual.name = "CharacterVisual"
	visual.add_child(character_visual)
