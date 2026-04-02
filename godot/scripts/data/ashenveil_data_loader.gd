extends RefCounted
class_name AshenveilDataLoader

const DEFAULT_DATA_DIR := "res://../src/data"

const DEFAULT_ITEM := {
	"id": "",
	"name": "",
	"type": "unknown",
	"subtype": "",
	"slot": "",
	"damage": 0,
	"speed": 0.0,
	"defense": 0,
	"weight": 0.0,
	"price": 0,
	"description": "",
	"icon": "",
	"stackable": false,
	"maxStack": 1,
	"effect": "",
	"value": 0,
	"corruptionOnPickup": 0,
}

const DEFAULT_NPC := {
	"id": "",
	"name": "",
	"role": "",
	"location": "",
	"dialogue": {},
	"inventory": [],
}

const DEFAULT_QUEST := {
	"id": "",
	"title": "",
	"description": "",
	"phase": "",
	"giver": "",
	"optional": false,
	"prerequisite": "",
	"trigger": "",
	"autoStart": false,
	"objectives": [],
	"rewards": {},
	"onComplete": {},
}

const DEFAULT_ENEMY := {
	"id": "",
	"name": "",
	"health": 1,
	"damage": 0,
	"attackSpeed": 1.0,
	"moveSpeed": 0.0,
	"aggroRange": 0.0,
	"attackRange": 0.0,
	"behavior": {},
	"loot": [],
	"mesh": {},
	"faction": "",
	"isBoss": false,
	"staggerRequirement": "",
	"staggerDuration": 0.0,
}

const DEFAULT_CHARACTER_MODEL := {
	"id": "",
	"path": "",
	"scale": 1.0,
	"yOffset": 0.0,
	"rotationY": 0.0,
}

func load_all(source_dir: String = DEFAULT_DATA_DIR) -> Dictionary:
	var payload := {
		"items": [],
		"npcs": [],
		"quests": [],
		"enemies": [],
		"character_models": [],
		"warnings": [],
	}

	payload["items"] = _load_collection(source_dir, "items.json", "items", Callable(self, "_normalize_item"), payload["warnings"])
	payload["npcs"] = _load_collection(source_dir, "npcs.json", "npcs", Callable(self, "_normalize_npc"), payload["warnings"])
	payload["quests"] = _load_collection(source_dir, "quests.json", "quests", Callable(self, "_normalize_quest"), payload["warnings"])
	payload["enemies"] = _load_collection(source_dir, "enemies.json", "enemies", Callable(self, "_normalize_enemy"), payload["warnings"])
	payload["character_models"] = _load_character_models(source_dir, payload["warnings"])

	return payload

func load_indexed(source_dir: String = DEFAULT_DATA_DIR) -> Dictionary:
	var data := load_all(source_dir)
	return {
		"items_by_id": _index_by_id(data["items"]),
		"npcs_by_id": _index_by_id(data["npcs"]),
		"quests_by_id": _index_by_id(data["quests"]),
		"enemies_by_id": _index_by_id(data["enemies"]),
		"character_models_by_id": _index_by_id(data["character_models"]),
		"warnings": data["warnings"],
	}

func _load_collection(source_dir: String, file_name: String, key: String, normalizer: Callable, warnings: Array) -> Array:
	var root := _read_json_object(source_dir, file_name, warnings)
	if root.is_empty():
		return []

	var raw := root.get(key, [])
	if not (raw is Array):
		warnings.append("%s: expected array at key '%s'" % [file_name, key])
		return []

	var result: Array = []
	for entry in raw:
		if not (entry is Dictionary):
			warnings.append("%s: skipped non-dictionary entry" % file_name)
			continue

		var normalized: Dictionary = normalizer.call(entry)
		if normalized.get("id", "").is_empty():
			warnings.append("%s: skipped entry missing id" % file_name)
			continue
		result.append(normalized)

	return result

func _load_character_models(source_dir: String, warnings: Array) -> Array:
	var root := _read_json_object(source_dir, "characterModels.json", warnings)
	if root.is_empty():
		return []

	var result: Array = []
	for id in root.keys():
		var raw := root.get(id)
		if not (raw is Dictionary):
			warnings.append("characterModels.json: skipped model '%s' because it is not a dictionary" % str(id))
			continue
		var model := _normalize_character_model(raw, str(id))
		if model["id"].is_empty():
			continue
		result.append(model)

	return result

func _read_json_object(source_dir: String, file_name: String, warnings: Array) -> Dictionary:
	var path := _source_path(source_dir, file_name)
	if not FileAccess.file_exists(path):
		warnings.append("%s: file not found at %s" % [file_name, path])
		return {}

	var file := FileAccess.open(path, FileAccess.READ)
	if file == null:
		warnings.append("%s: unable to open %s" % [file_name, path])
		return {}

	var raw := file.get_as_text()
	var parsed := JSON.parse_string(raw)
	if parsed == null or not (parsed is Dictionary):
		warnings.append("%s: invalid JSON object" % file_name)
		return {}

	return parsed

func _source_path(source_dir: String, file_name: String) -> String:
	return ProjectSettings.globalize_path("%s/%s" % [source_dir.rstrip("/"), file_name])

func _index_by_id(entries: Array) -> Dictionary:
	var indexed := {}
	for entry in entries:
		if entry is Dictionary:
			var id := str(entry.get("id", ""))
			if not id.is_empty():
				indexed[id] = entry
	return indexed

func _to_int(value, fallback: int) -> int:
	if value is int:
		return value
	if value is float:
		return int(value)
	return fallback

func _to_float(value, fallback: float) -> float:
	if value is float:
		return value
	if value is int:
		return float(value)
	return fallback

func _to_bool(value, fallback: bool) -> bool:
	if value is bool:
		return value
	return fallback

func _to_string(value, fallback: String) -> String:
	if value is String:
		return value.strip_edges()
	return fallback

func _copy_defaults(defaults: Dictionary) -> Dictionary:
	var copy := {}
	for key in defaults.keys():
		var value = defaults[key]
		if value is Array:
			copy[key] = value.duplicate(true)
		elif value is Dictionary:
			copy[key] = value.duplicate(true)
		else:
			copy[key] = value
	return copy

func _normalize_item(raw: Dictionary) -> Dictionary:
	var item := _copy_defaults(DEFAULT_ITEM)
	item["id"] = _to_string(raw.get("id", ""), "")
	item["name"] = _to_string(raw.get("name", ""), "")
	item["type"] = _to_string(raw.get("type", item["type"]), item["type"])
	item["subtype"] = _to_string(raw.get("subtype", ""), "")
	item["slot"] = _to_string(raw.get("slot", ""), "")
	item["damage"] = _to_int(raw.get("damage", 0), 0)
	item["speed"] = _to_float(raw.get("speed", 0.0), 0.0)
	item["defense"] = _to_int(raw.get("defense", 0), 0)
	item["weight"] = _to_float(raw.get("weight", 0.0), 0.0)
	item["price"] = _to_int(raw.get("price", 0), 0)
	item["description"] = _to_string(raw.get("description", ""), "")
	item["icon"] = _to_string(raw.get("icon", ""), "")
	item["stackable"] = _to_bool(raw.get("stackable", false), false)
	item["maxStack"] = max(1, _to_int(raw.get("maxStack", 1), 1))
	item["effect"] = _to_string(raw.get("effect", ""), "")
	item["value"] = _to_int(raw.get("value", 0), 0)
	item["corruptionOnPickup"] = _to_int(raw.get("corruptionOnPickup", 0), 0)
	return item

func _normalize_dialogue_tree(raw) -> Dictionary:
	if raw is Dictionary:
		return raw.duplicate(true)
	return {}

func _normalize_npc(raw: Dictionary) -> Dictionary:
	var npc := _copy_defaults(DEFAULT_NPC)
	npc["id"] = _to_string(raw.get("id", ""), "")
	npc["name"] = _to_string(raw.get("name", ""), "")
	npc["role"] = _to_string(raw.get("role", ""), "")
	npc["location"] = _to_string(raw.get("location", ""), "")
	npc["dialogue"] = _normalize_dialogue_tree(raw.get("dialogue", {}))
	var inventory := raw.get("inventory", [])
	npc["inventory"] = inventory if inventory is Array else []
	return npc

func _normalize_objectives(raw) -> Array:
	if not (raw is Array):
		return []
	return raw.duplicate(true)

func _normalize_quest(raw: Dictionary) -> Dictionary:
	var quest := _copy_defaults(DEFAULT_QUEST)
	quest["id"] = _to_string(raw.get("id", ""), "")
	quest["title"] = _to_string(raw.get("title", ""), "")
	quest["description"] = _to_string(raw.get("description", ""), "")
	quest["phase"] = _to_string(raw.get("phase", ""), "")
	quest["giver"] = _to_string(raw.get("giver", ""), "")
	quest["optional"] = _to_bool(raw.get("optional", false), false)
	quest["prerequisite"] = _to_string(raw.get("prerequisite", ""), "")
	quest["trigger"] = _to_string(raw.get("trigger", ""), "")
	quest["autoStart"] = _to_bool(raw.get("autoStart", false), false)
	quest["objectives"] = _normalize_objectives(raw.get("objectives", []))
	quest["rewards"] = raw.get("rewards", {}) if raw.get("rewards", {}) is Dictionary else {}
	quest["onComplete"] = raw.get("onComplete", {}) if raw.get("onComplete", {}) is Dictionary else {}
	return quest

func _normalize_enemy(raw: Dictionary) -> Dictionary:
	var enemy := _copy_defaults(DEFAULT_ENEMY)
	enemy["id"] = _to_string(raw.get("id", ""), "")
	enemy["name"] = _to_string(raw.get("name", ""), "")
	enemy["health"] = max(1, _to_int(raw.get("health", 1), 1))
	enemy["damage"] = max(0, _to_int(raw.get("damage", 0), 0))
	enemy["attackSpeed"] = max(0.01, _to_float(raw.get("attackSpeed", 1.0), 1.0))
	enemy["moveSpeed"] = max(0.0, _to_float(raw.get("moveSpeed", 0.0), 0.0))
	enemy["aggroRange"] = max(0.0, _to_float(raw.get("aggroRange", 0.0), 0.0))
	enemy["attackRange"] = max(0.0, _to_float(raw.get("attackRange", 0.0), 0.0))
	enemy["behavior"] = raw.get("behavior", {}) if raw.get("behavior", {}) is Dictionary else {}
	enemy["loot"] = raw.get("loot", []) if raw.get("loot", []) is Array else []
	enemy["mesh"] = raw.get("mesh", {}) if raw.get("mesh", {}) is Dictionary else {}
	enemy["faction"] = _to_string(raw.get("faction", ""), "")
	enemy["isBoss"] = _to_bool(raw.get("isBoss", false), false)
	enemy["staggerRequirement"] = _to_string(raw.get("staggerRequirement", ""), "")
	enemy["staggerDuration"] = max(0.0, _to_float(raw.get("staggerDuration", 0.0), 0.0))
	return enemy

func _normalize_character_model(raw: Dictionary, id: String = "") -> Dictionary:
	var model := _copy_defaults(DEFAULT_CHARACTER_MODEL)
	model["id"] = id.strip_edges()
	model["path"] = _to_string(raw.get("path", ""), "")
	model["scale"] = max(0.01, _to_float(raw.get("scale", 1.0), 1.0))
	model["yOffset"] = _to_float(raw.get("yOffset", 0.0), 0.0)
	model["rotationY"] = _to_float(raw.get("rotationY", 0.0), 0.0)
	return model
