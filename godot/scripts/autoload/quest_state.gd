extends Node

signal quests_changed

var active_quests: Dictionary = {}
var completed_quests: Array[String] = []

func start_quest(quest_id: String) -> void:
	if completed_quests.has(quest_id):
		return
	active_quests[quest_id] = true
	quests_changed.emit()

func complete_quest(quest_id: String) -> void:
	active_quests.erase(quest_id)
	if not completed_quests.has(quest_id):
		completed_quests.append(quest_id)
	quests_changed.emit()

func get_active_quest_title() -> String:
	var quest_id := get_active_quest_id()
	if quest_id.is_empty():
		return "None"
	var quest := AshenveilDataStore.get_quest(quest_id)
	var title := str(quest.get("title", quest_id))
	return title if not title.is_empty() else quest_id

func get_active_quest_id() -> String:
	if active_quests.is_empty():
		return ""
	return str(active_quests.keys()[0])
