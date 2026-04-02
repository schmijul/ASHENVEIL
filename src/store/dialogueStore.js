import { create } from "zustand";
import { useFactionStore } from "./factionStore";
import { useGameStore } from "./gameStore";
import { useInventoryStore } from "./inventoryStore";
import { useQuestStore } from "./questStore";
import { advanceDialogueFlow, openDialogueFlow } from "../utils/dialogueEngine";

const initialDialogueState = {
  focusedNpcId: null,
  activeNpcId: null,
  currentNodeId: null,
  isOpen: false,
  requestedTradeNpcId: null,
  visitedNodeKeys: [],
};

const collectStores = () => ({
  game: useGameStore.getState(),
  inventory: useInventoryStore.getState(),
  quest: useQuestStore.getState(),
  faction: useFactionStore.getState(),
});

export const useDialogueStore = create((set, get) => ({
  ...initialDialogueState,
  setFocusedNpc: (npcId) =>
    set((state) =>
      state.focusedNpcId === npcId ? state : { focusedNpcId: npcId },
    ),
  requestInteraction: () => {
    const state = get();
    if (state.isOpen || !state.focusedNpcId) {
      return state;
    }

    if (state.focusedNpcId === "maren") {
      useGameStore.getState().setQuestFlag("metMaren", true);
    }
    useGameStore.getState().resetControls();

    const opening = openDialogueFlow({
      npcId: state.focusedNpcId,
      stores: collectStores(),
    });

    set({
      focusedNpcId: state.focusedNpcId,
      activeNpcId: opening.closeDialogue ? null : state.focusedNpcId,
      currentNodeId: opening.currentNodeId,
      isOpen: !opening.closeDialogue,
      requestedTradeNpcId: opening.requestTradeNpcId,
      visitedNodeKeys: opening.visitedNodeKeys,
    });

    return get();
  },
  selectOption: (optionIndex) => {
    const state = get();
    if (!state.isOpen || !state.activeNpcId || !state.currentNodeId) {
      return state;
    }

    const advanced = advanceDialogueFlow({
      npcId: state.activeNpcId,
      nodeId: state.currentNodeId,
      optionIndex,
      stores: collectStores(),
      visitedNodeKeys: state.visitedNodeKeys,
    });

    set({
      focusedNpcId: state.focusedNpcId,
      activeNpcId: advanced.closeDialogue ? null : state.activeNpcId,
      currentNodeId: advanced.closeDialogue ? null : advanced.currentNodeId,
      isOpen: !advanced.closeDialogue,
      requestedTradeNpcId:
        advanced.requestTradeNpcId ?? state.requestedTradeNpcId,
      visitedNodeKeys: advanced.closeDialogue ? [] : advanced.visitedNodeKeys,
    });

    return get();
  },
  closeDialogue: () =>
    set((state) => {
      useGameStore.getState().resetControls();
      return {
        focusedNpcId: state.focusedNpcId,
        activeNpcId: null,
        currentNodeId: null,
        isOpen: false,
        requestedTradeNpcId: null,
        visitedNodeKeys: [],
      };
    }),
  clearTradeRequest: () =>
    set({
      requestedTradeNpcId: null,
    }),
  resetDialogue: () =>
    set({
      ...initialDialogueState,
    }),
}));

export const dialogueStoreDefaults = {
  initialDialogueState,
};
