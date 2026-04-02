extends CharacterBody3D

@export var walk_speed := 5.0
@export var sprint_speed := 8.0
@export var acceleration := 12.0
@export var gravity := 24.0
@export var mouse_sensitivity := 0.003

@onready var visual: Node3D = $Visual
@onready var head_pivot: Node3D = $HeadPivot

var _pitch := -0.1

func _ready() -> void:
	_apply_camera_pitch()
	GameState.register_player(self)

func _unhandled_input(event: InputEvent) -> void:
	if event is InputEventMouseMotion and Input.get_mouse_mode() == Input.MOUSE_MODE_CAPTURED:
		head_pivot.rotate_y(-event.relative.x * mouse_sensitivity)
		_pitch = clamp(_pitch - event.relative.y * mouse_sensitivity, -0.55, 0.2)
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

	velocity.x = move_toward(velocity.x, target_velocity.x, acceleration * delta)
	velocity.z = move_toward(velocity.z, target_velocity.z, acceleration * delta)

	if not is_on_floor():
		velocity.y -= gravity * delta
	else:
		velocity.y = -0.1

	move_and_slide()

	if movement != Vector3.ZERO:
		var target_yaw := atan2(-movement.x, -movement.z)
		rotation.y = lerp_angle(rotation.y, target_yaw, delta * 10.0)
		visual.rotation.y = lerp_angle(visual.rotation.y, 0.0, delta * 8.0)

	GameState.set_player_position(global_position)

func _apply_camera_pitch() -> void:
	head_pivot.rotation.x = _pitch

