# ASHENVEIL

Browser-based 3D action RPG prototype built with React, React Three Fiber, Rapier, and Zustand.

## Current Status

- Task 1 complete: foundation scene with sky, lighting, fog, and ground plane
- Task 6 complete: core Zustand stores for game, inventory, quests, and factions
- Task 2 complete: player capsule movement, sprinting, orbit camera, and camera collision fallback
- Task 3 complete: procedural terrain, forest placement, grass, rocks, and path-aware foliage rules
- Task 4 complete: village layout, buildings, market square, forge, props, and smoke effects
- Task 5 complete: terrain, tree, rock, and building collision bodies integrated for player traversal
- Task 8 complete: melee combat actions, stamina costs, dodge i-frames, and a training dummy target
- Task 9 complete: boar AI, forest spawns, loot drops, and hunt progression hooks
- Task 11 complete: NPC placements, proximity prompts, and village interaction targets
- Task 12 complete: JSON-driven dialogue, node conditions, quest/item side effects, and Maren's opening flow
- Task 10 complete: inventory overlay, equipment slots, item-driven gear handling, and overweight movement slowdown
- Task 13 complete: Korvin trading, merchant stock state, gold exchange, and quest-linked sell/buy events
- Unit tests cover the store layer, dialogue engine, trade flow, player movement math, camera math, terrain generation, village layout placement, NPC placement, and collision profiles

## Stack

- React 18
- Vite 5
- Three.js via React Three Fiber
- Rapier physics
- Zustand state management
- Vitest for unit tests

## Commands

```bash
npm install
npm run dev
npm run build
npm run test
```

## Project Layout

```text
src/
  components/
    entities/
    systems/
    ui/
    world/
  data/
  store/
  utils/
planning_docs/   # local only, not for sanitized GitHub branches
```

## Branch Strategy

- `feat/foundation`: world bootstrap, lighting, sky, fog
- `feat/game-state`: Zustand stores and state contracts
- Remaining feature branches follow the roadmap in the local planning docs

## Notes

- Runtime game data lives in `src/data/`.
- Browser-accessible debug handles are exposed through `window.__ASHENVEIL__`.
- Dialogue, inventory, and trading now run fully through Zustand-backed runtime stores.
- Detailed planning and narrative documents remain local in `planning_docs/` and should not be published to GitHub in online tracking branches.
