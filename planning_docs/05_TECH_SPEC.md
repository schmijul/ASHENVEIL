# ASHENVEIL - Technical Specification

## Tech Stack (Native Build)

| Layer | Technology | Reason |
|-------|-----------|--------|
| **Rendering** | Godot 4 PBR renderer | Native 3D, strong Linux support, fast iteration |
| **Language** | GDScript | Lightweight, readable, good fit for agentic implementation |
| **Physics** | Godot physics / CharacterBody3D / RigidBody3D | Integrated, simple, good enough for the vertical slice |
| **State** | Autoload singletons | Clear separation of game state and scene state |
| **UI** | Godot Control scenes and themes | Native HUD, dialogue, inventory, trade, quest log |
| **Audio** | Godot AudioServer and buses | Ambient layers, combat stingers, UI feedback |
| **Build** | Godot editor + Linux export templates | Native Ubuntu workflow |

## Project Structure

The native project should live in a dedicated `godot/` folder so the legacy web prototype can remain recoverable during the migration.

```
ashenveil/
├── godot/
│   ├── project.godot
│   ├── scenes/
│   │   ├── bootstrap/
│   │   ├── world/
│   │   ├── characters/
│   │   ├── ui/
│   │   └── encounters/
│   ├── scripts/
│   │   ├── autoload/
│   │   ├── player/
│   │   ├── camera/
│   │   ├── combat/
│   │   ├── dialogue/
│   │   ├── quest/
│   │   ├── inventory/
│   │   └── utils/
│   ├── assets/
│   │   ├── models/
│   │   ├── textures/
│   │   ├── materials/
│   │   └── audio/
│   └── data/
│       ├── items.json
│       ├── npcs.json
│       ├── enemies.json
│       ├── quests.json
│       └── characterModels.json
├── legacy_web/   # archived browser prototype during migration
├── planning_docs/
└── README.md
```

## Core Systems Architecture

### Game State

Use autoloads instead of component-local state:

- `GameState` for player stats, camera state, world flags, and debug hooks
- `InventoryState` for items, equipment, weight, and currency
- `QuestState` for active quests, objectives, rewards, and completion
- `DialogueState` for active NPC conversation and branching nodes
- `FactionState` for reputation per realm

### Scene Model

- `Boot` scene decides whether to load the main menu or the game world
- `WorldRoot` owns terrain, forest, village, and encounter zones
- `Player` uses `CharacterBody3D` and a separate visual root
- `CameraRig` handles shoulder framing, rotation, pitch, and collision push-in
- `UIRoot` mounts HUD, dialogue, inventory, trade, and quest widgets

### Data Flow

- Content stays in JSON where it is easy to edit: items, quests, NPCs, enemies, character model mappings
- Godot loads these files at runtime and maps them into plain runtime dictionaries or custom resources
- Scene scripts should not hardcode dialogue trees, item stats, or quest rewards
- Gameplay systems query state through autoloads, not through direct scene coupling

### Combat and Camera

- Combat hit detection should be native Godot overlap and ray-based logic
- Melee attacks use player-relative swing checks and short hit windows
- Aether Pulse is a radial area effect centered on the player
- Camera should stay low, close, and over the shoulder, not top-down
- Camera collision should push forward when walls or terrain obstruct the view
- In combat, the camera can widen slightly for better spatial awareness

### Save System

- Use `user://` for native save files
- Save should include player state, inventory, quests, world flags, faction reputation, and settings
- Keep autosave points tied to quest milestones or major transitions

## Performance Targets (Ubuntu Desktop)

| Metric | Target |
|--------|--------|
| FPS | 60 on mid-range GPU, 30 minimum |
| Draw complexity | Keep scenes readable and avoid unnecessary overdraw |
| Visible objects | Dense enough for atmosphere, but never at the cost of stable frame time |
| Startup | Game should boot to playable scene quickly on Ubuntu |

### Optimization Strategies
- Use instancing where Godot supports repeated scenery objects cleanly
- Use vertex colors and simple materials for most world props
- Keep the playable slice compact and authored, not systemically huge
- Prefer baked or simple dynamic lighting for the first vertical slice if performance requires it
- Use fog, lighting, and composition to fake density rather than brute-force detail

## Asset Pipeline

### 3D Models
- **Format:** GLB / glTF
- **Style:** Stylized-realistic Gothic / Witcher atmosphere
- **Generation strategy for prototype:** Use procedural or imported GLB assets, but keep the visual language grounded and believable
- **Asset classes:** player, NPCs, boars, rabbit, trees, buildings, crystals, props

### Materials
- Terrain and vegetation should rely on strong color design and simple roughness variation
- Emissive use should be rare and reserved for fire, lanterns, crystals, and Aether effects
- Avoid flat-shaded prototype reads in the final vertical slice unless it is an intentional low-detail placeholder

### Audio
- Ambient forest loops
- Village ambience
- Combat impacts and swing sounds
- Aether hum and corruption cues
- UI feedback for inventory, trade, quest updates

## Controls

| Action | Key | Alt |
|--------|-----|-----|
| Move | WASD | Arrow keys |
| Camera | Mouse | - |
| Light Attack | Left Click | - |
| Heavy Attack | Right Click (hold) | - |
| Block | Right Click (tap) | - |
| Dodge | Space | - |
| Aether Pulse | Q | - |
| Interact | E | - |
| Inventory | I | Tab |
| Quest Log | J | - |
| Pause/Menu | Escape | - |

## Quality Gates

- The game must run natively on Ubuntu without browser dependencies
- Every major system must be testable from a dedicated scene or debug path
- No gameplay state should remain trapped in scene-local variables
- Every task must finish with a bootable, playable, and reviewable build
- The vertical slice is not done until the camera, player feel, village, forest, dialogue, combat, and quest flow are all integrated
