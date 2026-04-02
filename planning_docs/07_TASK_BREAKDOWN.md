# ASHENVEIL — Task Breakdown for Agentic Coding System

## Instructions for AI Agent

You are building a browser-based 3D RPG prototype. Read ALL documents in this directory before starting any task. Each task below is a self-contained unit. Complete them in order. After each task, the result should be testable in the browser.

**Rules:**
- Use React Three Fiber + Zustand + Rapier physics
- Every component must be in its own file
- Game state lives in Zustand stores only — no component-local game state
- All game data (items, NPCs, quests) lives in JSON files in `src/data/`
- Test after every task. If it doesn't run, fix it before moving on.
- Commit after each completed task
- Refer to `06_ART_DIRECTION.md` for all visual decisions
- Refer to `03_GAME_MECHANICS.md` for all gameplay logic
- Refer to `04_TUTORIAL_DESIGN.md` for quest/level design

---

## Sprint 1: Foundation (Tasks 1-5)

### Task 1 — Project Setup
**Goal:** Empty Three.js scene running in browser
- Initialize Vite + React project
- Install: three, @react-three/fiber, @react-three/drei, @react-three/rapier, zustand, howler
- Create basic scene: camera, directional light, ambient light, ground plane
- Sky gradient matching art direction (midday colors)
- Distance fog enabled
- **Verify:** Browser shows a lit ground plane with sky and fog

### Task 2 — Player Character (Movement)
**Goal:** Controllable character in the scene
- Player is a simple capsule mesh for now (placeholder)
- WASD movement with physics body (Rapier)
- Third-person camera: orbit behind player, mouse controls rotation, scroll = zoom
- Camera collision (don't clip through terrain)
- Walking animation = slight bob (no skeletal animation yet)
- Sprint with Shift (faster movement, stamina drain later)
- **Verify:** Player capsule moves around ground plane, camera follows smoothly

### Task 3 — Terrain & Forest
**Goal:** Explorable forest environment
- Generate terrain: noise-based heightmap, gently rolling hills
- Terrain texture: vertex colors (green grass, brown paths, gray rocks)
- Place instanced trees: pine tree = cone on cylinder, oak = sphere on cylinder
- Tree placement: noise-based density, avoid paths and clearings
- Grass tufts: instanced small geometry, scattered on terrain
- Rocks: instanced low-poly rocks, various sizes
- Apply fog and lighting from art direction
- **Verify:** Player walks through a Valheim-style forest. Feels atmospheric.

### Task 4 — Village Grauweiler
**Goal:** Walkable village with buildings
- Create simple building meshes: box base + sloped roof prism
- Timber-frame look: darker edge lines or vertex color contrast
- Place buildings according to map layout in `04_TUTORIAL_DESIGN.md`
- Market square: open area with simple stall meshes
- Forge: open structure with orange point light (fire)
- Add details: fence meshes, barrel meshes, lantern meshes with point lights
- Smoke particles from chimneys (simple billboard particles rising)
- **Verify:** Village feels like a small inhabited settlement. Player can walk through it.

### Task 5 — Collision & Physics
**Goal:** Player can't walk through buildings or trees
- Add colliders to all buildings (box colliders)
- Add colliders to tree trunks (cylinder colliders)
- Add colliders to rocks (convex hull or box)
- Terrain has a trimesh collider
- Player physics: capsule collider, gravity, slopes
- **Verify:** Player collides with all world objects properly. No clipping.

---

## Sprint 2: Core Systems (Tasks 6-10)

### Task 6 — Game State Store
**Goal:** Central game state management
- Create Zustand stores per `05_TECH_SPEC.md`:
  - `gameStore.js` — player stats, world flags
  - `inventoryStore.js` — items, equipment
  - `questStore.js` — active/completed quests
  - `factionStore.js` — reputation values
- Player stats: health, maxHealth, stamina, maxStamina, corruption, gold
- **Verify:** State updates correctly when modified (console.log tests)

### Task 7 — HUD
**Goal:** Health, stamina, corruption bars on screen
- React overlay (not 3D): fixed position UI
- Health bar: red, bottom-left
- Stamina bar: green, below health. Fades when full, appears when draining.
- Corruption bar: purple, only visible when > 0%
- Gold counter: bottom-right, small icon + number
- Minimap: stretch goal, skip if complex
- All bars read from Zustand store
- **Verify:** Bars display and update when state changes

### Task 8 — Combat System (Melee)
**Goal:** Player can attack and take damage
- Light attack: left click. Swing animation (rotate weapon placeholder). Sphere-cast hit detection.
- Heavy attack: hold right click + release. Slower, more damage, staggers.
- Block: tap right click. Damage reduction while held, stamina drain.
- Dodge roll: spacebar. Quick movement in input direction, i-frames.
- Stamina: all actions cost stamina. Regen when idle.
- Damage numbers: small floating text on hit (optional but nice).
- Player hit: flash red, brief knockback.
- **Verify:** Player can perform all combat actions against a test dummy

### Task 9 — Enemy AI (Boar)
**Goal:** Boars that can be hunted
- Boar mesh: low-poly (elongated box body, small cone legs, triangle tusks)
- AI states: Idle (wander randomly), Alert (player within 15m), Chase (run at player), Attack (charge, 1-2 sec windup), Flee (low health)
- Boar stats from `04_TUTORIAL_DESIGN.md`
- Spawn 5-6 boars in the forest hunting area
- Death: ragdoll or fall-over animation, drop loot (meat, pelt)
- Loot: press E near dead boar to collect
- **Verify:** Player can find, fight, kill boars and collect loot

### Task 10 — Inventory System
**Goal:** Inventory screen with items
- Press I/Tab to open inventory
- Grid layout: 6 columns, scrollable
- Items show: icon (colored rectangle with letter for prototype), name, quantity
- Equipment panel: slots for weapon, armor, etc.
- Drag to equip (or click to equip)
- Item data loaded from `src/data/items.json`
- Weight system: total weight shown, movement slows if overweight
- **Verify:** Picked up boar meat/pelts appear in inventory. Can equip a weapon.

---

## Sprint 3: Interaction (Tasks 11-15)

### Task 11 — NPC System
**Goal:** NPCs you can talk to
- NPC mesh: colored capsule with simple features (prototype)
- NPCs placed per village layout
- Name label floating above NPC (drei Text)
- Walk up + press E = open dialogue
- NPCs face player when in conversation
- **Verify:** Player can approach and interact with NPCs, names visible

### Task 12 — Dialogue System
**Goal:** Branching dialogue from JSON
- Dialogue box: bottom of screen, semi-transparent panel
- NPC name + text displayed, typewriter effect
- Player response options listed as clickable buttons
- Dialogue data from `src/data/npcs.json`
- Conditions: check quest flags, faction rep, inventory items
- Actions: start quest, give item, open trade, change reputation
- **Verify:** Full dialogue tree with Maren works, including quest assignment

### Task 13 — Trading System
**Goal:** Buy and sell items with Korvin
- Trade screen: split view (player inventory | merchant inventory)
- Prices from items.json
- Buy: click item in merchant inventory, pay gold, item moves to player
- Sell: click item in player inventory, receive gold, item moves to merchant
- Price display on hover
- Gold deducted/added to player store
- **Verify:** Sell boar meat to Korvin, receive gold, buy hunting knife

### Task 14 — Quest System
**Goal:** Trackable quests with objectives
- Quest definitions in `src/data/quests.json`
- Quest structure: id, title, description, objectives[], rewards
- Objective types: kill (enemy, count), collect (item, count), talk (npc), visit (location)
- HUD: small quest tracker top-right showing active quest + current objective
- Quest log screen (J): list of active and completed quests
- Quest completion: reward gold/items, set world flags
- **Verify:** "Hunt boars" quest tracks kills, completes when 3 pelts collected

### Task 15 — Tutorial Quest Chain
**Goal:** Full Phase 1 + Phase 2 playable
- Implement quest: "Maren's Request" — hunt 3 boars, get pelts
- Implement quest: "Sell to Korvin" — sell meat, earn gold
- Implement quest: "Gear Up" — buy weapon from Hagen
- Optional quest: "Lotte's Herbs" — collect 3 herbs, reward potions
- Optional quest: "Hagen's Tusk" — bring boar tusk, reward weapon upgrade
- Optional: "Ren's Training" — sparring encounter, teaches heavy attack
- Quest flow triggers correctly: completing one unlocks the next
- **Verify:** Full tutorial Phase 1 + Phase 2 playable start to finish

---

## Sprint 4: Aether & Finale (Tasks 16-20)

### Task 16 — Aether Crystal Clearing
**Goal:** New area with Aether visuals
- Deep forest area beyond normal hunting grounds (new zone)
- Crystal meshes: octahedrons + elongated prisms, emissive cyan material
- Point lights on crystals, pulsing intensity (sine wave)
- Ground corruption: vertex colors transition to gray/purple near crystals
- Floating particle motes (small spheres, slow orbit)
- Ambient sound: crystalline hum (synthesized or placeholder)
- **Verify:** Area is visually distinct and atmospheric. Aether feels alien and dangerous.

### Task 17 — Aether Pulse Ability
**Goal:** Player's first magic ability
- Press Q: Aether Pulse
- Visual: expanding cyan sphere from player hands, fades quickly
- Effect: knockback + stagger to enemies in range (5m radius)
- Cost: fills corruption meter by 5% per use
- Corruption visual: screen edges get subtle purple tint
- Corruption decay: 1% per 10 seconds when not using Aether
- Corruption > 50%: chromatic aberration post-processing increases
- **Verify:** Aether Pulse knocks back enemies, corruption meter fills and decays

### Task 18 — Corrupted Wolf (Mini-Boss)
**Goal:** Boss encounter requiring Aether
- Corrupted Wolf mesh: wolf shape with crystal growths (elongated prisms attached to back)
- Glowing cyan eyes (emissive)
- AI: circles player, fast lunge attacks, high damage
- Special: takes reduced damage unless staggered with Aether Pulse first
- Stats from `04_TUTORIAL_DESIGN.md`
- Encounter trigger: approach crystal clearing after Phase 2 quest completion
- Scripted: Maren sends you to investigate noise
- **Verify:** Fight requires using Aether Pulse strategically. Feels challenging but fair.

### Task 19 — Village Destruction Sequence
**Goal:** Phase 4 story event
- Trigger: return to village after wolf kill
- Visual: smoke particles rising from village direction while still in forest
- Arriving at village: 2 buildings on fire (fire particles + orange light), 1 collapsed
- Kernwall soldiers (3-4): standing in village, armored, hostile if attacked
- Scattered objects: overturned cart, broken barrels, items on ground
- Maren: standing near village entrance, delivers final dialogue
- Player choice: fight/sneak/talk (implement at least fight path)
- After sequence: quest flag set, NPCs removed, village stays destroyed
- **Verify:** Sequence triggers correctly, village visually destroyed, emotional impact

### Task 20 — Prolog Complete
**Goal:** End screen and crossroads
- After village destruction dialogue: player walks to crossroads
- Crossroads: path splits three ways, signs pointing to Kernwall/Flimmermoor/Hohensang
- "PROLOG COMPLETE" overlay screen
- Stats shown: time played, boars killed, gold earned, corruption peak
- "Thank you for playing the prototype" message
- Option to restart
- **Verify:** Complete playthrough from wake-up to crossroads works without crashes

---

## Sprint 5: Polish (Tasks 21-25)

### Task 21 — Audio
- Ambient forest soundscape (looping)
- Footstep sounds (varies on terrain type if possible)
- Combat sounds: swing, hit, block
- Aether sounds: pulse, crystal hum, corruption crackle
- UI sounds: inventory open/close, buy/sell, quest complete
- Music: simple ambient track for village, tension track for combat (can be generated/royalty-free)

### Task 22 — Main Menu
- Title screen: "ASHENVEIL" with atmospheric background
- New Game button
- Controls reference
- Settings: volume, graphics quality toggle (shadow resolution, particle density)

### Task 23 — Save/Load
- Auto-save at quest milestones
- Manual save from pause menu
- LocalStorage serialization of full game state
- Load from main menu

### Task 24 — Visual Polish
- Bloom post-processing (subtle, mainly Aether)
- God rays through trees (simple screen-space effect)
- Improved particle effects
- Better tree/building models if time allows
- Vignette + color grading

### Task 25 — Playtesting & Bug Fixes
- Full playthrough test
- Fix collision issues
- Balance combat (damage, health, stamina values)
- Fix quest logic edge cases
- Performance optimization (instancing, culling, draw call reduction)
- Browser compatibility check (Chrome, Firefox, Safari)
