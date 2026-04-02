import { useMemo } from "react";
import { useGameStore } from "../../store/gameStore";
import { useInventoryStore } from "../../store/inventoryStore";

const itemToneMap = {
  weapon: ["#5c4033", "#d4a843"],
  armor: ["#7a7a7a", "#e8dcc8"],
  trade_good: ["#8b6914", "#e8dcc8"],
  ingredient: ["#3b5f3b", "#e8dcc8"],
  consumable: ["#5c7a4a", "#e0ffff"],
  quest_item: ["#a08090", "#f3ede2"],
  material: ["#00f5ff", "#10130f"],
};

const equipmentLabels = {
  weapon: "Weapon",
  head: "Head",
  chest: "Chest",
  legs: "Legs",
  boots: "Boots",
  gloves: "Gloves",
  shield: "Shield",
  accessory1: "Accessory I",
  accessory2: "Accessory II",
};

const getItemTone = (definition) =>
  itemToneMap[definition?.type] ?? ["#5c4033", "#f3ede2"];

export default function InventoryScreen() {
  const inventoryOpen = useGameStore((state) => state.ui.inventoryOpen);
  const tradeOpen = useGameStore((state) => state.ui.tradeOpen);
  const gold = useGameStore((state) => state.player.gold);
  const {
    inventory,
    equipment,
    totalWeight,
    capacity,
    getItemDefinition,
    equipItemById,
    unequipItem,
    resolveEquipmentSlot,
  } = useInventoryStore((state) => ({
    inventory: state.inventory,
    equipment: state.equipment,
    totalWeight: state.totalWeight,
    capacity: state.capacity,
    getItemDefinition: state.getItemDefinition,
    equipItemById: state.equipItemById,
    unequipItem: state.unequipItem,
    resolveEquipmentSlot: state.resolveEquipmentSlot,
  }));

  const inventoryEntries = useMemo(
    () =>
      inventory.map((entry) => ({
        ...entry,
        definition: getItemDefinition(entry.itemId),
      })),
    [getItemDefinition, inventory],
  );
  const isOverweight = totalWeight > capacity;

  if (!inventoryOpen || tradeOpen) {
    return null;
  }

  return (
    <div className="inventory-screen">
      <div className="inventory-screen__panel">
        <div className="inventory-screen__header">
          <div>
            <p className="inventory-screen__eyebrow">Inventory</p>
            <h2 className="inventory-screen__title">Carry What You Can Defend</h2>
          </div>
          <div className="inventory-screen__meta">
            <span>{gold} gold</span>
            <span className={isOverweight ? "inventory-screen__weight inventory-screen__weight--over" : "inventory-screen__weight"}>
              {totalWeight.toFixed(1)} / {capacity.toFixed(1)} weight
            </span>
          </div>
        </div>

        <div className="inventory-screen__body">
          <section className="inventory-screen__equipment">
            <h3 className="inventory-screen__section-title">Equipped</h3>
            <div className="inventory-screen__slots">
              {Object.entries(equipmentLabels).map(([slot, label]) => {
                const equippedItemId = equipment[slot];
                const equippedItem = equippedItemId
                  ? getItemDefinition(equippedItemId)
                  : null;

                return (
                  <button
                    key={slot}
                    className="inventory-screen__slot"
                    onClick={() => equippedItemId && unequipItem(slot)}
                    type="button"
                  >
                    <span className="inventory-screen__slot-label">{label}</span>
                    <span className="inventory-screen__slot-value">
                      {equippedItem ? equippedItem.name : "Empty"}
                    </span>
                  </button>
                );
              })}
            </div>
          </section>

          <section className="inventory-screen__inventory">
            <div className="inventory-screen__section-header">
              <h3 className="inventory-screen__section-title">Backpack</h3>
              {isOverweight ? (
                <span className="inventory-screen__warning">Overweight: movement slowed</span>
              ) : null}
            </div>
            <div className="inventory-screen__grid">
              {inventoryEntries.map((entry) => {
                const definition = entry.definition;
                const [backgroundColor, textColor] = getItemTone(definition);
                const equipSlot = resolveEquipmentSlot(entry.itemId);
                return (
                  <button
                    key={entry.itemId}
                    className="inventory-screen__item"
                    onClick={() => equipSlot && equipItemById(entry.itemId)}
                    type="button"
                  >
                    <span
                      className="inventory-screen__badge"
                      style={{
                        backgroundColor,
                        color: textColor,
                      }}
                    >
                      {definition?.name?.slice(0, 1) ?? "?"}
                    </span>
                    <span className="inventory-screen__item-name">
                      {definition?.name ?? entry.itemId}
                    </span>
                    <span className="inventory-screen__item-quantity">x{entry.quantity}</span>
                    <span className="inventory-screen__item-weight">
                      {(definition?.weight ?? 0) * entry.quantity} wt
                    </span>
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
