# ASHENVEIL — Game Mechanics

## Combat System (Real-Time, Gothic 3 Reference)

### Core Combat
- **Third-person melee-focused** real-time combat
- **Light Attack** — fast, low damage, can chain 3-hit combo
- **Heavy Attack** — slow, high damage, can stagger enemies
- **Block** — reduces damage, drains stamina. Timing-based parry for perfect block.
- **Dodge Roll** — i-frames, stamina cost
- **No lock-on by default** — free aim like Gothic. Optional soft lock-on.

### Stamina System
- All combat actions drain stamina
- Stamina regenerates faster out of combat
- Running also costs stamina
- Low stamina = attacks get slower, can't block → punishes button mashing

### Aether Powers (Limited Magic)
- Player can channel Aether through hands (unique ability)
- **Aether Pulse** — short-range knockback, first ability learned in tutorial
- Additional powers found/taught throughout game (NOT in prototype scope)
- Using Aether fills a **Corruption Meter** (see below)

### Corruption Mechanic
- Every Aether use fills the corruption bar slightly
- Corruption stages:
  - 0-25%: No effect
  - 25-50%: Visual distortion (screen edges pulse), minor stat buff (power increases)
  - 50-75%: Hallucinations (fake enemies appear), significant power boost
  - 75-100%: Losing control. Random Aether discharges. NPCs become afraid.
  - 100%: Game over — you become a crystal statue (Aether consumed you)
- Corruption decays slowly over time when NOT using Aether
- Creates risk/reward: Aether is powerful but addictive and deadly

## Progression System

### No Classes, No XP Bar
- You don't pick a class at start
- You don't earn XP from kills
- **You improve by doing + finding teachers:**
  - Want better sword skills? Find a combat trainer in Kernwall, pay/quest for lessons
  - Want to handle Aether better? Risk learning from Hohensang cultists
  - Want to pick locks? Befriend a thief in Flimmermoor
- Skills unlock through specific trainer interactions, not a skill tree
- Natural specialization: you can't train everything in one playthrough (time/faction limits)

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
Each realm has an independent reputation value: -100 (Hostile) to +100 (Trusted Ally)

| Range | Status | Effect |
|-------|--------|--------|
| -100 to -50 | Hostile | Attack on sight |
| -50 to -10 | Distrusted | Limited access, bad prices, guards watch you |
| -10 to +10 | Neutral | Default starting state |
| +10 to +50 | Friendly | Access to trainers, fair prices, side quests |
| +50 to +100 | Trusted | Inner circle quests, best trainers, faction-specific ending |

### Faction Tension
- Helping one faction often reduces reputation with others (not always, but often)
- Some quests are mutually exclusive between factions
- Endgame requires at least one faction at Trusted level to attempt the Aether restart
- The faction you're allied with determines the ending

## Inventory & Economy

### Inventory
- **Weight-based** inventory (no magic bag of holding)
- Equipment slots: Head, Chest, Legs, Boots, Gloves, Weapon, Shield/Offhand, 2x Accessory
- Quick slots for consumables (food, potions)

### Economy
- **Gold coins** as currency
- Prices affected by faction reputation (better rep = better prices)
- Hunting and selling meat/pelts is the early-game economy loop
- Aether crystals are high-value but dangerous to carry (slow corruption)

### Crafting (Simple)
- **Cooking:** Combine ingredients at campfires. Food = health regeneration over time.
- **Aether Smithing:** Upgrade weapons/armor with Aether crystals at forges. Risk: corruption.
- No complex crafting trees. Keep it Gothic-style: find recipe, have materials, use station.

## NPC & Dialogue System

### Gothic-Style Dialogue
- No voiced protagonist
- NPCs have dialogue options but **no moral indicators** (no blue/red choices)
- Consequences are not telegraphed — you learn from experience
- NPCs remember your actions and adjust dialogue
- Some dialogue options only available with sufficient skill (e.g., Persuasion) or faction rep

### NPC Schedules (Stretch Goal)
- NPCs have daily routines (sleep, work, eat, patrol)
- Not required for prototype but planned for full game

## World Navigation

### Open World Rules
- No invisible walls, no level gates
- Difficulty scales by geography: areas near Aether zones are harder
- Fast travel: none initially. Unlock routes through faction reputation.
- Day/night cycle affects enemy spawns and NPC availability
- Weather: Aether storms are random events that increase danger temporarily
