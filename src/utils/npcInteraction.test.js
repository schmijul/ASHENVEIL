import { describe, expect, it } from "vitest";
import { buildNpcPlacements } from "./npcPlacement";
import { findNearestNpc, getFacingYaw, getNpcPreviewText } from "./npcInteraction";

describe("npcInteraction", () => {
  it("finds the nearest NPC inside interaction range", () => {
    const placements = buildNpcPlacements({ seed: 7 });
    const korvin = placements.find((npc) => npc.id === "korvin");

    const nearest = findNearestNpc(
      [korvin.position[0] + 0.4, korvin.position[1], korvin.position[2] + 0.2],
      placements,
    );

    expect(nearest?.id).toBe("korvin");
  });

  it("returns no NPC when the player is outside interaction range", () => {
    const placements = buildNpcPlacements({ seed: 7 });

    const nearest = findNearestNpc([40, 0, 40], placements);

    expect(nearest).toBeNull();
  });

  it("calculates a yaw that points an NPC back toward the player", () => {
    const yaw = getFacingYaw([1, 0, 1], [1, 0, 5]);

    expect(yaw).toBeCloseTo(0);
  });

  it("uses the first JSON node for the preview panel", () => {
    expect(getNpcPreviewText("maren")).toContain("You're awake");
  });
});
