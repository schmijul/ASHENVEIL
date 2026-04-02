# ASHENVEIL — Tutorial / Prolog Design

## Setting: Village Grauweiler

A small hunter/farmer village at the edge of a dense forest. ~15 buildings. Surrounded by fields on one side, thick forest on the other, faint Aether crystal formations visible on distant hills.

## Pre-Prolog

Player wakes up on a bed inside a small house. The village elder (Maren) explains: "We found you in the Aether storm three days ago. You were barely alive. You can stay — but you earn your keep. We need hunters."

**Player learns:** Basic movement (WASD), camera control (mouse), interact (E).

## Phase 1 — The Hunt (5-10 min)

### Objective
Maren gives you a simple hunting knife and sends you into the forest. "Bring back 3 boar pelts. Sell the meat to Korvin at the market."

### Gameplay Taught
- **Movement + Camera** — navigate forest paths
- **Combat basics** — fight boars (easy enemies, 2-3 hits to kill, slow attacks)
- **Looting** — pick up drops (meat, pelts) from killed animals
- **Environmental awareness** — forest layout, landmarks

### Level Design Notes
- Forest is semi-linear for this phase: clear path from village to hunting grounds
- 5-6 boars spread across a small area
- One boar is slightly tougher (scarred boar) — optional mini-challenge
- A deer runs away if you approach — teaches that not everything fights back
- Small stream serves as natural boundary

### Enemies
| Enemy | HP | Damage | Behavior |
|-------|-----|--------|----------|
| Boar | 30 | 5 per hit | Charges when aggro'd, 1-2 second wind-up |
| Scarred Boar | 50 | 8 per hit | Faster charge, can chain 2 attacks |

## Phase 2 — The Village (5-10 min)

### Objective
Return to village. Sell meat to Korvin the trader. Buy a better weapon or armor piece with the gold.

### Gameplay Taught
- **Inventory management** — open inventory, see items, equip gear
- **Trading** — buy/sell interface with Korvin
- **NPC dialogue** — talk to villagers, optional flavor dialogue
- **Gold economy** — first transaction, understanding prices

### Key NPCs in Grauweiler
| NPC | Role | Location | Interaction |
|-----|------|----------|-------------|
| Maren | Village elder | Her house | Quest giver, exposition |
| Korvin | Trader | Market stall | Buy/sell |
| Hagen | Blacksmith | Forge | Sells weapons, hints at Aether smithing |
| Lotte | Herbalist | Garden | Side quest: collect 3 herbs. Reward: health potion |
| Ren | Hunter | Forest edge | Combat tips, teaches heavy attack if asked |

### Optional Side Content
- **Lotte's Herbs:** Collect 3 specific herbs from the forest edge. Reward: 2 health potions + herbalism basics. Teaches consumable usage.
- **Hagen's Request:** Bring him a boar tusk. Reward: reinforced hunting knife (damage upgrade). Teaches crafting concept.
- **Ren's Challenge:** Spar with Ren. Practice combat in safe environment. He teaches the heavy attack move.

## Phase 3 — Aether Awakening (5 min)

### Trigger
After selling meat to Korvin (Phase 2 complete), Maren asks you to check on a noise in the forest. "Something spooked the animals last night."

### Sequence
1. Player goes deeper into forest than before (new area unlocks)
2. Finds a small clearing with an **Aether Crystal Cluster** — glowing, pulsing
3. Cutscene/scripted moment: Player approaches, hands start glowing
4. Player touches crystal — flash of light, learns **Aether Pulse** ability
5. A **Corrupted Wolf** attacks — mutated, Aether crystals growing from its body
6. Player must use Aether Pulse to stagger it, then finish with melee
7. After kill: corruption meter appears briefly (first fill, small amount)

### Gameplay Taught
- **Aether Pulse** ability (hotkey: Q)
- **Corruption meter** — visual feedback that using Aether has a cost
- **Corrupted enemies** — tougher than normal animals, Aether-infused

### Enemy
| Enemy | HP | Damage | Behavior | Special |
|-------|-----|--------|----------|---------|
| Corrupted Wolf | 80 | 12 per hit | Fast lunges, circles player | Must be staggered with Aether Pulse first, otherwise blocks halve damage taken |

## Phase 4 — The Destruction (5 min)

### Trigger
Returning to village after defeating the Corrupted Wolf.

### Sequence
1. Player exits forest — sees smoke rising from Grauweiler
2. Approaching: village is under attack
3. **Attackers:** Kernwall patrol soldiers (3-4 soldiers in the village)
4. They're searching for the Aether Crystal source (they detected the player's activation)
5. Player sees: Maren confronting the patrol leader, Korvin's stall overturned, one house burning
6. **Player choice moment:**
   - **Fight** the soldiers (hard but possible — teaches that consequences exist)
   - **Sneak** around the edge (stealth tutorial moment)
   - **Talk** to the patrol leader (dialogue option, but they're hostile)
7. Regardless of approach: the village is destroyed. Key NPCs scatter.
8. Maren's last words: "Go. Find answers. Kernwall, Flimmermoor, Hohensang — someone knows what you are."

### Aftermath
- Player stands at a crossroads sign. Three directions. Three realms.
- **PROLOG COMPLETE** screen
- Open world begins (out of prototype scope)

### Gameplay Taught
- **Stakes:** Your actions drew attention. People got hurt.
- **Faction introduction:** Kernwall is aggressive, but they had a reason.
- **Player agency:** Multiple approaches to the same situation.
- **Motivation:** Find out what you are, why the Aether responds to you.

## Prototype Map Layout

```
                    [Aether Crystal Clearing]
                            |
                    [Deep Forest]
                            |
    [Herb Garden]---[Forest Path]---[Ren's Camp]
                            |
                    [Forest Edge]
                            |
    [Lotte's House]---[Village Square/Market]---[Hagen's Forge]
                            |
                    [Maren's House]
                            |
                    [Village Entrance]
                            |
                    [Crossroads — 3 directions]
```

## Estimated Play Time
- Speedrun (main objectives only): ~15 minutes
- Full exploration (all side content): ~30 minutes
