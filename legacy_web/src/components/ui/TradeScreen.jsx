import { useMemo } from "react";
import npcsData from "../../data/npcs.json" with { type: "json" };
import { useGameStore } from "../../store/gameStore";
import { useInventoryStore } from "../../store/inventoryStore";
import { useTradeStore } from "../../store/tradeStore";

const npcCatalog = Object.fromEntries(npcsData.npcs.map((npc) => [npc.id, npc]));

export default function TradeScreen() {
  const tradeOpen = useGameStore((state) => state.ui.tradeOpen);
  const activeMerchantId = useGameStore((state) => state.ui.activeMerchantId);
  const gold = useGameStore((state) => state.player.gold);
  const closeTrade = useGameStore((state) => state.closeTrade);
  const inventory = useInventoryStore((state) => state.inventory);
  const getItemDefinition = useInventoryStore((state) => state.getItemDefinition);
  const getMerchantInventory = useTradeStore((state) => state.getMerchantInventory);
  const buyItem = useTradeStore((state) => state.buyItem);
  const sellItem = useTradeStore((state) => state.sellItem);

  const merchantInventory = useMemo(
    () => getMerchantInventory(activeMerchantId),
    [activeMerchantId, getMerchantInventory],
  );

  if (!tradeOpen || !activeMerchantId) {
    return null;
  }

  const merchant = npcCatalog[activeMerchantId];

  return (
    <div className="trade-screen">
      <div className="trade-screen__panel">
        <div className="trade-screen__header">
          <div>
            <p className="trade-screen__eyebrow">Trade</p>
            <h2 className="trade-screen__title">{merchant?.name ?? activeMerchantId}</h2>
          </div>
          <button className="trade-screen__close" onClick={closeTrade} type="button">
            Close
          </button>
        </div>
        <div className="trade-screen__gold">{gold} gold</div>
        <div className="trade-screen__body">
          <section className="trade-screen__column">
            <div className="trade-screen__column-header">
              <h3>Your Inventory</h3>
              <span>Click to sell</span>
            </div>
            <div className="trade-screen__list">
              {inventory.map((entry) => {
                const definition = getItemDefinition(entry.itemId);
                return (
                  <button
                    key={`sell-${entry.itemId}`}
                    className="trade-screen__item"
                    onClick={() => sellItem(activeMerchantId, entry.itemId, 1)}
                    type="button"
                  >
                    <span>{definition?.name ?? entry.itemId}</span>
                    <span>x{entry.quantity}</span>
                    <span>{definition?.price ?? 0}g</span>
                  </button>
                );
              })}
            </div>
          </section>

          <section className="trade-screen__column">
            <div className="trade-screen__column-header">
              <h3>{merchant?.name ?? "Merchant"}'s Stock</h3>
              <span>Click to buy</span>
            </div>
            <div className="trade-screen__list">
              {merchantInventory.map((entry) => {
                const definition = getItemDefinition(entry.itemId);
                return (
                  <button
                    key={`buy-${entry.itemId}`}
                    className="trade-screen__item"
                    onClick={() => buyItem(activeMerchantId, entry.itemId, 1)}
                    type="button"
                  >
                    <span>{definition?.name ?? entry.itemId}</span>
                    <span>x{entry.quantity}</span>
                    <span>{definition?.price ?? 0}g</span>
                  </button>
                );
              })}
            </div>
          </section>
        </div>
      </div>
    </div>
  );
}
