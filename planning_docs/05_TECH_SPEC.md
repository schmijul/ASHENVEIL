# ASHENVEIL — Technical Specification

## Tech Stack (Prototype)

| Layer | Technology | Reason |
|-------|-----------|--------|
| **Rendering** | Three.js r160+ | Industry standard WebGL, huge ecosystem |
| **Framework** | React Three Fiber (R3F) | Declarative 3D in React, fast iteration |
| **Physics** | @react-three/rapier | Rust-based physics via WASM, performant |
| **State** | Zustand | Lightweight, works perfectly with R3F |
| **UI Overlay** | React + Tailwind CSS | HUD, inventory, dialogue boxes |
| **Audio** | Howler.js | Spatial audio, cross-browser |
| **Build** | Vite | Fast HMR, good for iteration |

## Project Structure

```
ashenveil/
├── public/
│   ├── models/          # .glb 3D models
│   ├── textures/        # Texture atlases, terrain
│   ├── audio/           # Sound effects, ambient
│   └── index.html
├── src/
│   ├── main.jsx         # Entry point
│   ├── App.jsx          # Root component, scene setup
│   ├── store/
│   │   ├── gameStore.js       # Main game state (Zustand)
│   │   ├── inventoryStore.js  # Inventory state
│   │   ├── questStore.js      # Quest tracking
│   │   └── factionStore.js    # Faction reputation
│   ├── components/
│   │   ├── world/
│   │   │   ├── Terrain.jsx        # Ground/terrain mesh
│   │   │   ├── Forest.jsx         # Tree placement, foliage
│   │   │   ├── Village.jsx        # Building placement
│   │   │   ├── AetherCrystals.jsx # Crystal formations
│   │   │   ├── Water.jsx          # Stream/water shader
│   │   │   └── Skybox.jsx         # Sky, lighting, fog
│   │   ├── entities/
│   │   │   ├── Player.jsx         # Player model, animations
│   │   │   ├── NPC.jsx            # NPC base component
│   │   │   ├── Enemy.jsx          # Enemy base component
│   │   │   ├── Boar.jsx           # Boar AI + model
│   │   │   ├── CorruptedWolf.jsx  # Mini-boss
│   │   │   └── Soldier.jsx        # Kernwall soldiers
│   │   ├── systems/
│   │   │   ├── CombatSystem.jsx   # Hit detection, damage calc
│   │   │   ├── CameraController.jsx # Third-person camera
│   │   │   ├── InputManager.jsx   # Keyboard/mouse bindings
│   │   │   ├── AetherSystem.jsx   # Aether powers + corruption
│   │   │   ├── DayNightCycle.jsx  # Lighting changes
│   │   │   └── QuestTrigger.jsx   # Zone/event triggers
│   │   └── ui/
│   │       ├── HUD.jsx            # Health, stamina, corruption bars
│   │       ├── Inventory.jsx      # Inventory screen
│   │       ├── DialogueBox.jsx    # NPC dialogue UI
│   │       ├── TradeScreen.jsx    # Buy/sell interface
│   │       ├── MainMenu.jsx       # Start screen
│   │       └── QuestLog.jsx       # Active/completed quests
│   ├── data/
│   │   ├── npcs.json          # NPC definitions, dialogue trees
│   │   ├── items.json         # All items, stats, prices
│   │   ├── enemies.json       # Enemy stats, loot tables
│   │   ├── quests.json        # Quest definitions, triggers
│   │   └── recipes.json       # Crafting recipes
│   ├── utils/
│   │   ├── math.js            # Game math helpers
│   │   ├── pathfinding.js     # Simple A* for NPC movement
│   │   └── saveLoad.js        # LocalStorage save/load
│   └── shaders/
│       ├── aetherGlow.glsl    # Aether crystal shader
│       ├── corruption.glsl    # Screen corruption effect
│       └── water.glsl         # Stylized water shader
├── package.json
├── vite.config.js
└── README.md
```

## Core Systems Architecture

### Game State (Zustand)

```javascript
// gameStore.js — central state
{
  // Player
  player: {
    position: [0, 0, 0],
    rotation: 0,
    health: 100,
    maxHealth: 100,
    stamina: 100,
    maxStamina: 100,
    corruption: 0,
    gold: 0,
    skills: {},
    equipment: { weapon: null, armor: null, ... }
  },

  // World state
  world: {
    timeOfDay: 0.0,        // 0.0 - 1.0 (day cycle)
    questFlags: {},         // "metMaren": true, "soldBoarMeat": true, etc.
    destroyedVillage: false,
    aetherAwakened: false
  },

  // Faction rep
  factions: {
    kernwall: 0,
    flimmermoor: 0,
    hohensang: 0
  }
}
```

### Combat Hit Detection
- Melee: Sphere-cast from weapon tip along swing arc
- Aether Pulse: Sphere overlap check, fixed radius from player
- Damage formula: `baseDamage * weaponMultiplier * (1 - blockReduction)`
- Stagger: Heavy attacks and Aether Pulse apply stagger. Staggered enemies can't act for 1 second.

### Camera System
- Third-person orbit camera (like Gothic/Witcher)
- Mouse controls rotation, scroll controls distance
- Camera collision with environment (push forward when hitting walls)
- Combat mode: camera pulls back slightly for better spatial awareness

### NPC Dialogue System
- JSON-driven dialogue trees
- Each dialogue node has: text, options[], conditions (faction rep, quest flags, skills)
- Options can trigger: quest start, reputation change, trade screen, combat

```json
{
  "npc": "korvin",
  "nodes": {
    "greeting": {
      "text": "Fresh meat? I'll take what you've got.",
      "options": [
        { "text": "I want to sell.", "action": "openTrade" },
        { "text": "What's the news?", "next": "rumors" },
        { "text": "Nevermind.", "action": "close" }
      ]
    }
  }
}
```

### Save System
- Auto-save at quest milestones
- Manual save via menu
- LocalStorage for prototype (JSON serialization of full game state)
- Save includes: player state, world flags, inventory, faction rep, quest progress

## Performance Targets (Browser)

| Metric | Target |
|--------|--------|
| FPS | 60 on mid-range GPU, 30 minimum |
| Draw calls | < 200 per frame |
| Triangles | < 100k visible |
| Texture memory | < 256MB |
| Load time | < 5 seconds |

### Optimization Strategies
- Instanced meshes for trees, grass, rocks
- LOD (Level of Detail) for distant objects
- Frustum culling (built into Three.js)
- Texture atlases instead of individual textures
- Simple shadow maps, no real-time GI

## Asset Pipeline

### 3D Models
- **Format:** .glb (binary glTF)
- **Style:** Low-poly Valheim aesthetic — flat shading, hand-painted texture vibes, but vertex colors are acceptable for prototype
- **Generation strategy for prototype:** Procedural geometry (Three.js primitives + noise) where possible, simple Blender models where needed
- **Poly budget per asset:**
  - Player character: 2000-3000 tris
  - NPCs: 1500-2500 tris
  - Buildings: 500-1500 tris
  - Trees: 200-500 tris (instanced)
  - Animals/enemies: 500-1500 tris

### Audio
- Ambient forest loops
- Combat sound effects (sword swing, hit, block)
- UI sounds (inventory open, buy/sell, quest complete)
- Aether sounds (crystalline hum, pulse ability, corruption crackle)

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

## Migration Path (Post-Prototype)

If prototype validates the concept:
1. **Godot 4** is the recommended engine for full game
   - GDScript is Python-like (fits agentic coding)
   - Native 3D with PBR pipeline
   - Open source, no royalties
   - Export to PC, console, web
2. Port game logic as-is (state management, dialogue, quests)
3. Rebuild rendering in Godot's scene system
4. Upgrade assets from low-poly to mid-poly with proper PBR materials
5. Add proper animation system (skeletal animation, blend trees)
