# Legacy Web Prototype

This directory preserves the former browser-based Ashenveil prototype built with React Three Fiber.

It is retained for reference during the native Godot migration and is not the active runtime target.

## Legacy run commands

```bash
npm install
npm run dev
```

## Legacy data source

The native Godot scaffold currently reads gameplay data from:

- `legacy_web/src/data/items.json`
- `legacy_web/src/data/npcs.json`
- `legacy_web/src/data/quests.json`
- `legacy_web/src/data/enemies.json`
- `legacy_web/src/data/characterModels.json`
