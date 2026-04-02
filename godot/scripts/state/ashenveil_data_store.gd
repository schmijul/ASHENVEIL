extends Node
class_name AshenveilDataStore

const DEFAULT_DATA_DIR := AshenveilDataLoader.DEFAULT_DATA_DIR

signal data_loaded
signal data_load_failed(warnings: Array)

@export var source_dir: String = DEFAULT_DATA_DIR
@export var auto_load_on_ready: bool = true

var items_by_id: Dictionary = {}
var npcs_by_id: Dictionary = {}
var quests_by_id: Dictionary = {}
var enemies_by_id: Dictionary = {}
var character_models_by_id: Dictionary = {}
var warnings: Array = []
var is_loaded: bool = false

func _ready() -> void:
	if auto_load_on_ready:
		load_data()

func load_data() -> bool:
	var loader := AshenveilDataLoader.new()
	var payload := loader.load_indexed(source_dir)
	items_by_id = payload["items_by_id"]
	npcs_by_id = payload["npcs_by_id"]
	quests_by_id = payload["quests_by_id"]
	enemies_by_id = payload["enemies_by_id"]
	character_models_by_id = payload["character_models_by_id"]
	warnings = payload["warnings"]
	is_loaded = not items_by_id.is_empty() or not npcs_by_id.is_empty() or not quests_by_id.is_empty() or not enemies_by_id.is_empty() or not character_models_by_id.is_empty()

	if is_loaded:
		data_loaded.emit()
	else:
		data_load_failed.emit(warnings.duplicate(true))

	return is_loaded

func reload() -> bool:
	return load_data()

func get_item(id: String) -> Dictionary:
	return items_by_id.get(id, {})

func get_npc(id: String) -> Dictionary:
	return npcs_by_id.get(id, {})

func get_npc_dialogue(id: String) -> Dictionary:
	var npc := get_npc(id)
	var dialogue = npc.get("dialogue", {})
	return dialogue if dialogue is Dictionary else {}

func get_quest(id: String) -> Dictionary:
	return quests_by_id.get(id, {})

func get_enemy(id: String) -> Dictionary:
	return enemies_by_id.get(id, {})

func get_character_model(id: String) -> Dictionary:
	return character_models_by_id.get(id, {})

func get_all_items() -> Array:
	return items_by_id.values()

func get_all_npcs() -> Array:
	return npcs_by_id.values()

func get_all_quests() -> Array:
	return quests_by_id.values()

func get_all_enemies() -> Array:
	return enemies_by_id.values()

func get_quests_for_npc(npc_id: String) -> Array:
	var result: Array = []
	for quest in quests_by_id.values():
		if not (quest is Dictionary):
			continue
		if quest.get("giver", "") == npc_id:
			result.append(quest)
	return result

func get_quest_ids_for_npc(npc_id: String) -> Array:
	var result: Array = []
	for quest in get_quests_for_npc(npc_id):
		result.append(quest.get("id", ""))
	return result

func has_item(id: String) -> bool:
	return items_by_id.has(id)

func has_npc(id: String) -> bool:
	return npcs_by_id.has(id)

func has_quest(id: String) -> bool:
	return quests_by_id.has(id)

func has_enemy(id: String) -> bool:
	return enemies_by_id.has(id)

func has_character_model(id: String) -> bool:
	return character_models_by_id.has(id)
