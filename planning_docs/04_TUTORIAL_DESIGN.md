# ASHENVEIL - Tutorial / Prolog Design

The tutorial is the first native Godot vertical slice. It must teach the camera, movement, combat, interaction, inventory, trading, the first Aether ability, and the story stakes without feeling like a system checklist.

## Setting: Village Grauweiler

Grauweiler is a small hunter/farmer village at the edge of a dense forest. It has about 15 buildings, open fields on one side, and thick forest with distant Aether crystal formations on the other.

## Pre-Prolog

The player wakes up in a bed inside a small house. Maren explains: "We found you in the Aether storm three days ago. You were barely alive. You can stay, but you earn your keep. We need hunters."

The player learns:
- Movement: WASD
- Camera: mouse
- Interact: E

## Phase 1 - The Hunt

### Objective
Maren gives the player a hunting knife and sends them into the forest. "Bring back 3 boar pelts. Sell the meat to Korvin at the market."

### Gameplay Taught
- Movement and camera control in a grounded third-person view
- Combat basics against boars
- Loot pickup from killed animals
- Reading the forest path and landmarks

### Level Design Notes
- The forest is semi-linear for this phase
- Five to six boars are spread through a compact hunting area
- One scarred boar is a slightly tougher optional challenge
- A deer runs away if approached to show that not everything is hostile
- A stream acts as a soft boundary

### Enemies
| Enemy | HP | Damage | Behavior |
|-------|-----|--------|----------|
| Boar | 30 | 5 per hit | Charges when aggro'd, 1-2 second wind-up |
| Scarred Boar | 50 | 8 per hit | Faster charge, can chain two attacks |

## Phase 2 - The Village

### Objective
Return to village. Sell meat to Korvin. Buy a better weapon or armor piece with the gold.

### Gameplay Taught
- Inventory management
- Trading with Korvin
- NPC dialogue and flavor interactions
- Gold economy and item pricing

### Key NPCs
| NPC | Role | Location | Interaction |
|-----|------|----------|-------------|
| Maren | Village elder | Her house | Quest giver, exposition |
| Korvin | Trader | Market stall | Buy/sell |
| Hagen | Blacksmith | Forge | Sells weapons, hints at Aether smithing |
| Lotte | Herbalist | Garden | Optional herb quest, reward potions |
| Ren | Hunter | Forest edge | Combat tips, teaches heavy attack |

### Optional Side Content
- Lotte's Herbs: collect three herbs, reward potions and healing basics
- Hagen's Request: bring a boar tusk, reward a reinforced hunting knife
- Ren's Challenge: spar to learn the heavy attack

## Phase 3 - Aether Awakening

### Trigger
After selling meat to Korvin, Maren asks the player to check on a noise in the forest.

### Sequence
1. The player goes deeper into the forest than before
2. They find an Aether crystal clearing, glowing and pulsing
3. The player approaches and their hands start glowing
4. They touch the crystal and learn Aether Pulse
5. A corrupted wolf attacks
6. The player must stagger it with Aether Pulse and finish it with melee
7. Corruption appears as a visible cost

### Gameplay Taught
- Aether Pulse on Q
- Corruption meter and its cost
- Corrupted enemies as a new threat tier

### Enemy
| Enemy | HP | Damage | Behavior | Special |
|-------|-----|--------|----------|---------|
| Corrupted Wolf | 80 | 12 per hit | Fast lunges, circles player | Must be staggered with Aether Pulse first |

## Phase 4 - The Destruction

### Trigger
Return to village after defeating the corrupted wolf.

### Sequence
1. The player exits the forest and sees smoke rising from Grauweiler
2. Approaching the village reveals a Kernwall patrol
3. The patrol is searching for the Aether source they detected
4. Maren confronts the leader, Korvin's stall is overturned, and one house burns
5. The player can fight, sneak, or talk, but the village falls regardless
6. Maren tells the player to leave and find answers

### Aftermath
- The player reaches a crossroads with three directions
- PROLOG COMPLETE appears
- The open world begins after the vertical slice

### Gameplay Taught
- Stakes and consequence
- Faction introduction
- Player agency inside a scripted event
- Motivation to continue the main story

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
                    [Crossroads - 3 directions]
```

## Estimated Play Time
- Speedrun of main objectives: about 15 minutes
- Full exploration including side content: about 30 minutes
