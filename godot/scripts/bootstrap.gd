extends Node

const WorldScene := preload("res://scenes/world/world_root.tscn")
const PlayerScene := preload("res://scenes/entities/player.tscn")
const HudScene := preload("res://scenes/ui/hud.tscn")

const INPUT_ACTIONS := {
	"move_forward": {"keys": [KEY_W, KEY_UP]},
	"move_backward": {"keys": [KEY_S, KEY_DOWN]},
	"move_left": {"keys": [KEY_A, KEY_LEFT]},
	"move_right": {"keys": [KEY_D, KEY_RIGHT]},
	"sprint": {"keys": [KEY_SHIFT]},
	"attack_light": {"mouse": [MOUSE_BUTTON_LEFT]},
	"attack_heavy": {"mouse": [MOUSE_BUTTON_RIGHT]},
	"block": {"mouse": [MOUSE_BUTTON_RIGHT]},
	"dodge": {"keys": [KEY_SPACE]},
	"interact": {"keys": [KEY_E]},
	"inventory": {"keys": [KEY_I, KEY_TAB]},
	"quest_log": {"keys": [KEY_J]},
	"pause": {"keys": [KEY_ESCAPE]},
	"camera_reset": {"keys": [KEY_R]},
}

func _ready() -> void:
	_ensure_input_map()
	if not AshenveilDataStore.is_loaded:
		AshenveilDataStore.load_data()
	for warning in AshenveilDataStore.warnings:
		push_warning(warning)
	Input.set_mouse_mode(Input.MOUSE_MODE_CAPTURED)

	var world := WorldScene.instantiate()
	add_child(world)

	var player := PlayerScene.instantiate()
	player.global_position = Vector3(0, 1.4, 6)
	add_child(player)

	var hud := HudScene.instantiate()
	add_child(hud)

func _ensure_input_map() -> void:
	for action_name in INPUT_ACTIONS.keys():
		if not InputMap.has_action(action_name):
			InputMap.add_action(action_name)

		var spec: Dictionary = INPUT_ACTIONS[action_name]
		for keycode in spec.get("keys", []):
			_add_key_event(action_name, keycode)
		for button_index in spec.get("mouse", []):
			_add_mouse_event(action_name, button_index)

func _add_key_event(action_name: String, keycode: int) -> void:
	var event := InputEventKey.new()
	event.physical_keycode = keycode
	event.keycode = keycode
	InputMap.action_add_event(action_name, event)

func _add_mouse_event(action_name: String, button_index: int) -> void:
	var event := InputEventMouseButton.new()
	event.button_index = button_index
	InputMap.action_add_event(action_name, event)
