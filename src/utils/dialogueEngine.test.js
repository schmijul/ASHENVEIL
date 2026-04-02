import { afterEach, describe, expect, it } from "vitest";
import { useFactionStore } from "../store/factionStore";
import { useGameStore } from "../store/gameStore";
import { useInventoryStore } from "../store/inventoryStore";
import { useQuestStore } from "../store/questStore";
import {
  advanceDialogueFlow,
  evaluateDialogueConditions,
  getStartNodeId,
  openDialogueFlow,
} from "./dialogueEngine";

const resetStores = () => {
  useGameStore.getState().resetGameState();
  useInventoryStore.getState().clearInventory();
  useQuestStore.getState().resetAllQuests();
  useFactionStore.getState().resetFactions();
};

const getStores = () => ({
  game: useGameStore.getState(),
  inventory: useInventoryStore.getState(),
  quest: useQuestStore.getState(),
  faction: useFactionStore.getState(),
});

afterEach(() => {
  resetStores();
});

describe("dialogueEngine", () => {
  it("evaluates quest, inventory, and faction conditions", () => {
    const stores = getStores();
    stores.game.setQuestFlag("sold_first_meat", true);
    stores.inventory.addItem("boar_tusk", 1);
    stores.faction.adjustReputation("kernwall", 15);

    expect(
      evaluateDialogueConditions(
        {
          questFlag: "sold_first_meat",
          hasItem: "boar_tusk",
          factionRep: { kernwall: 10 },
        },
        getStores(),
      ),
    ).toBe(true);
  });

  it("prefers the highest-priority start node that matches current world state", () => {
    useGameStore.getState().setQuestFlag("sold_first_meat", true);

    expect(getStartNodeId("korvin", getStores())).toBe("after_first_sale");
    expect(getStartNodeId("maren", getStores())).toBe("initial_wakeup");
  });

  it("starts Maren's hunt quest and grants the hunting knife on node entry", () => {
    const stores = getStores();
    const opening = openDialogueFlow({ npcId: "maren", stores });
    const advanced = advanceDialogueFlow({
      npcId: "maren",
      nodeId: opening.currentNodeId,
      optionIndex: 2,
      stores: getStores(),
      visitedNodeKeys: opening.visitedNodeKeys,
    });

    const quest = useQuestStore.getState().getQuestState("hunt_boars");
    const inventory = useInventoryStore.getState();

    expect(advanced.currentNodeId).toBe("first_quest");
    expect(quest.status).toBe("active");
    expect(inventory.equipment.weapon).toBe("hunting_knife");
  });

  it("returns a trade request when the selected option opens merchant trading", () => {
    const opening = openDialogueFlow({ npcId: "korvin", stores: getStores() });
    const advanced = advanceDialogueFlow({
      npcId: "korvin",
      nodeId: opening.currentNodeId,
      optionIndex: 0,
      stores: getStores(),
      visitedNodeKeys: opening.visitedNodeKeys,
    });

    expect(advanced.closeDialogue).toBe(true);
    expect(advanced.requestTradeNpcId).toBe("korvin");
  });
});
