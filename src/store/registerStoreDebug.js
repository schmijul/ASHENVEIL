import { useGameStore } from "./gameStore";
import { useInventoryStore } from "./inventoryStore";
import { useQuestStore } from "./questStore";
import { useFactionStore } from "./factionStore";
import { useDialogueStore } from "./dialogueStore";

export const registerStoreDebug = () => {
  if (typeof window === "undefined") {
    return null;
  }

  const debugApi = {
    gameStore: useGameStore,
    inventoryStore: useInventoryStore,
    questStore: useQuestStore,
    factionStore: useFactionStore,
    dialogueStore: useDialogueStore,
    resetAllStores: () => {
      useDialogueStore.getState().resetDialogue();
      useGameStore.getState().resetGameState();
      useInventoryStore.getState().clearInventory();
      useQuestStore.getState().resetAllQuests();
      useFactionStore.getState().resetFactions();
    },
  };

  window.__ASHENVEIL__ = debugApi;
  return debugApi;
};
