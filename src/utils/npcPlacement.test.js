import { describe, expect, it } from "vitest";
import { buildNpcPlacements, npcPlacementDefaults } from "./npcPlacement";
import { buildVillageLayout } from "./villageLayout";

describe("npcPlacement", () => {
  it("creates one grounded placement for each village NPC", () => {
    const placements = buildNpcPlacements({ seed: 7 });

    expect(placements).toHaveLength(5);
    expect(new Set(placements.map((npc) => npc.id)).size).toBe(5);
    placements.forEach((npc) => {
      expect(npc.position[1]).toBeGreaterThan(-0.5);
      expect(Number.isFinite(npc.defaultYaw)).toBe(true);
    });
  });

  it("keeps NPCs near their assigned landmarks instead of overlapping the village core", () => {
    const placements = buildNpcPlacements({ seed: 7 });
    const villageLayout = buildVillageLayout({ seed: 7 });
    const buildings = Object.fromEntries(
      villageLayout.buildings.map((building) => [building.id, building]),
    );

    placements.forEach((npc) => {
      const anchorId = npcPlacementDefaults.placementRules[npc.id].anchorId;
      const anchor = buildings[anchorId];
      const distanceToAnchor = Math.hypot(
        npc.position[0] - anchor.position[0],
        npc.position[2] - anchor.position[2],
      );

      expect(distanceToAnchor).toBeGreaterThan(1.5);
      expect(distanceToAnchor).toBeLessThan(4.1);
    });
  });
});
