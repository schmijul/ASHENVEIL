import { describe, expect, it } from "vitest";
import { buildVillageLayout } from "./villageLayout";

describe("villageLayout", () => {
  it("builds the expected village anchors and keeps the market open", () => {
    const layout = buildVillageLayout({ seed: 7 });
    const byId = Object.fromEntries(layout.buildings.map((building) => [building.id, building]));

    expect(layout.buildings.length).toBeGreaterThanOrEqual(10);
    expect(layout.marketSquare.width).toBeGreaterThan(8);
    expect(layout.marketSquare.depth).toBeGreaterThan(6);
    expect(byId.maren_house.position[2]).toBeLessThan(-10);
    expect(byId.korvin_stall.position[0]).toBeCloseTo(0);
    expect(Math.abs(byId.korvin_stall.position[2])).toBeLessThan(4);
    expect(byId.hagen_forge.position[0]).toBeGreaterThan(10);
    expect(byId.lotte_house.position[0]).toBeLessThan(-10);

    for (const building of layout.buildings) {
      if (building.id === "korvin_stall") {
        continue;
      }

      const [x, , z] = building.position;
      expect(Math.abs(x) < 5 && Math.abs(z) < 4).toBe(false);
    }
  });

  it("includes props for the village atmosphere", () => {
    const layout = buildVillageLayout({ seed: 7 });

    expect(layout.fences.length).toBeGreaterThan(4);
    expect(layout.barrels.length).toBeGreaterThan(4);
    expect(layout.lanterns.length).toBeGreaterThan(3);
    expect(layout.smoke.length).toBeGreaterThan(3);
  });
});
