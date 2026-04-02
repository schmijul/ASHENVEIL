import { describe, expect, it } from "vitest";
import {
  buildForestPlacements,
  buildTerrainData,
  isInsideClearing,
  isOnForestPath,
  sampleTerrainHeight,
} from "./terrainGeneration";

describe("terrainGeneration", () => {
  it("generates gently varying terrain heights in a bounded range", () => {
    const h1 = sampleTerrainHeight(0, 0, { seed: 7 });
    const h2 = sampleTerrainHeight(18, 12, { seed: 7 });
    const h3 = sampleTerrainHeight(-14, 28, { seed: 7 });

    expect(h1).toBeGreaterThan(-0.22);
    expect(h1).toBeLessThan(0.36);
    expect(h2).toBeGreaterThan(-0.22);
    expect(h3).toBeLessThan(0.36);
    expect(Math.abs(h1 - h2) + Math.abs(h2 - h3)).toBeGreaterThan(0.05);
  });

  it("builds a terrain grid with matching vertex and index counts", () => {
    const terrain = buildTerrainData({ seed: 7, segments: 8, size: 24 });

    expect(terrain.positions).toHaveLength((8 + 1) * (8 + 1) * 3);
    expect(terrain.colors).toHaveLength((8 + 1) * (8 + 1) * 3);
    expect(terrain.indices).toHaveLength(8 * 8 * 6);
  });

  it("marks the forest path and clearings consistently", () => {
    expect(isOnForestPath(0, 0)).toBe(true);
    expect(isOnForestPath(18, 0)).toBe(false);
    expect(isInsideClearing(0, -2)).toBe(true);
    expect(isInsideClearing(30, 30)).toBe(false);
  });

  it("places foliage outside the path and clearings", () => {
    const placements = buildForestPlacements({ seed: 7, size: 120 });

    expect(placements.pines.length + placements.oaks.length).toBeGreaterThan(10);
    expect(placements.grass.length).toBeGreaterThan(20);
    expect(placements.rocks.length).toBeGreaterThan(0);

    for (const placement of [
      ...placements.pines,
      ...placements.oaks,
      ...placements.grass,
      ...placements.rocks,
    ]) {
      const [x, , z] = placement.position;
      expect(isOnForestPath(x, z)).toBe(false);
      expect(isInsideClearing(x, z)).toBe(false);
    }
  });
});
