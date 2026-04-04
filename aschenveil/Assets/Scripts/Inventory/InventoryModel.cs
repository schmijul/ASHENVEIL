using Ashenveil.Data;

namespace Ashenveil.Inventory
{
    public sealed class InventoryModel
    {
        public const int DefaultColumns = 6;
        public const int DefaultRows = 4;

        private readonly InventoryGrid _grid;
        private readonly EquipmentLoadout _equipment;

        public InventoryModel()
            : this(DefaultColumns, DefaultRows)
        {
        }

        public InventoryModel(int columns, int rows)
        {
            _grid = new InventoryGrid(columns, rows);
            _equipment = new EquipmentLoadout();
        }

        public InventoryGrid Grid => _grid;

        public EquipmentLoadout Equipment => _equipment;

        public bool TryAddItem(ItemData item, int quantity)
        {
            return _grid.TryAddItem(item, quantity);
        }

        public int AddItem(ItemData item, int quantity)
        {
            return _grid.AddItem(item, quantity);
        }

        public bool TryRemoveItem(ItemData item, int quantity)
        {
            return _grid.TryRemoveItem(item, quantity);
        }

        public int RemoveItem(ItemData item, int quantity)
        {
            return _grid.RemoveItem(item, quantity);
        }

        public int GetItemCount(ItemData item)
        {
            return _grid.GetItemCount(item);
        }

        public bool CanEquip(ItemData item)
        {
            return item != null && item.IsEquippable;
        }

        public bool TryEquip(ItemData item)
        {
            if (!CanEquip(item) || !_grid.ContainsItem(item, 1))
            {
                return false;
            }

            EquipSlot slot = item.equipSlot;
            ItemData currentItem = _equipment.GetEquippedItem(slot);
            if (currentItem == item)
            {
                return false;
            }

            if (currentItem != null && !_grid.CanAddItem(currentItem, 1))
            {
                return false;
            }

            if (!_grid.TryRemoveItem(item, 1))
            {
                return false;
            }

            if (currentItem != null)
            {
                _grid.AddItem(currentItem, 1);
            }

            return _equipment.TryEquip(item, out _);
        }

        public bool TryUnequip(EquipSlot slot)
        {
            ItemData currentItem = _equipment.GetEquippedItem(slot);
            if (currentItem == null || !_grid.CanAddItem(currentItem, 1))
            {
                return false;
            }

            if (!_equipment.TryUnequip(slot, out ItemData removedItem))
            {
                return false;
            }

            _grid.AddItem(removedItem, 1);
            return true;
        }

        public void Clear()
        {
            _grid.Clear();
            _equipment.Clear();
        }
    }
}
