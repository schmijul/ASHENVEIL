import { useGameStore } from "./gameStore";
import { useInventoryStore } from "./inventoryStore";
import { useQuestStore } from "./questStore";
import { useFactionStore } from "./factionStore";

export const registerStoreDebug = () => {
  if (typeof window === "undefined") {
    return null;
  }

  const debugApi = {
    gameStore: useGameStore,
    inventoryStore: useInventoryStore,
    questStore: useQuestStore,
    factionStore: useFactionStore,
    resetAllStores: () => {
      useGameStore.getState().resetGameState();
      useInventoryStore.getState().clearInventory();
      useQuestStore.getState().resetAllQuests();
      useFactionStore.getState().resetFactions();
    },
  };

  window.__ASHENVEIL__ = debugApi;
  return debugApi;
};
