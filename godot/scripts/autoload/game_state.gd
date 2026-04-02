extends Node

signal player_registered
signal player_position_changed(position: Vector3)
signal world_flag_changed(flag_name: String, value: bool)

var player_node: Node3D
var player_position := Vector3.ZERO
var world_flags: Dictionary = {
	"prologue_started": true,
}

func register_player(node: Node3D) -> void:
	player_node = node
	player_registered.emit()

func set_player_position(position: Vector3) -> void:
	player_position = position
	player_position_changed.emit(position)

func set_world_flag(flag_name: String, value: bool) -> void:
	world_flags[flag_name] = value
	world_flag_changed.emit(flag_name, value)

func reset() -> void:
	player_node = null
	player_position = Vector3.ZERO
	world_flags = {
		"prologue_started": true,
	}

