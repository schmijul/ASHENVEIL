extends Node

signal inventory_changed

var gold := 0
var items: Dictionary = {}
var equipment: Dictionary = {
	"weapon": null,
	"armor": null,
}

func add_item(item_id: String, quantity: int = 1) -> void:
	items[item_id] = items.get(item_id, 0) + quantity
	inventory_changed.emit()

func remove_item(item_id: String, quantity: int = 1) -> bool:
	var current_quantity := int(items.get(item_id, 0))
	if current_quantity < quantity:
		return false

	current_quantity -= quantity
	if current_quantity <= 0:
		items.erase(item_id)
	else:
		items[item_id] = current_quantity

	inventory_changed.emit()
	return true

func add_gold(amount: int) -> void:
	gold = max(0, gold + amount)
	inventory_changed.emit()

