import { describe, expect, it } from "vitest";
import { buildVillageLayout } from "./villageLayout";
import {
  BOAR_SPAWNS,
  createBoarTarget,
  resolveBoarState,
  rollEnemyLoot,
} from "./boarAI";

describe("boarAI", () => {
  it("defines the required six boar hunting targets", () => {
    expect(BOAR_SPAWNS).toHaveLength(6);
    expect(BOAR_SPAWNS.some((spawn) => spawn.enemyId === "scarred_boar")).toBe(
      true,
    );
  });

  it("switches from idle to chase when the player enters aggro range", () => {
    const boar = createBoarTarget(BOAR_SPAWNS[0]);
    const update = resolveBoarState({
      boar,
      playerPosition: [boar.position[0], boar.position[1], boar.position[2] + 5],
      now: 2,
      delta: 0.016,
    });

    expect(update.aiState).toMatch(/alert|chase/);
    expect(update.position).toBeDefined();
  });

  it("flees when a standard boar is critically wounded", () => {
    const boar = {
      ...createBoarTarget(BOAR_SPAWNS[0]),
      health: 5,
    };

    const update = resolveBoarState({
      boar,
      playerPosition: [boar.position[0], boar.position[1], boar.position[2] + 2],
      now: 4,
      delta: 0.016,
    });

    expect(update.aiState).toBe("flee");
  });

  it("produces deterministic loot drops", () => {
    const boar = createBoarTarget(BOAR_SPAWNS[0]);
    const first = rollEnemyLoot(boar);
    const second = rollEnemyLoot(boar);

    expect(first).toEqual(second);
    expect(first.some((drop) => drop.itemId === "boar_meat")).toBe(true);
  });

  it("keeps boar spawns clear of the village footprint", () => {
    const layout = buildVillageLayout({ seed: 7 });

    for (const spawn of BOAR_SPAWNS) {
      for (const building of layout.buildings) {
        const dx = spawn.x - building.position[0];
        const dz = spawn.z - building.position[2];
        const safeRadius =
          Math.max(building.width, building.depth) * 0.8 + 2.5;

        expect(Math.hypot(dx, dz)).toBeGreaterThan(safeRadius);
      }
    }
  });
});
