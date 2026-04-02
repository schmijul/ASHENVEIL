extends CanvasLayer

func _ready() -> void:
	var panel := _make_panel("Trade", "Merchant UI placeholder. Active quests: %s" % QuestState.get_active_quest_title())
	add_child(panel)

func _make_panel(title: String, body_text: String) -> Control:
	var panel := PanelContainer.new()
	panel.offset_left = 580
	panel.offset_top = 280
	panel.custom_minimum_size = Vector2(360, 220)

	var stack := VBoxContainer.new()
	panel.add_child(stack)

	var title_label := Label.new()
	title_label.text = title
	stack.add_child(title_label)

	var body := Label.new()
	body.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
	body.text = body_text
	stack.add_child(body)

	return panel

