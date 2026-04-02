extends CanvasLayer

func _ready() -> void:
	var panel := _make_panel("Dialogue", "Dialogue state: %s" % DialogueState.active_npc_id)
	add_child(panel)

func _make_panel(title: String, body_text: String) -> Control:
	var panel := PanelContainer.new()
	panel.offset_left = 24
	panel.offset_bottom = -24
	panel.offset_right = 520
	panel.offset_top = -200
	panel.anchor_top = 1.0
	panel.anchor_bottom = 1.0

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

