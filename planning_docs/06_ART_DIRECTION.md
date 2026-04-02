# ASHENVEIL — Art Direction

## Visual Style: "Valheim Aesthetic"

Low-poly stylized realism. Not cartoony, not photorealistic. The world feels handcrafted and atmospheric despite simple geometry.

### Key Visual Principles

1. **Flat shading + subtle gradients** — No smooth shading. Hard edges on geometry give the low-poly look. Color gradients on large surfaces add depth.
2. **Muted earth tones + Aether contrast** — The natural world is browns, greens, grays. Aether elements are vivid cyan/purple — they pop against the muted palette.
3. **Atmospheric fog** — Distance fog is essential. Hides LOD transitions, adds mood, makes the world feel vast.
4. **Volumetric light shafts** — God rays through trees. Simple post-processing effect but huge atmosphere payoff.
5. **Particle systems for life** — Dust motes, fireflies, Aether sparks, campfire embers. The world breathes.

## Color Palette

### Natural World
| Element | Hex | Usage |
|---------|-----|-------|
| Forest Green | `#3B5F3B` | Trees, grass |
| Moss Green | `#5C7A4A` | Ground cover, bushes |
| Bark Brown | `#5C4033` | Tree trunks, wood |
| Stone Gray | `#7A7A7A` | Rocks, cliffs |
| Dirt Brown | `#8B6914` | Paths, ground |
| Sky Blue | `#87CEEB` | Clear sky |
| Fog Gray | `#B0B8C0` | Distance fog |

### Aether Elements
| Element | Hex | Usage |
|---------|-----|-------|
| Aether Cyan | `#00F5FF` | Crystal glow, player hands |
| Aether Purple | `#9B30FF` | Corruption, deep Aether zones |
| Aether White | `#E0FFFF` | Crystal highlights, energy pulses |
| Corruption Red | `#FF3030` | High corruption, danger indicators |

### Village
| Element | Hex | Usage |
|---------|-----|-------|
| Thatch Yellow | `#D4A843` | Roof material |
| Plaster White | `#E8DCC8` | Wall surfaces |
| Warm Wood | `#8B6841` | Beams, furniture |
| Forge Orange | `#FF6B35` | Blacksmith fire, torches |

## Lighting

### Day Cycle Colors
| Time | Sun Color | Ambient | Fog |
|------|-----------|---------|-----|
| Dawn | `#FFD4A0` | `#4A4A6A` | `#C0A8B0` |
| Midday | `#FFFFF0` | `#8090A0` | `#B0B8C0` |
| Dusk | `#FF8040` | `#3A3A5A` | `#A08090` |
| Night | `#2020A0` (moon) | `#1A1A3A` | `#202040` |

### Aether Storm Lighting
- Sky shifts to dark purple-gray
- Lightning flashes in cyan
- Fog thickens, visibility drops
- Aether crystals glow brighter

## Environment Design

### Forest
- Trees: Tall pines and oaks, low-poly trunks with billboard leaf clusters
- Ground: Uneven terrain with grass tufts (instanced), scattered rocks
- Paths: Worn dirt trails, slightly lighter color than surroundings
- Details: Fallen logs, mushrooms, small flowers, moss patches
- Density: Medium — player can see ~20m ahead, feels enclosed but not claustrophobic

### Village Grauweiler
- Style: Germanic medieval hamlet, timber-frame houses
- Buildings: Simple box geometry with sloped roofs, wooden beams on exterior
- Market square: Central area, Korvin's stall with hanging meat/pelts
- Forge: Open structure, anvil, glowing coals (particle emitter)
- Details: Hay bales, wooden fences, hanging lanterns, smoke from chimneys
- Feel: Cozy and small — you know everyone here

### Aether Crystal Clearing
- Contrast: Dark forest opens to bright clearing
- Crystals: Jagged geometric shapes (octahedrons, elongated prisms)
- Glow: Emissive material + point lights, pulsing intensity
- Ground: Corrupted — grass fading to gray/purple near crystals
- Sound design critical here: crystalline hum, wind drops to silence
- Particles: Floating Aether motes, slow orbiting lights

### Destroyed Village (Phase 4)
- Same buildings but: collapsed roofs, broken walls, fire particle effects
- Smoke rising (billboard particles)
- Scattered items on ground (overturned cart, broken barrels)
- NPCs gone or cowering
- Lighting shift: overcast, desaturated, orange fire glow

## Character Design

### Player Character
- Gender-neutral silhouette (or simple selection)
- Starting outfit: Simple leather tunic, cloth pants, boots
- Distinctive feature: Faintly glowing hands (emissive material on hand mesh)
- Silhouette should read clearly at all zoom levels
- Animation priority: walk, run, idle, light attack, heavy attack, dodge, block, interact

### NPCs
- Recognizable by silhouette and color coding
- Maren: Gray hair, long robe, carries a staff (elder archetype)
- Korvin: Stocky, apron, animated hand gestures (merchant archetype)
- Hagen: Broad shoulders, leather apron, near forge (blacksmith archetype)
- Lotte: Thin, carries herb basket, flower in hair (herbalist archetype)
- Ren: Lean, bow on back, hood (ranger archetype)

### Enemies
- **Boar:** Stocky, low, tusks clearly visible. Brown/dark brown.
- **Scarred Boar:** Same but with lighter scar mark across face, slightly larger.
- **Corrupted Wolf:** Wolf base shape BUT with cyan Aether crystals growing from spine and shoulder. Eyes glow cyan. Slightly larger than normal. Movements are jerky/unnatural.
- **Kernwall Soldiers:** Heavy plate armor (angular, low-poly), red tabard with Kernwall symbol. Swords + shields. Intimidating silhouettes.

## UI Design

### Style: Minimal, Diegetic Where Possible
- HUD elements are small and unobtrusive
- Health bar: Bottom left, thin red bar
- Stamina bar: Below health, thin green bar (fades when full)
- Corruption: Only appears when > 0%. Purple bar below stamina. Pulses at high levels.
- Crosshair: None in exploration. Small dot in combat.
- Quest indicator: Small icon above relevant NPCs, not a waypoint marker on screen

### Inventory Screen
- Grid-based, dark parchment background
- Item icons: Simple, clear silhouettes
- Equipment slots: Paper-doll style character view on left
- Stats on right side
- Drag and drop to equip

### Dialogue Box
- Bottom of screen, semi-transparent dark panel
- NPC portrait/name on left
- Dialogue text center
- Response options listed below
- No voice acting (text only for prototype)

### Trade Screen
- Split view: Your inventory left, merchant inventory right
- Gold counter in center
- Price displayed on hover
- Buy/sell buttons

## Post-Processing (Prototype)

Keep it simple but impactful:
1. **Distance fog** — essential, always on
2. **Bloom** — subtle, mainly for Aether glow
3. **Color grading** — slight desaturation for natural world, boost for Aether
4. **Vignette** — subtle darkening at screen edges
5. **Corruption effect** — chromatic aberration + purple tint, intensity scales with corruption meter
