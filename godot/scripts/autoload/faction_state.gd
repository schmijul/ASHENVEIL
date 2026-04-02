extends Node

signal reputation_changed(faction_id: String, value: int)

var reputation: Dictionary = {
	"kernwall": 0,
	"flimmermoor": 0,
	"hohensang": 0,
}

func add_reputation(faction_id: String, amount: int) -> void:
	reputation[faction_id] = int(reputation.get(faction_id, 0)) + amount
	reputation_changed.emit(faction_id, int(reputation[faction_id]))

