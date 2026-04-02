# Ashenveil Native Prototype

This directory contains the native Godot 4 scaffold for Ashenveil on Ubuntu.

## Requirements

- Godot 4.3 or newer
- Ubuntu 22.04+ recommended

## Open the project

Open `godot/project.godot` in the Godot editor.

## Run from the command line

```bash
godot4 --path /home/schmijul/fun/ASHENVEIL/godot
```

If your binary is named differently:

```bash
godot --path /home/schmijul/fun/ASHENVEIL/godot
```

## Current scaffold

- `scenes/main.tscn` boots the native slice
- `scenes/world/world_root.tscn` composes forest + village world blocks
- `scenes/entities/player.tscn` provides a third-person capsule controller
- `scenes/entities/npc.tscn` and `scenes/entities/boar.tscn` are the first gameplay actors
- `scenes/ui/*.tscn` hold native HUD, dialogue, inventory, trade, and quest UI shells
- `scripts/state/ashenveil_data_store.gd` is the canonical JSON-backed runtime store
- `scripts/autoload/*.gd` define the singleton state layers for gameplay state

## Inputs

See `INPUT_MAP.md` for the runtime action map and control bindings.

## Data runtime

The native project reads from the repo-owned JSON data in `../src/data/` through `AshenveilDataStore`.
That store is the canonical source for items, NPCs, quests, enemies, and character model mappings.
