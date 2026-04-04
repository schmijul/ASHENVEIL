using System;
using Ashenveil.Data;

namespace Ashenveil.Inventory
{
    public sealed class EquipmentLoadout
    {
        private readonly ItemData[] _equippedItems = new ItemData[(int)EquipSlot.Boots + 1];

        public ItemData GetEquippedItem(EquipSlot slot)
        {
            if (slot == EquipSlot.None)
            {
                return null;
            }

            return _equippedItems[ToIndex(slot)];
        }

        public bool IsOccupied(EquipSlot slot)
        {
            return GetEquippedItem(slot) != null;
        }

        public bool TryEquip(ItemData item, out ItemData previousItem)
        {
            previousItem = null;

            if (item == null || !item.IsEquippable)
            {
                return false;
            }

            int index = ToIndex(item.equipSlot);
            ItemData currentItem = _equippedItems[index];
            if (currentItem == item)
            {
                return false;
            }

            previousItem = currentItem;
            _equippedItems[index] = item;
            return true;
        }

        public bool TryUnequip(EquipSlot slot, out ItemData removedItem)
        {
            removedItem = null;

            if (slot == EquipSlot.None)
            {
                return false;
            }

            int index = ToIndex(slot);
            removedItem = _equippedItems[index];
            if (removedItem == null)
            {
                return false;
            }

            _equippedItems[index] = null;
            return true;
        }

        public void Clear()
        {
            Array.Clear(_equippedItems, 0, _equippedItems.Length);
        }

        private static int ToIndex(EquipSlot slot)
        {
            int index = (int)slot;
            if (index <= 0 || index >= (int)EquipSlot.Boots + 1)
            {
                throw new ArgumentOutOfRangeException(nameof(slot));
            }

            return index;
        }
    }
}
