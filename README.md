# ASHENVEIL

Browser-based 3D action RPG prototype built with React, React Three Fiber, Rapier, and Zustand.

## Current Status

- Task 1 complete: foundation scene with sky, lighting, fog, and ground plane
- Task 6 complete: core Zustand stores for game, inventory, quests, and factions
- Task 2 complete: player capsule movement, sprinting, orbit camera, and camera collision fallback
- Task 3 complete: procedural terrain, forest placement, grass, rocks, and path-aware foliage rules
- Task 4 complete: village layout, buildings, market square, forge, props, and smoke effects
- Unit tests cover the current store layer
- Unit tests also cover player movement math and camera math
- Unit tests also cover terrain generation parameters
- Unit tests also cover village layout placement

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
    world/
  data/
  store/
planning_docs/
```

## Branch Strategy

- `feat/foundation`: world bootstrap, lighting, sky, fog
- `feat/game-state`: Zustand stores and state contracts
- Remaining feature branches follow the roadmap in the local planning docs

## Notes

- Runtime game data lives in `src/data/`.
- Detailed planning and narrative documents remain local in `planning_docs/` and should not be published to GitHub in online tracking branches.
