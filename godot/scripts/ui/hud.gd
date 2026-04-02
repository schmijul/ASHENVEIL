extends CanvasLayer

@onready var status_label := Label.new()
@onready var hint_label := Label.new()

func _ready() -> void:
	_build_ui()

func _process(_delta: float) -> void:
	status_label.text = "Ashenveil Native Slice\nPlayer: %s\nGold: %d\nQuest: %s\nData: %d items / %d NPCs / %d quests / %d enemies" % [
		str(GameState.player_position),
		InventoryState.gold,
		QuestState.get_active_quest_title(),
		AshenveilDataStore.get_all_items().size(),
		AshenveilDataStore.get_all_npcs().size(),
		AshenveilDataStore.get_all_quests().size(),
		AshenveilDataStore.get_all_enemies().size(),
	]

func _build_ui() -> void:
	var panel := PanelContainer.new()
	panel.offset_left = 24
	panel.offset_top = 24
	panel.custom_minimum_size = Vector2(340, 120)
	add_child(panel)

	var margins := MarginContainer.new()
	margins.add_theme_constant_override("margin_left", 12)
	margins.add_theme_constant_override("margin_top", 12)
	margins.add_theme_constant_override("margin_right", 12)
	margins.add_theme_constant_override("margin_bottom", 12)
	panel.add_child(margins)

	var stack := VBoxContainer.new()
	stack.add_theme_constant_override("separation", 8)
	margins.add_child(stack)

	status_label.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
	stack.add_child(status_label)

	hint_label.text = "WASD move | Mouse look | Shift sprint | E interact | Esc toggle mouse"
	hint_label.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
	stack.add_child(hint_label)
