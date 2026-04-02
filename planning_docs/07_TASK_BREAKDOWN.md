# ASHENVEIL - Task Breakdown for Native Godot Migration

## Instructions for AI Agent

You are building a native Godot 4 action RPG vertical slice for Ubuntu. Read all planning docs before starting work. Complete tasks in order. Every task must end in a runnable, reviewable state.

## Rules

- Use Godot 4 and GDScript
- Keep game state in autoload singletons or dedicated runtime resources, not in ad hoc scene-local variables
- Keep items, NPCs, quests, enemies, and model mappings in JSON or equivalent data files
- Every task must be testable in the native project
- Commit after each completed task
- Refer to `06_ART_DIRECTION.md` for visual decisions
- Refer to `03_GAME_MECHANICS.md` for gameplay logic
- Refer to `04_TUTORIAL_DESIGN.md` for quest and level flow

## Branch Strategy

- Use one integration branch for the migration
- Use sub-agent branches per subsystem where parallel work helps
- Merge only after the task-specific quality gate is met

## Sprint 1: Native Foundation

### Task 1 - Godot Project Bootstrap
Goal: a native Godot project opens on Ubuntu and boots into a placeholder scene.
- Create the Godot project structure
- Add startup scene, input map, base settings, and export-friendly folders
- Set up a simple boot path and debug logging
- Verify: project opens and runs on Ubuntu without browser dependencies

### Task 2 - Player Controller
Goal: a grounded third-person player character with movement and collision.
- Build a `CharacterBody3D`-based player
- Implement WASD movement, sprinting, gravity, slopes, and interaction
- Keep the silhouette simple at first if needed, but preserve the final camera framing
- Verify: player moves and collides correctly in a test scene

### Task 3 - Grounded Third-Person Camera
Goal: a low, over-the-shoulder camera that feels like classic dark-fantasy RPGs / Witcher.
- Implement follow lag, yaw, pitch, zoom, and camera collision
- Ensure the camera stays close enough to avoid a Pokemon-style read
- Add combat backoff behavior for spatial awareness
- Verify: camera feels grounded, smooth, and readable during movement

### Task 4 - Terrain
Goal: a playable terrain blockout with height variation and path readability.
- Build terrain generation or authored terrain for the prolog map
- Apply vertex-color-style color logic or equivalent material variation
- Ensure the terrain supports the forest path, clearings, and village approach
- Verify: player can walk through a believable natural landscape

### Task 5 - Forest
Goal: an atmospheric forest that matches the reference imagery.
- Add tree, grass, rock, bush, and log placement
- Use denser layering and clearer path composition than the browser prototype
- Keep the forest warm, readable, and not overly geometric in presentation
- Verify: the forest immediately reads as the intended art direction

## Sprint 2: World and Collision

### Task 6 - Village Grauweiler
Goal: a walkable village with strong silhouette and social space.
- Build the village layout, buildings, market square, forge, and props
- Add smoke, firelight, lanterns, and other small environmental cues
- Verify: player can walk through the village and identify the main landmarks

### Task 7 - World Collision
Goal: the player cannot walk through important world geometry.
- Add collision bodies for terrain, buildings, trees, rocks, and main props
- Tune collision so traversal feels stable and natural
- Verify: the player stays within the world space without clipping

### Task 8 - Game State
Goal: central runtime state for player, world, inventory, quests, and factions.
- Create autoload singletons for the core state domains
- Expose a stable API for gameplay systems and UI
- Verify: state updates are visible from debug output or a test scene

### Task 9 - HUD
Goal: health, stamina, corruption, and gold are visible in a native UI.
- Build the HUD as Godot Control scenes
- Bind to the runtime state layer
- Keep the UI small, unobtrusive, and readable
- Verify: bars update when state changes

### Task 10 - Combat
Goal: the player can attack, block, dodge, and take damage.
- Implement melee actions and stamina costs
- Add a simple test dummy target
- Keep the camera and player movement compatible with combat feel
- Verify: all combat actions work in a test scene

## Sprint 3: Hunt Loop

### Task 11 - Boar AI
Goal: boars can wander, aggro, chase, attack, flee, and die.
- Build boar behavior and death handling
- Add loot drops for meat and pelts
- Verify: player can find and defeat boars

### Task 12 - Inventory
Goal: picked-up items appear in a usable inventory.
- Build inventory UI, equipment slots, and weight tracking
- Load item data from the shared JSON files
- Verify: boar loot appears and gear can be equipped

### Task 13 - NPC System
Goal: NPCs can be placed, named, and interacted with.
- Add Maren, Korvin, Hagen, Lotte, and Ren as native NPCs
- Show floating labels or an equivalent readable name presentation
- Verify: the player can approach and interact with NPCs

### Task 14 - Dialogue
Goal: branching dialogue works from the JSON data.
- Build dialogue UI and branching node evaluation
- Support conditions, quest flags, inventory checks, and actions
- Verify: Maren's opening conversation works end to end

### Task 15 - Trading
Goal: the player can buy and sell items with Korvin.
- Build the trade UI and gold exchange logic
- Use item prices from the data layer
- Verify: meat can be sold and a weapon can be bought

## Sprint 4: Tutorial Completion

### Task 16 - Quest System
Goal: active quests, objectives, and completion states are tracked.
- Build quest state, objective checks, and quest log UI
- Support kill, collect, talk, and visit objectives
- Verify: "Hunt boars" tracks and completes correctly

### Task 17 - Tutorial Quest Chain
Goal: the opening play loop is fully playable.
- Wire Maren's request, Korvin's sale, Hagen's gear step, and optional side content
- Make sure quest flow unlocks in the intended order
- Verify: the Phase 1 and Phase 2 tutorial loop can be finished start to finish

### Task 18 - Aether Clearing
Goal: a visually distinct crystal zone exists beyond the forest.
- Build the crystal clearing, emissive effects, and corrupted terrain read
- Make the area feel dangerous and different from the safe forest
- Verify: the zone is immediately readable as the first Aether space

### Task 19 - Aether Pulse
Goal: the player gains the first magic ability.
- Bind Aether Pulse to Q
- Add knockback, stagger, and corruption increase
- Verify: the ability changes combat and visibly affects enemies

### Task 20 - Corrupted Wolf
Goal: a mini-boss encounter that teaches Aether use.
- Build the corrupted wolf enemy and encounter script
- Require stagger or pressure management to win cleanly
- Verify: the fight feels like a real escalation above boars

## Sprint 5: Finale and Polish

### Task 21 - Village Destruction
Goal: the story pivot from safe village to hostile world is playable.
- Build the smoke, fire, soldier presence, and destroyed village state
- Handle the sequence transition cleanly
- Verify: the event triggers and the village becomes visibly destroyed

### Task 22 - Prolog Complete
Goal: the slice ends at the crossroads with a clear completion state.
- Add the crossroads scene and completion overlay
- Show basic run stats if useful
- Verify: the playthrough can end without crashes

### Task 23 - Audio
Goal: ambient forest, village, combat, and Aether audio are present.
- Add looping ambience, combat feedback, and UI audio
- Verify: the world feels less silent and more alive

### Task 24 - Main Menu and Save Load
Goal: the player can start, resume, and save the native game.
- Build main menu, pause flow, save files, and load behavior
- Verify: native saves persist across restart

### Task 25 - Final Polish and Playtest
Goal: the vertical slice feels cohesive and shippable.
- Tune lighting, camera, combat feel, and readability
- Fix collision, quest, and UI edge cases
- Verify: one full playthrough on Ubuntu works from start to finish

## Quality Gates

- Every task must be runnable on Ubuntu before merge
- Every task must include a test or smoke check appropriate to that system
- No merge with failing checks
- No task is done until the main slice remains playable after the merge
- Native project docs and runtime assumptions must stay aligned
