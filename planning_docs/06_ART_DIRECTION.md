# ASHENVEIL - Art Direction

## Visual Style: Stylized-Realistic Forest Atmosphere

Ashenveil should feel like a grounded dark-fantasy / Witcher-inspired RPG rather than a prototype with visible geometric shortcuts. The look is atmospheric, warm, and believable. Strong lighting, fog, composition, and material treatment carry the scene more than brute-force polygon density.

## Key Visual Principles

1. Smooth shading and organic forms. Terrain, trunks, buildings, and props should read as physical objects, not toy primitives.
2. Warm natural earth tones. The prolog forest is dominated by greens, browns, moss, and sunlight rather than saturated fantasy colors.
3. Atmospheric fog. Distance fog should soften the world and create depth.
4. Volumetric god rays. Light shafts through canopy are a signature part of the forest identity.
5. Rich post-processing. Bloom, tone mapping, vignette, and subtle grading should create cohesion.
6. Dense layered vegetation. Tall trees, bushes, grass, ground cover, logs, mushrooms, and rocks should make the world feel lived in.
7. Small life details. Dust motes, fireflies, embers, and Aether sparks help the world breathe.

## What This Is Not

- Not cheap low-poly with flat-shaded primitives
- Not cartoonish fantasy
- Not photorealistic AAA rendering
- Not generic asset-pack presentation
- Not a shiny effect-heavy forest where the mood disappears under noise

## Reference Images

The files in `design_inspiration/` are the target look. They show the balance to aim for:
- `forest.png`: warm golden light, clear paths, dense readable forest, layered depth
- `village.png.png`: medieval Germanic village, timber-frame architecture, soft morning light, believable scale

## Color Palette

### Natural World
| Element | Hex | Usage |
|---------|-----|-------|
| Deep Forest Green | `#3D6438` | Pine canopies, shadowed foliage |
| Canopy Green | `#4A7040` | Main foliage color |
| Moss Green | `#6B8450` | Ground cover, highlights |
| Grass Green | `#5D7548` | Grass, forest floor |
| Grass Tip | `#7A9A50` | Blade tips, warm highlights |
| Bark Brown | `#5C4033` | Trunks, timber, wood props |
| Warm Bark | `#6B4D38` | Wood highlights |
| Stone Gray | `#5A5A58` | Rock base |
| Moss Stone | `#4A5A40` | Mossy rock variation |
| Earth Brown | `#7A6035` | Paths, dirt, worn ground |
| Forest Floor | `#4A6340` | Base forest terrain |
| Morning Gold | `#FFD4A0` | Sunlight, god rays |
| Fog Warm | `#C8BFA8` | Warm distance fog |
| Sky Morning | `#B8C4D4` | Soft blue sky bounce |

### Aether Elements
| Element | Hex | Usage |
|---------|-----|-------|
| Aether Cyan | `#00F5FF` | Crystals, pulse ability |
| Aether Purple | `#9B30FF` | Corruption, hostile zones |
| Aether White | `#E0FFFF` | Highlights and energy pulses |
| Corruption Red | `#FF3030` | High corruption warnings |

The peaceful starting forest should use Aether colors sparingly. The alien palette becomes dominant only once the player reaches the crystal clearing.

### Village Palette
| Element | Hex | Usage |
|---------|-----|-------|
| Thatch Yellow | `#D4A843` | Roofs |
| Plaster White | `#E8DCC8` | Walls |
| Warm Wood | `#8B6841` | Beams, furniture |
| Forge Orange | `#FF6B35` | Fire, torches, forge glow |

## Lighting

### Prolog Morning

The tutorial uses a fixed warm morning setup.

| Element | Setting |
|---------|---------|
| Hemisphere Light | Sky `#C4B898` / Ground `#3A4A30`, intensity 0.5 |
| Directional Sun | Color `#FFD4A0`, intensity 1.2, position `[35, 12, 15]` |
| Fill Light | Color `#6080A0`, intensity 0.18 |
| Shadow Map | 4096x4096, tuned for soft but readable shadows |

### Lighting Goals
- Long shadows from a low morning sun
- Warm light on sun-facing surfaces
- Cool fill in shadow areas
- Strong contrast between lit clearings and shaded forest
- Clear god ray silhouettes wherever the canopy opens

## Environment Design

### Forest

The forest is the first playable area and must immediately communicate the game identity.

**Geometry**
- Tall pines and oaks with organic silhouette variation
- Terrain with smooth rolling hills and clear path readability
- Grass, bushes, logs, and rocks layered densely enough to feel alive
- Buildings and props should have believable proportions rather than toy-like scale

**Atmosphere**
- Warm fog with visible depth
- Clear worn path as the main navigation line
- Lighting should guide the player without obvious UI arrows
- Panoramic openings should reward exploration and orientation

### Village Grauweiler
- Medieval Germanic hamlet with timber-frame houses
- Market square as the social center
- Forge with firelight and bloom
- Small, cozy, readable, and lived in
- Same morning lighting language as the forest

### Aether Crystal Clearing
- Strong contrast against the peaceful forest
- Jagged crystals and alien light
- Ground corruption that shifts the palette toward gray and purple
- Emissive glow plus subtle particle motion
- The first place where the Aether colors dominate

### Destroyed Village
- Collapsed roofs and broken walls
- Smoke, fire, and orange glow
- Lower saturation and harsher contrast
- A visible emotional break from the safe village look

## Character Design

### Player Character
- Clear, grounded silhouette
- Leather tunic, cloth pants, boots
- Faintly glowing hands
- Reads well from close over-the-shoulder camera distance

### NPCs
- Maren: gray hair, long robe, staff
- Korvin: stocky, apron, animated merchant posture
- Hagen: broad shoulders, leather apron
- Lotte: thin, herb basket, flower in hair
- Ren: lean, bow on back, hood

### Enemies
- Boar: stocky, low, tusks clearly visible
- Scarred Boar: same silhouette, lighter scar mark
- Corrupted Wolf: wolf silhouette with cyan crystal growths and glowing eyes
- Kernwall Soldiers: angular armor, red tabard, intimidating shape

## UI Design

### General UI Style
- Minimal and diegetic where possible
- Small, unobtrusive HUD
- No crosshair in exploration
- Quest indicators should stay subtle

### Inventory Screen
- Dark parchment feel
- Clear item silhouettes
- Equipment slots with readable layout

### Dialogue Box
- Semi-transparent dark panel
- NPC name and portrait area
- Clean, readable response options

### Trade Screen
- Split view with player and merchant inventories
- Prices visible and easy to compare

## Post-Processing Pipeline

The native build should use Godot's post-processing and camera stack to approximate the same filmic look:

| Effect | Purpose |
|--------|---------|
| Tone mapping | Warm, filmic highlights |
| Bloom | Fire, lanterns, Aether glow |
| Fog | Depth and mood |
| Vignette | Framing and focus |
| Subtle color grading | Natural forest warmth |

## Performance Targets

| Element | Budget |
|---------|--------|
| Visible complexity | Dense enough for atmosphere, stable enough for 60 FPS on mid-range Ubuntu hardware |
| Draw overhead | Keep repeated props and vegetation efficient |
| Lighting | Prefer composition and fog over brute-force light spam |

### Polygon Budget Guidance
| Asset | Target |
|-------|--------|
| Player character | 2,000-3,000 tris |
| NPCs | 1,500-2,500 tris |
| Buildings | 500-1,500 tris |
| Pine tree | about 120 tris when instanced |
| Oak tree | about 180 tris when instanced |
| Grass cluster | about 12 tris when instanced |
| Bush / fern | about 8 tris when instanced |
| Rock | about 180 tris when instanced |
| Terrain | 32,768 tris for the main render mesh |
| Animals / enemies | 500-1,500 tris |
