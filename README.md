# Ashenveil — 3D Action RPG Demo

An open-world action RPG demo built as a stress test for AI-assisted (agentic) game development.

## What is this?
A playable 30-45 minute demo set in the village of Grauweiler: hunt wildlife, trade with NPCs, discover Aether magic, and survive when the village is destroyed.

## Current Progress
- Core gameplay foundation is in place: player controller, terrain/forest, village, combat, inventory, NPC/dialog foundations.
- The main scene is `Assets/Scenes/Grauwald.unity`.
- Asset Store content is used locally under `Assets/`, but it is not downloaded or versioned from this repo. The repo only documents the packages and their authors so the project can be reconstructed on a fresh machine.
- The local `docs/BUILD_LOG.md` tracks the system-by-system build state during development.

## Tech Stack
- **Engine:** Unity 6.3 LTS (URP)
- **AI Development:** Codex CLI with subagent orchestration
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

### 2. Import Assets
See `docs/STARTPROMPT.md` for the full asset list and import instructions.

### 3. Start Codex Orchestrator
```bash
codex --full-auto --approval-mode on-request
```
Then paste the orchestrator prompt from `docs/STARTPROMPT.md`.

## Project Structure
```
AGENTS.md                    # Codex agent instructions
.agents/skills/              # Codex skills (orchestrator, unity-csharp)
docs/
├── GDD.md                   # Game Design Document
├── BUILD_LOG.md             # Build progress tracker
└── STARTPROMPT.md           # Codex CLI launch instructions
Assets/
├── Scripts/                 # All C# game code
├── ScriptableObjects/       # Data definitions
├── Scenes/                  # Unity scenes
├── Tests/                   # EditMode + PlayMode tests
└── ...                      # Assets, Materials, VFX, Audio
```

## Build Order
See `AGENTS.md` for the 14-system dependency chain. Each system is built on a feature branch, tested, and merged sequentially.

## License
This project is a personal experiment. Assets from the Unity Asset Store are subject to their respective licenses.
