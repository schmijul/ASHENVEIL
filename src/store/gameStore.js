import { create } from "zustand";
import {
  COMBAT_CONSTANTS,
  createDodgeState,
  getHeavyAttackWindow,
  getLightAttackWindow,
  resolveBlockDamage,
} from "../utils/combatMath";
import { useInventoryStore } from "./inventoryStore";
import { useQuestStore } from "./questStore";
import { rollEnemyLoot } from "../utils/boarAI";
import { sampleTerrainHeight } from "../utils/terrainGeneration";

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
  position: [0, 0.675, 0],
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

const initialControlsState = {
  forward: false,
  backward: false,
  left: false,
  right: false,
  sprint: false,
};

const initialCameraState = {
  yaw: 0,
  pitch: 0.55,
  distance: 9.5,
};

const initialInteractionState = {
  focusedNpcId: null,
  activeNpcId: null,
  dialogueOpen: false,
};

const initialCombatState = {
  combatMode: false,
  isAttacking: false,
  isBlocking: false,
  isDodging: false,
  comboStep: 0,
  lastAttackAt: 0,
  attackWindow: null,
  guardStartedAt: null,
  dodgeDirection: [0, 1],
  dodgeEndsAt: 0,
  invulnerableUntil: 0,
  staminaRegenBlockedUntil: 0,
  playerHitFlashUntil: 0,
  targets: {
    training_dummy: {
      id: "training_dummy",
      kind: "dummy",
      position: [0, sampleTerrainHeight(0, 9, { seed: 7 }) + 1.1, 9],
      radius: 0.9,
      health: 90,
      maxHealth: 90,
      alive: true,
      attackDamage: 10,
      attackCooldown: 1.3,
      nextAttackAt: 0,
      staggeredUntil: 0,
      hitFlashUntil: 0,
    },
  },
};

const clamp = (value, min, max) => Math.min(max, Math.max(min, value));

export const useGameStore = create((set) => ({
  player: { ...initialPlayerState },
  world: { ...initialWorldState },
  controls: { ...initialControlsState },
  camera: { ...initialCameraState },
  interaction: { ...initialInteractionState },
  combat: { ...initialCombatState },
  setPlayerPosition: (position) =>
    set((state) => ({
      player: { ...state.player, position: [...position] },
    })),
  setPlayerRotation: (rotation) =>
    set((state) => ({
      player: { ...state.player, rotation },
    })),
  setControlState: (control, value) =>
    set((state) => {
      if (!(control in state.controls)) {
        return state;
      }

      return {
        controls: {
          ...state.controls,
          [control]: value,
        },
      };
    }),
  resetControls: () =>
    set({
      controls: { ...initialControlsState },
    }),
  setCameraOrbit: ({ yaw, pitch, distance }) =>
    set((state) => ({
      camera: {
        ...state.camera,
        ...(typeof yaw === "number" ? { yaw } : {}),
        ...(typeof pitch === "number"
          ? { pitch: clamp(pitch, 0.24, 1.12) }
          : {}),
        ...(typeof distance === "number"
          ? { distance: clamp(distance, 4.5, 16) }
          : {}),
      },
    })),
  setFocusedNpc: (npcId) =>
    set((state) => {
      if (state.interaction.focusedNpcId === npcId) {
        return state;
      }

      return {
        interaction: {
          ...state.interaction,
          focusedNpcId: npcId,
        },
      };
    }),
  startNpcInteraction: (npcId) =>
    set((state) => ({
      interaction: {
        ...state.interaction,
        activeNpcId: npcId,
        dialogueOpen: Boolean(npcId),
      },
      world: {
        ...state.world,
        questFlags: {
          ...state.world.questFlags,
          ...(npcId === "maren" ? { metMaren: true } : {}),
        },
      },
    })),
  endNpcInteraction: () =>
    set((state) => ({
      interaction: {
        ...state.interaction,
        activeNpcId: null,
        dialogueOpen: false,
      },
    })),
  interact: () =>
    set((state) => {
      if (state.interaction.dialogueOpen) {
        return {
          interaction: {
            ...state.interaction,
            activeNpcId: null,
            dialogueOpen: false,
          },
        };
      }

      if (!state.interaction.focusedNpcId) {
        state.collectNearbyLoot(state.player.position);
        return state;
      }

      return {
        interaction: {
          ...state.interaction,
          activeNpcId: state.interaction.focusedNpcId,
          dialogueOpen: true,
        },
        world: {
          ...state.world,
          questFlags: {
            ...state.world.questFlags,
            ...(state.interaction.focusedNpcId === "maren"
              ? { metMaren: true }
              : {}),
          },
        },
      };
    }),
  setCombatState: (patch) =>
    set((state) => ({
      combat:
        typeof patch === "function"
          ? patch(state.combat)
          : { ...state.combat, ...patch },
    })),
  startLightAttack: (now) =>
    set((state) => {
      if (
        state.combat.isAttacking ||
        state.combat.isDodging ||
        state.player.stamina <
          getLightAttackWindow({
            comboStep: state.combat.comboStep,
            lastAttackAt: state.combat.lastAttackAt,
            now,
          }).staminaCost
      ) {
        return state;
      }

      const attackWindow = getLightAttackWindow({
        comboStep: state.combat.comboStep,
        lastAttackAt: state.combat.lastAttackAt,
        now,
      });

      return {
        player: {
          ...state.player,
          stamina: clamp(
            state.player.stamina - attackWindow.staminaCost,
            0,
            state.player.maxStamina,
          ),
        },
        combat: {
          ...state.combat,
          combatMode: true,
          isAttacking: true,
          isBlocking: false,
          attackWindow,
          comboStep: attackWindow.comboStep,
          lastAttackAt: now,
          guardStartedAt: null,
          staminaRegenBlockedUntil: now + COMBAT_CONSTANTS.staminaRegenDelay,
        },
      };
    }),
  beginGuard: (now) =>
    set((state) => {
      if (state.combat.isAttacking || state.combat.isDodging) {
        return state;
      }

      return {
        combat: {
          ...state.combat,
          combatMode: true,
          isBlocking: true,
          guardStartedAt: now,
          staminaRegenBlockedUntil: now + COMBAT_CONSTANTS.staminaRegenDelay,
        },
      };
    }),
  stopBlocking: () =>
    set((state) => ({
      combat: {
        ...state.combat,
        isBlocking: false,
        guardStartedAt: null,
      },
    })),
  releaseGuard: (now) =>
    set((state) => {
      if (!state.combat.isBlocking) {
        return state;
      }

      const heldDuration = now - (state.combat.guardStartedAt ?? now);
      if (
        heldDuration >= COMBAT_CONSTANTS.heavyHoldThreshold &&
        state.player.stamina >= getHeavyAttackWindow({ now }).staminaCost
      ) {
        const heavyAttack = getHeavyAttackWindow({ now });

        return {
          player: {
            ...state.player,
            stamina: clamp(
              state.player.stamina - heavyAttack.staminaCost,
              0,
              state.player.maxStamina,
            ),
          },
          combat: {
            ...state.combat,
            combatMode: true,
            isBlocking: false,
            isAttacking: true,
            attackWindow: heavyAttack,
            comboStep: 0,
            lastAttackAt: now,
            guardStartedAt: null,
            staminaRegenBlockedUntil: now + COMBAT_CONSTANTS.staminaRegenDelay,
          },
        };
      }

      return {
        combat: {
          ...state.combat,
          isBlocking: false,
          guardStartedAt: null,
        },
      };
    }),
  triggerDodge: (direction, now) =>
    set((state) => {
      if (
        state.combat.isDodging ||
        state.combat.isAttacking ||
        state.player.stamina < COMBAT_CONSTANTS.dodgeCost
      ) {
        return state;
      }

      const dodgeState = createDodgeState({ direction, now });
      return {
        player: {
          ...state.player,
          stamina: clamp(
            state.player.stamina - COMBAT_CONSTANTS.dodgeCost,
            0,
            state.player.maxStamina,
          ),
        },
        combat: {
          ...state.combat,
          combatMode: true,
          isDodging: true,
          isBlocking: false,
          isAttacking: false,
          attackWindow: null,
          comboStep: 0,
          ...dodgeState,
          staminaRegenBlockedUntil: now + COMBAT_CONSTANTS.staminaRegenDelay,
        },
      };
    }),
  finishDodge: () =>
    set((state) => ({
      combat: {
        ...state.combat,
        isDodging: false,
      },
    })),
  markAttackResolved: () =>
    set((state) => ({
      combat: {
        ...state.combat,
        attackWindow: state.combat.attackWindow
          ? { ...state.combat.attackWindow, hitResolved: true }
          : null,
      },
    })),
  finishAttack: (now) =>
    set((state) => ({
      combat: {
        ...state.combat,
        isAttacking: false,
        attackWindow: null,
        staminaRegenBlockedUntil: now + COMBAT_CONSTANTS.staminaRegenDelay,
      },
    })),
  setTargetPosition: (targetId, position) =>
    set((state) => {
      const target = state.combat.targets[targetId];
      if (!target) {
        return state;
      }

      return {
        combat: {
          ...state.combat,
          targets: {
            ...state.combat.targets,
            [targetId]: {
              ...target,
              position: [...position],
            },
          },
        },
      };
    }),
  upsertCombatTargets: (targets) =>
    set((state) => ({
      combat: {
        ...state.combat,
        targets: targets.reduce(
          (nextTargets, target) => ({
            ...nextTargets,
            [target.id]: target,
          }),
          { ...state.combat.targets },
        ),
      },
    })),
  setCombatTargetState: (targetId, patch) =>
    set((state) => {
      const target = state.combat.targets[targetId];
      if (!target) {
        return state;
      }

      return {
        combat: {
          ...state.combat,
          targets: {
            ...state.combat.targets,
            [targetId]: {
              ...target,
              ...patch,
            },
          },
        },
      };
    }),
  removeCombatTarget: (targetId) =>
    set((state) => {
      if (!state.combat.targets[targetId]) {
        return state;
      }

      const nextTargets = { ...state.combat.targets };
      delete nextTargets[targetId];

      return {
        combat: {
          ...state.combat,
          targets: nextTargets,
        },
      };
    }),
  damageTarget: (targetId, damage, now, staggerDuration = 0) =>
    set((state) => {
      const target = state.combat.targets[targetId];
      if (!target || !target.alive) {
        return state;
      }

      const nextHealth = clamp(target.health - damage, 0, target.maxHealth);
      const diedNow = nextHealth <= 0 && target.alive;
      const lootDrop = diedNow && target.kind === "boar" ? rollEnemyLoot(target) : target.lootDrop;
      if (diedNow && target.kind === "boar") {
        useQuestStore.getState().recordObjectiveEvent({
          type: "kill",
          target: "boar",
          count: 1,
        });
      }

      return {
        combat: {
          ...state.combat,
          targets: {
            ...state.combat.targets,
            [targetId]: {
              ...target,
              health: nextHealth,
              alive: nextHealth > 0,
              staggeredUntil: Math.max(
                target.staggeredUntil,
                now + staggerDuration,
              ),
              hitFlashUntil: now + 0.18,
              lootDrop,
              aiState: nextHealth > 0 ? target.aiState : "dead",
            },
          },
        },
      };
    }),
  resetTarget: (targetId) =>
    set((state) => {
      const target = initialCombatState.targets[targetId];
      if (!target) {
        return state;
      }

      return {
        combat: {
          ...state.combat,
          targets: {
            ...state.combat.targets,
            [targetId]: { ...target },
          },
        },
      };
    }),
  setTargetNextAttackAt: (targetId, nextAttackAt) =>
    set((state) => {
      const target = state.combat.targets[targetId];
      if (!target) {
        return state;
      }

      return {
        combat: {
          ...state.combat,
          targets: {
            ...state.combat.targets,
            [targetId]: {
              ...target,
              nextAttackAt,
            },
          },
        },
      };
    }),
  collectNearbyLoot: (position, radius = 2.2) =>
    set((state) => {
      let didCollect = false;
      const nextTargets = { ...state.combat.targets };

      Object.values(state.combat.targets).forEach((target) => {
        if (
          target.kind !== "boar" ||
          target.alive ||
          target.looted ||
          !target.lootDrop?.length
        ) {
          return;
        }

        const distance = Math.hypot(
          target.position[0] - position[0],
          target.position[2] - position[2],
        );

        if (distance > radius) {
          return;
        }

        target.lootDrop.forEach((drop) => {
          useInventoryStore.getState().addItem(drop.itemId, drop.quantity);
          useQuestStore.getState().recordObjectiveEvent({
            type: "collect",
            item: drop.itemId,
            count: drop.quantity,
          });
        });

        nextTargets[target.id] = {
          ...target,
          looted: true,
        };
        didCollect = true;
      });

      if (!didCollect) {
        return state;
      }

      return {
        combat: {
          ...state.combat,
          targets: nextTargets,
        },
      };
    }),
  applyIncomingDamage: (incomingDamage, now) =>
    set((state) => {
      if (now < state.combat.invulnerableUntil) {
        return state;
      }

      const defence = resolveBlockDamage({
        incomingDamage,
        isBlocking: state.combat.isBlocking,
        stamina: state.player.stamina,
      });

      return {
        player: {
          ...state.player,
          health: clamp(
            state.player.health - defence.damageTaken,
            0,
            state.player.maxHealth,
          ),
          stamina: clamp(
            state.player.stamina - defence.staminaSpent,
            0,
            state.player.maxStamina,
          ),
        },
        combat: {
          ...state.combat,
          combatMode: true,
          playerHitFlashUntil: now + 0.18,
          staminaRegenBlockedUntil: now + COMBAT_CONSTANTS.staminaRegenDelay,
          isBlocking:
            state.combat.isBlocking &&
            state.player.stamina - defence.staminaSpent > 0,
        },
      };
    }),
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
      controls: { ...initialControlsState },
      camera: { ...initialCameraState },
      interaction: { ...initialInteractionState },
      combat: { ...initialCombatState },
    }),
}));

export const gameStoreDefaults = {
  initialPlayerState,
  initialWorldState,
  initialQuestFlags,
  initialControlsState,
  initialCameraState,
  initialInteractionState,
  initialCombatState,
};
