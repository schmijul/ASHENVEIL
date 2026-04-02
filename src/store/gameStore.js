import { create } from "zustand";

const initialQuestFlags = {
  metMaren: false,
  boars_hunted: false,
  sold_first_meat: false,
  heavy_attack_learned: false,
  aether_awakened: false,
  destroyedVillage: false,
  prolog_complete: false,
};

const initialPlayerState = {
  position: [0, 0, 0],
  rotation: 0,
  health: 100,
  maxHealth: 100,
  stamina: 100,
  maxStamina: 100,
  corruption: 0,
  gold: 0,
  skills: {},
};

const initialWorldState = {
  timeOfDay: 0,
  questFlags: { ...initialQuestFlags },
  destroyedVillage: false,
  aetherAwakened: false,
};

const clamp = (value, min, max) => Math.min(max, Math.max(min, value));

export const useGameStore = create((set) => ({
  player: { ...initialPlayerState },
  world: { ...initialWorldState },
  setPlayerPosition: (position) =>
    set((state) => ({
      player: { ...state.player, position: [...position] },
    })),
  setPlayerRotation: (rotation) =>
    set((state) => ({
      player: { ...state.player, rotation },
    })),
  setHealth: (health) =>
    set((state) => ({
      player: {
        ...state.player,
        health: clamp(health, 0, state.player.maxHealth),
      },
    })),
  modifyHealth: (delta) =>
    set((state) => {
      const nextHealth = clamp(
        state.player.health + delta,
        0,
        state.player.maxHealth,
      );

      return {
        player: { ...state.player, health: nextHealth },
      };
    }),
  setMaxHealth: (maxHealth) =>
    set((state) => ({
      player: {
        ...state.player,
        maxHealth,
        health: clamp(state.player.health, 0, maxHealth),
      },
    })),
  setStamina: (stamina) =>
    set((state) => ({
      player: {
        ...state.player,
        stamina: clamp(stamina, 0, state.player.maxStamina),
      },
    })),
  modifyStamina: (delta) =>
    set((state) => ({
      player: {
        ...state.player,
        stamina: clamp(
          state.player.stamina + delta,
          0,
          state.player.maxStamina,
        ),
      },
    })),
  setMaxStamina: (maxStamina) =>
    set((state) => ({
      player: {
        ...state.player,
        maxStamina,
        stamina: clamp(state.player.stamina, 0, maxStamina),
      },
    })),
  setCorruption: (corruption) =>
    set((state) => ({
      player: {
        ...state.player,
        corruption: clamp(corruption, 0, 100),
      },
    })),
  modifyCorruption: (delta) =>
    set((state) => ({
      player: {
        ...state.player,
        corruption: clamp(state.player.corruption + delta, 0, 100),
      },
    })),
  setGold: (gold) =>
    set((state) => ({
      player: { ...state.player, gold: Math.max(0, gold) },
    })),
  modifyGold: (delta) =>
    set((state) => ({
      player: {
        ...state.player,
        gold: Math.max(0, state.player.gold + delta),
      },
    })),
  setSkill: (skillId, value) =>
    set((state) => ({
      player: {
        ...state.player,
        skills: {
          ...state.player.skills,
          [skillId]: value,
        },
      },
    })),
  setTimeOfDay: (timeOfDay) =>
    set((state) => ({
      world: {
        ...state.world,
        timeOfDay: ((timeOfDay % 1) + 1) % 1,
      },
    })),
  setQuestFlag: (flag, value = true) =>
    set((state) => ({
      world: {
        ...state.world,
        questFlags: {
          ...state.world.questFlags,
          [flag]: value,
        },
      },
    })),
  setWorldFlags: (flags) =>
    set((state) => ({
      world: {
        ...state.world,
        ...flags,
        questFlags: {
          ...state.world.questFlags,
          ...(flags.questFlags ?? {}),
        },
      },
    })),
  resetGameState: () =>
    set({
      player: { ...initialPlayerState },
      world: { ...initialWorldState },
    }),
}));

export const gameStoreDefaults = {
  initialPlayerState,
  initialWorldState,
  initialQuestFlags,
};
