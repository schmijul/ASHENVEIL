# Ashenveil — 3D Action RPG Demo

An open-world action RPG demo built in Unity 6.3 LTS (URP).

## What is this?
A playable 30-45 minute demo set in the village of Grauweiler: hunt wildlife, trade with NPCs, discover Aether magic, and survive when the village is destroyed.

## Current Progress
- Core gameplay foundation is in place: player controller, terrain/forest, village, combat, inventory, NPC/dialog foundations.
- The main scene is `Assets/Scenes/Grauwald.unity`.
- Asset Store content is used locally under `Assets/`, but it is not downloaded or versioned from this repo. The repo only documents the packages and their authors so the project can be reconstructed on a fresh machine.
- The local `docs/BUILD_LOG.md` tracks the system-by-system build state during development.

## Tech Stack
- **Engine:** Unity 6.3 LTS (URP)
- **Art:** PBR assets from Unity Asset Store + free packs

## Asset Notes
The Unity Asset Store packages in this project are installed locally in the `Assets/` tree. They are referenced in the documentation, but they are not intended to be re-downloaded or redistributed through git.

## Getting Started

### 1. Clone & Setup Unity
```bash
git clone https://github.com/YOUR_USERNAME/Ashenveil.git
cd Ashenveil
```
Open in Unity Hub → Add project → Select folder → Open with Unity 6.3 LTS (URP).

## Project Structure
```
docs/
├── GDD.md                   # Game Design Document
└── BUILD_LOG.md             # Build progress tracker
Assets/
├── Scripts/                 # All C# game code
├── ScriptableObjects/       # Data definitions
├── Scenes/                  # Unity scenes
├── Tests/                   # EditMode + PlayMode tests
└── ...                      # Assets, Materials, VFX, Audio
```

## Build Order
The project is developed system by system on feature branches and integrated into `main` after validation.

## License
This project is a personal experiment. Assets from the Unity Asset Store are subject to their respective licenses.
