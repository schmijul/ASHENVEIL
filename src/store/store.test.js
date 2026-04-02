import { afterEach, describe, expect, it } from "vitest";
import { useFactionStore } from "./factionStore";
import { useGameStore } from "./gameStore";
import { useInventoryStore } from "./inventoryStore";
import { useQuestStore } from "./questStore";

const resetStores = () => {
  useGameStore.getState().resetGameState();
  useInventoryStore.getState().clearInventory();
  useQuestStore.getState().resetAllQuests();
  useFactionStore.getState().resetFactions();
};

afterEach(() => {
  resetStores();
});

describe("gameStore", () => {
  it("clamps health, stamina, and corruption values", () => {
    const gameStore = useGameStore.getState();

    gameStore.setMaxHealth(80);
    gameStore.setHealth(120);
    gameStore.modifyStamina(-150);
    gameStore.modifyCorruption(140);

    const nextState = useGameStore.getState();

    expect(nextState.player.maxHealth).toBe(80);
    expect(nextState.player.health).toBe(80);
    expect(nextState.player.stamina).toBe(0);
    expect(nextState.player.corruption).toBe(100);
  });

  it("tracks quest flags and gold safely", () => {
    const gameStore = useGameStore.getState();

    gameStore.setQuestFlag("sold_first_meat");
    gameStore.modifyGold(12);
    gameStore.modifyGold(-20);

    const nextState = useGameStore.getState();

    expect(nextState.world.questFlags.sold_first_meat).toBe(true);
    expect(nextState.player.gold).toBe(0);
  });
});

describe("inventoryStore", () => {
  it("adds, removes, equips, and recalculates carry weight", () => {
    const inventoryStore = useInventoryStore.getState();

    inventoryStore.addItem("boar_meat", 2);
    inventoryStore.addItem("hunting_knife", 1);
    inventoryStore.equipItem("weapon", "hunting_knife");
    inventoryStore.removeItem("boar_meat", 1);

    const nextState = useInventoryStore.getState();

    expect(nextState.inventory).toEqual([{ itemId: "boar_meat", quantity: 1 }]);
    expect(nextState.equipment.weapon).toBe("hunting_knife");
    expect(nextState.totalWeight).toBeCloseTo(3.5);
    expect(nextState.canCarryItem("iron_sword", 10)).toBe(true);
  });
});

describe("questStore", () => {
  it("starts quests and completes tracked objectives from events", () => {
    const questStore = useQuestStore.getState();

    questStore.startQuest("hunt_boars");
    questStore.recordObjectiveEvent({
      type: "kill",
      target: "boar",
      count: 3,
    });
    questStore.recordObjectiveEvent({
      type: "collect",
      item: "boar_pelt",
      count: 3,
    });

    const questState = useQuestStore.getState().quests.hunt_boars;

    expect(questState.status).toBe("completed");
    expect(useQuestStore.getState().completedQuestIds).toContain("hunt_boars");
  });
});

describe("factionStore", () => {
  it("clamps faction reputation to the supported range", () => {
    const factionStore = useFactionStore.getState();

    factionStore.adjustReputation("kernwall", 120);
    factionStore.adjustReputation("flimmermoor", -150);

    const nextState = useFactionStore.getState();

    expect(nextState.factions.kernwall).toBe(100);
    expect(nextState.factions.flimmermoor).toBe(-100);
  });
});
