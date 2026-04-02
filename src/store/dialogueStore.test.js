import { afterEach, describe, expect, it } from "vitest";
import { useDialogueStore } from "./dialogueStore";
import { useFactionStore } from "./factionStore";
import { useGameStore } from "./gameStore";
import { useInventoryStore } from "./inventoryStore";
import { useQuestStore } from "./questStore";

const resetStores = () => {
  useDialogueStore.getState().resetDialogue();
  useGameStore.getState().resetGameState();
  useInventoryStore.getState().clearInventory();
  useQuestStore.getState().resetAllQuests();
  useFactionStore.getState().resetFactions();
};

afterEach(() => {
  resetStores();
});

describe("dialogueStore", () => {
  it("opens Maren's dialogue and advances into the quest assignment node", () => {
    const dialogue = useDialogueStore.getState();

    dialogue.setFocusedNpc("maren");
    dialogue.requestInteraction();
    let nextState = useDialogueStore.getState();

    expect(nextState.activeNpcId).toBe("maren");
    expect(nextState.currentNodeId).toBe("initial_wakeup");
    expect(nextState.isOpen).toBe(true);

    dialogue.selectOption(2);
    nextState = useDialogueStore.getState();

    expect(nextState.currentNodeId).toBe("first_quest");
    expect(useQuestStore.getState().getQuestState("hunt_boars").status).toBe("active");
    expect(useInventoryStore.getState().equipment.weapon).toBe("hunting_knife");
  });

  it("records a trade request and closes when Korvin opens trading", () => {
    const dialogue = useDialogueStore.getState();

    dialogue.setFocusedNpc("korvin");
    dialogue.requestInteraction();
    dialogue.selectOption(0);

    const nextState = useDialogueStore.getState();
    expect(nextState.isOpen).toBe(false);
    expect(nextState.requestedTradeNpcId).toBe("korvin");

    dialogue.clearTradeRequest();
    expect(useDialogueStore.getState().requestedTradeNpcId).toBeNull();
  });
});
