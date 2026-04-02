extends Node

signal dialogue_started(npc_id: String)
signal dialogue_finished

var active_npc_id := ""
var active_node_id := ""

func start_dialogue(npc_id: String, node_id: String = "") -> void:
	active_npc_id = npc_id
	active_node_id = node_id
	dialogue_started.emit(npc_id)

func close_dialogue() -> void:
	active_npc_id = ""
	active_node_id = ""
	dialogue_finished.emit()

