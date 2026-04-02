import { create } from "zustand";
import itemsData from "../data/items.json" with { type: "json" };

const itemCatalog = Object.fromEntries(
  itemsData.items.map((item) => [item.id, item]),
);

const initialEquipment = {
  weapon: null,
  head: null,
  chest: null,
  legs: null,
  boots: null,
  gloves: null,
  shield: null,
  accessory1: null,
  accessory2: null,
};

const cloneInventory = (inventory) =>
  inventory.map((entry) => ({ ...entry }));

const getItemDefinition = (itemId) => itemCatalog[itemId] ?? null;

const getStackWeight = (entry) => {
  const definition = getItemDefinition(entry.itemId);
  return (definition?.weight ?? 0) * entry.quantity;
};

const findItemIndex = (inventory, itemId) =>
  inventory.findIndex((entry) => entry.itemId === itemId);

const createStack = (itemId, quantity) => ({ itemId, quantity });

const recalculateTotalWeight = (inventory, equipment) => {
  const itemWeight = inventory.reduce((total, entry) => total + getStackWeight(entry), 0);
  const equipmentWeight = Object.values(equipment).reduce((total, itemId) => {
    const definition = itemId ? getItemDefinition(itemId) : null;
    return total + (definition?.weight ?? 0);
  }, 0);

  return itemWeight + equipmentWeight;
};

export const useInventoryStore = create((set, get) => ({
  inventory: [],
  equipment: { ...initialEquipment },
  capacity: 40,
  totalWeight: 0,
  getItemDefinition,
  addItem: (itemId, quantity = 1) =>
    set((state) => {
      const definition = getItemDefinition(itemId);
      if (!definition || quantity <= 0) {
        return state;
      }

      const inventory = cloneInventory(state.inventory);
      const stackIndex = findItemIndex(inventory, itemId);

      if (stackIndex === -1) {
        inventory.push(createStack(itemId, quantity));
      } else {
        inventory[stackIndex] = {
          ...inventory[stackIndex],
          quantity: inventory[stackIndex].quantity + quantity,
        };
      }

      const totalWeight = recalculateTotalWeight(inventory, state.equipment);

      return {
        inventory,
        totalWeight,
      };
    }),
  removeItem: (itemId, quantity = 1) =>
    set((state) => {
      const inventory = cloneInventory(state.inventory);
      const stackIndex = findItemIndex(inventory, itemId);

      if (stackIndex === -1 || quantity <= 0) {
        return state;
      }

      const nextQuantity = inventory[stackIndex].quantity - quantity;
      if (nextQuantity > 0) {
        inventory[stackIndex] = {
          ...inventory[stackIndex],
          quantity: nextQuantity,
        };
      } else {
        inventory.splice(stackIndex, 1);
      }

      const totalWeight = recalculateTotalWeight(inventory, state.equipment);

      return {
        inventory,
        totalWeight,
      };
    }),
  setItemQuantity: (itemId, quantity) =>
    set((state) => {
      const inventory = cloneInventory(state.inventory);
      const stackIndex = findItemIndex(inventory, itemId);

      if (quantity <= 0) {
        if (stackIndex === -1) {
          return state;
        }

        inventory.splice(stackIndex, 1);
      } else if (stackIndex === -1) {
        inventory.push(createStack(itemId, quantity));
      } else {
        inventory[stackIndex] = {
          ...inventory[stackIndex],
          quantity,
        };
      }

      const totalWeight = recalculateTotalWeight(inventory, state.equipment);

      return {
        inventory,
        totalWeight,
      };
    }),
  equipItem: (slot, itemId) =>
    set((state) => {
      const definition = getItemDefinition(itemId);
      if (!definition) {
        return state;
      }

      const equipment = {
        ...state.equipment,
        [slot]: itemId,
      };

      const totalWeight = recalculateTotalWeight(state.inventory, equipment);

      return {
        equipment,
        totalWeight,
      };
    }),
  unequipItem: (slot) =>
    set((state) => {
      if (!(slot in state.equipment)) {
        return state;
      }

      const equipment = {
        ...state.equipment,
        [slot]: null,
      };

      const totalWeight = recalculateTotalWeight(state.inventory, equipment);

      return {
        equipment,
        totalWeight,
      };
    }),
  setCapacity: (capacity) =>
    set({
      capacity: Math.max(0, capacity),
    }),
  clearInventory: () =>
    set((state) => ({
      inventory: [],
      equipment: { ...initialEquipment },
      totalWeight: recalculateTotalWeight([], initialEquipment),
      capacity: state.capacity,
    })),
  canCarryItem: (itemId, quantity = 1) => {
    const definition = getItemDefinition(itemId);
    if (!definition) {
      return false;
    }

    const addedWeight = (definition.weight ?? 0) * quantity;
    return get().totalWeight + addedWeight <= get().capacity;
  },
}));

export const inventoryStoreDefaults = {
  initialEquipment,
  itemCatalog,
};
