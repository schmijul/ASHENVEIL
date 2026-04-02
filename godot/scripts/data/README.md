# Godot Data Runtime

These scripts load the repo-owned JSON data from `src/data/` so the native Godot port can keep the same content source of truth.

Expected autoload:
- `AshenveilDataStore` from `godot/scripts/state/ashenveil_data_store.gd`

Default data source:
- `res://../src/data`

Key APIs:
- `get_item(id)`
- `get_npc(id)`
- `get_npc_dialogue(id)`
- `get_quest(id)`
- `get_enemy(id)`
- `get_character_model(id)`

Return values are normalized dictionaries with safe defaults, so the runtime can boot even if a data file is partial during migration.
