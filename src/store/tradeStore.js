import { create } from "zustand";
import npcsData from "../data/npcs.json" with { type: "json" };
import { useGameStore } from "./gameStore";
import { useInventoryStore } from "./inventoryStore";
import { useQuestStore } from "./questStore";

const toStackedInventory = (items = []) => {
  const stacks = new Map();
  items.forEach((itemId) => {
    stacks.set(itemId, (stacks.get(itemId) ?? 0) + 1);
  });

  return Array.from(stacks.entries()).map(([itemId, quantity]) => ({
    itemId,
    quantity,
  }));
};

const initialMerchantInventories = Object.fromEntries(
  npcsData.npcs
    .filter((npc) => Array.isArray(npc.inventory))
    .map((npc) => [npc.id, toStackedInventory(npc.inventory)]),
);

const cloneInventory = (inventory = []) =>
  inventory.map((entry) => ({ ...entry }));

const findItemIndex = (inventory, itemId) =>
  inventory.findIndex((entry) => entry.itemId === itemId);

const adjustInventoryStack = (inventory, itemId, quantityDelta) => {
  const stackIndex = findItemIndex(inventory, itemId);
  if (stackIndex === -1 && quantityDelta > 0) {
    inventory.push({ itemId, quantity: quantityDelta });
    return inventory;
  }

  if (stackIndex === -1) {
    return inventory;
  }

  const nextQuantity = inventory[stackIndex].quantity + quantityDelta;
  if (nextQuantity > 0) {
    inventory[stackIndex] = {
      ...inventory[stackIndex],
      quantity: nextQuantity,
    };
  } else {
    inventory.splice(stackIndex, 1);
  }

  return inventory;
};

const getItemPrice = (itemId) =>
  useInventoryStore.getState().getItemDefinition(itemId)?.price ?? 0;

export const useTradeStore = create((set) => ({
  merchantInventories: initialMerchantInventories,
  getMerchantInventory: (merchantId) =>
    useTradeStore.getState().merchantInventories[merchantId] ?? [],
  buyItem: (merchantId, itemId, quantity = 1) =>
    set((state) => {
      if (quantity <= 0) {
        return state;
      }

      const merchantInventory = cloneInventory(state.merchantInventories[merchantId]);
      const stackIndex = findItemIndex(merchantInventory, itemId);
      if (
        stackIndex === -1 ||
        merchantInventory[stackIndex].quantity < quantity
      ) {
        return state;
      }

      const totalPrice = getItemPrice(itemId) * quantity;
      if (useGameStore.getState().player.gold < totalPrice) {
        return state;
      }

      adjustInventoryStack(merchantInventory, itemId, -quantity);
      useGameStore.getState().modifyGold(-totalPrice);
      useInventoryStore.getState().addItem(itemId, quantity);
      useQuestStore.getState().recordObjectiveEvent({
        type: "interact",
        target: merchantId,
        action: "buy",
        item: itemId,
        count: quantity,
      });

      return {
        merchantInventories: {
          ...state.merchantInventories,
          [merchantId]: merchantInventory,
        },
      };
    }),
  sellItem: (merchantId, itemId, quantity = 1) =>
    set((state) => {
      if (quantity <= 0 || !useInventoryStore.getState().hasItem(itemId, quantity)) {
        return state;
      }

      const merchantInventory = cloneInventory(state.merchantInventories[merchantId]);
      const totalPrice = getItemPrice(itemId) * quantity;

      adjustInventoryStack(merchantInventory, itemId, quantity);
      useInventoryStore.getState().removeItem(itemId, quantity);
      useGameStore.getState().modifyGold(totalPrice);
      useQuestStore.getState().recordObjectiveEvent({
        type: "interact",
        target: merchantId,
        action: "sell",
        item: itemId,
        count: quantity,
      });

      return {
        merchantInventories: {
          ...state.merchantInventories,
          [merchantId]: merchantInventory,
        },
      };
    }),
  resetTradeStore: () =>
    set({
      merchantInventories: initialMerchantInventories,
    }),
}));

export const tradeStoreDefaults = {
  initialMerchantInventories,
};
