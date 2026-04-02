extends CanvasLayer

func _ready() -> void:
	var panel := _make_panel("Inventory", "Gold: %d | Items: %d" % [InventoryState.gold, InventoryState.items.size()])
	add_child(panel)

func _make_panel(title: String, body_text: String) -> Control:
	var panel := PanelContainer.new()
	panel.offset_left = 580
	panel.offset_top = 24
	panel.custom_minimum_size = Vector2(360, 240)

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

