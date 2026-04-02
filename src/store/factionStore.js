import { create } from "zustand";

const initialFactions = {
  kernwall: 0,
  flimmermoor: 0,
  hohensang: 0,
};

const clampReputation = (value) => Math.max(-100, Math.min(100, value));

export const useFactionStore = create((set) => ({
  factions: { ...initialFactions },
  setReputation: (factionId, value) =>
    set((state) => {
      if (!(factionId in state.factions)) {
        return state;
      }

      return {
        factions: {
          ...state.factions,
          [factionId]: clampReputation(value),
        },
      };
    }),
  adjustReputation: (factionId, delta) =>
    set((state) => {
      if (!(factionId in state.factions)) {
        return state;
      }

      return {
        factions: {
          ...state.factions,
          [factionId]: clampReputation(state.factions[factionId] + delta),
        },
      };
    }),
  resetFactions: () =>
    set({
      factions: { ...initialFactions },
    }),
}));

export const factionStoreDefaults = {
  initialFactions,
};
