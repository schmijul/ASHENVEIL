# ASHENVEIL

Ashenveil is being migrated to a native Godot 4 action RPG for Ubuntu. The active design target is a dark-fantasy / Witcher-style vertical slice with grounded third-person camera, forest and village exploration, dialogue, trading, combat, and the opening quest chain.

## Current Status

- The long-term target is a native Ubuntu build in Godot 4 with GDScript
- Planning docs define the migration roadmap, visual direction, and tutorial slice
- The legacy browser prototype remains a reference point during the transition

## Stack

- Godot 4
- GDScript
- Native Ubuntu export
- JSON-driven content for items, NPCs, quests, enemies, and model mappings

## Commands

Once the native project is in place:

```bash
cd /home/schmijul/fun/ASHENVEIL
tools/asset_pipeline/sync_assets.sh
./.tools/Godot_v4.3-stable_linux.x86_64 --path ./godot
```

Or open the Godot project directly through the editor and run the main scene from there.

Run legacy unit tests:

```bash
cd /home/schmijul/fun/ASHENVEIL/legacy_web
npm test
```

## Project Layout

```text
godot/           # native Godot project
legacy_web/      # archived browser prototype during migration
planning_docs/   # design, tech, art, and roadmap docs
```

## Branch Strategy

- `feat/native-godot-migration`: integration branch for the native rebuild
- Subsystem branches are used for docs, bootstrap, camera/player, world, data, and gameplay systems

## Notes

- `planning_docs/data/` stays untouched unless a doc update explicitly requires it
- Runtime gameplay data should stay external and editable
- Visual direction is driven by the reference images in `design_inspiration/`
- Third-party visual assets are synced from `tools/asset_pipeline/asset_manifest.json`
- License tracking for downloaded assets is written to `godot/assets/THIRD_PARTY_ASSETS.md`
