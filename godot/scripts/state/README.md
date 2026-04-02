# Godot State Runtime

`AshenveilDataStore` is the canonical native runtime for the repo-owned JSON content.

Autoload:
- `AshenveilDataStore` from `res://scripts/state/ashenveil_data_store.gd`

Public API:
- `load_data()`
- `reload()`
- `get_item(id)`
- `get_npc(id)`
- `get_npc_dialogue(id)`
- `get_quest(id)`
- `get_enemy(id)`
- `get_character_model(id)`

The store normalizes data into indexed dictionaries and keeps gameplay code independent from raw file parsing.

