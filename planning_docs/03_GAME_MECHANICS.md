# ASHENVEIL - Game Mechanics

This document defines the native Godot gameplay model for Ashenveil. The rules are the same design-wise as before, but the runtime now targets a third-person Ubuntu build in Godot 4 with GDScript, scene resources, and autoload singletons.

## Combat System

### Core Combat
- Third-person melee-focused real-time combat
- Light Attack: fast, low damage, can chain into a 3-hit combo
- Heavy Attack: slow, high damage, can stagger enemies
- Block: reduces damage, drains stamina, with timing-based perfect block windows
- Dodge Roll: i-frames, stamina cost
- No lock-on by default: free aim like classic dark-fantasy RPGs, with optional soft lock behavior if needed

### Stamina System
- All combat actions drain stamina
- Stamina regenerates faster out of combat
- Running also costs stamina
- Low stamina slows attacks and disables blocking, which punishes button mashing

### Aether Powers
- The player can channel Aether through their hands
- Aether Pulse is the first ability learned in the tutorial
- Later powers remain out of prototype scope
- Using Aether fills the corruption meter

### Corruption Mechanic
- Every Aether use fills corruption slightly
- Corruption stages:
  - 0-25%: no effect
  - 25-50%: visual distortion, minor power increase
  - 50-75%: hallucinations, stronger power, higher risk
  - 75-100%: losing control, random Aether discharges, NPC fear
  - 100%: game over, the player becomes a crystal statue
- Corruption decays slowly over time when Aether is not used
- The mechanic exists as risk/reward, not as a simple mana bar

## Progression System

### No Classes, No XP Bar
- No class selection at start
- No XP from kills
- Improvement comes from actions and trainers, not from abstract leveling
- Skills unlock through specific trainer interactions, not via a generic skill tree
- Natural specialization matters: the full game should not let one playthrough master everything

### Skill Categories
| Category | Examples | Primary Trainers |
|----------|----------|-----------------|
| Melee Combat | Sword mastery, heavy weapons, dual wield | Kernwall soldiers |
| Ranged | Bow proficiency, throwing knives | Hunters, Flimmermoor scouts |
| Aether | New powers, corruption resistance, crystal crafting | Hohensang scholars/cultists |
| Survival | Hunting efficiency, cooking, herb knowledge | Village NPCs, wanderers |
| Social | Persuasion, intimidation, barter prices | Flimmermoor merchants |
| Stealth | Lockpicking, pickpocket, silent movement | Flimmermoor thieves |

## Faction Reputation System

### Three-Axis Reputation
Each realm has an independent reputation value from -100 to +100.

| Range | Status | Effect |
|-------|--------|--------|
| -100 to -50 | Hostile | Attack on sight |
| -50 to -10 | Distrusted | Limited access, bad prices, guards watch you |
| -10 to +10 | Neutral | Default starting state |
| +10 to +50 | Friendly | Access to trainers, fair prices, side quests |
| +50 to +100 | Trusted | Inner circle quests, best trainers, faction-specific ending |

### Faction Tension
- Helping one faction often reduces reputation with others
- Some quests are mutually exclusive between factions
- The faction alliance should matter for endings and late-game access

## Inventory & Economy

### Inventory
- Weight-based inventory, no magic bag of holding
- Equipment slots: head, chest, legs, boots, gloves, weapon, shield/offhand, accessories
- Quick slots for consumables such as food and potions

### Economy
- Gold coins as currency
- Prices are affected by faction reputation
- Early-game economy loop: hunt, sell meat and pelts, buy gear
- Aether crystals are valuable but dangerous to carry because they accelerate corruption

### Crafting
- Cooking: combine ingredients at campfires for regeneration food
- Aether smithing: upgrade weapons or armor with Aether crystals at forges, with corruption risk
- No complex crafting trees. Keep it classic dark-fantasy RPG style: recipe, materials, station.

## NPC & Dialogue System

### Grounded Dialogue Style
- No voiced protagonist
- Dialogue options have no moral color coding
- Consequences are learned through play, not telegraphed by the UI
- NPCs remember your actions and can react to them later
- Some dialogue options depend on skills or faction reputation

### NPC Schedules
- NPC daily routines are planned but not required for the vertical slice

## World Navigation

### Open World Rules
- No invisible walls and no level gates in the long-term design
- Difficulty scales by geography, especially near Aether zones
- Fast travel is not part of the early game
- Day/night affects enemy spawns and NPC availability
- Aether storms temporarily raise danger

## Native Runtime Notes

- Game state lives in Godot autoload singletons, not in scene-local state
- Item, quest, NPC, and enemy data stay external in JSON and are loaded at runtime
- Combat, dialogue, inventory, and quests must stay decoupled enough to test separately
- Camera and movement tuning should favor the heavier heavy grounded feel rather than a floaty action-cam feel
