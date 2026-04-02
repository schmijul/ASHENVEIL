import { afterEach, describe, expect, it } from "vitest";
import { useDialogueStore } from "./dialogueStore";
import { useFactionStore } from "./factionStore";
import { useGameStore } from "./gameStore";
import { useInventoryStore } from "./inventoryStore";
import { useQuestStore } from "./questStore";
import { useTradeStore } from "./tradeStore";

const resetStores = () => {
  useDialogueStore.getState().resetDialogue();
  useFactionStore.getState().resetFactions();
  useGameStore.getState().resetGameState();
  useInventoryStore.getState().clearInventory();
  useQuestStore.getState().resetAllQuests();
  useTradeStore.getState().resetTradeStore();
};

afterEach(() => {
  resetStores();
});

describe("tradeStore", () => {
  it("loads Korvin's stock from NPC data", () => {
    const inventory = useTradeStore.getState().getMerchantInventory("korvin");

    expect(inventory).toEqual(
      expect.arrayContaining([
        { itemId: "iron_sword", quantity: 1 },
        { itemId: "padded_vest", quantity: 1 },
        { itemId: "health_potion", quantity: 3 },
      ]),
    );
  });

  it("buys items from Korvin and deducts gold", () => {
    useGameStore.getState().setGold(60);

    useTradeStore.getState().buyItem("korvin", "iron_sword", 1);

    expect(useGameStore.getState().player.gold).toBe(10);
    expect(useInventoryStore.getState().hasItem("iron_sword", 1)).toBe(true);
  });

  it("sells boar meat, adds merchant stock, and completes the Korvin quest flag", () => {
    useInventoryStore.getState().addItem("boar_meat", 2);
    useQuestStore.getState().startQuest("sell_to_korvin");

    useTradeStore.getState().sellItem("korvin", "boar_meat", 1);

    expect(useGameStore.getState().player.gold).toBe(5);
    expect(useInventoryStore.getState().getItemQuantity("boar_meat")).toBe(1);
    expect(useGameStore.getState().world.questFlags.sold_first_meat).toBe(true);
    expect(useTradeStore.getState().getMerchantInventory("korvin")).toEqual(
      expect.arrayContaining([{ itemId: "boar_meat", quantity: 1 }]),
    );
  });
});
