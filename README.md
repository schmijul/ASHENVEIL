# Ashenveil

Ashenveil is an in-progress 3D action RPG demo project built with Unity 6.3 LTS. The target is a 30-45 minute prologue set around Grauwald forest and the village of Grauweiler.

## Current State

- The tracked Unity project lives in `aschenveil/`.
- Open the project with Unity `6000.3.12f1`.
- The main scene is `aschenveil/Assets/Scenes/Grauwald.unity`.
- The project is currently configured to use HDRP.
- `main` already contains gameplay foundations for player movement, third-person camera, terrain/forest setup, village layout, melee combat, inventory, and supporting EditMode tests.
- Wildlife AI and NPC/dialog code are also present, but the repository is still an active prototype rather than a finished playable vertical slice.

## Getting Started

```bash
git clone https://github.com/schmijul/ASHENVEIL.git
cd ASHENVEIL
```

Then open `ASHENVEIL/aschenveil` in Unity Hub with editor version `6000.3.12f1`.

## Project Layout

```text
README.md
aschenveil/
├── Assets/
│   ├── Scenes/
│   ├── ScriptableObjects/
│   ├── Scripts/
│   ├── Settings/
│   └── Tests/
├── Packages/
└── ProjectSettings/
```

## Notes On Assets

This repository tracks the authored gameplay code, scene assets, tests, and render-pipeline configuration needed to work on the project.

Large third-party Asset Store content under `aschenveil/Assets/` is intentionally excluded from git. If you open the project on a fresh machine, expect to restore those local art/content packages separately.

## Testing

EditMode tests live under `aschenveil/Assets/Tests/EditMode/`.

Run them from Unity Test Runner or via Unity batch mode, for example:

```bash
unity -runTests -testPlatform EditMode -projectPath aschenveil
```

## License

Project code and original project files are part of a personal prototype. Third-party assets remain subject to their respective licenses.
