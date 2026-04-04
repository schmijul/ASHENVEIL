using System;
using System.Collections.Generic;
using Ashenveil.Data;
using UnityEngine;

namespace Ashenveil.Inventory
{
    [Serializable]
    public sealed class InventorySlot
    {
        [SerializeField] private ItemData _item;
        [SerializeField] private int _quantity;

        public ItemData Item => _item;

        public int Quantity => _quantity;

        public bool IsEmpty => _item == null || _quantity <= 0;

        public int Add(ItemData item, int quantity, int maxStack)
        {
            if (item == null || quantity <= 0 || maxStack <= 0)
            {
                return 0;
            }

            if (!IsEmpty && _item != item)
            {
                return 0;
            }

            if (IsEmpty)
            {
                _item = item;
            }

            int space = Mathf.Max(0, maxStack - _quantity);
            int added = Mathf.Min(space, quantity);
            _quantity += added;
            return added;
        }

        public int Remove(int quantity)
        {
            if (quantity <= 0 || IsEmpty)
            {
                return 0;
            }

            int removed = Mathf.Min(_quantity, quantity);
            _quantity -= removed;

            if (_quantity <= 0)
            {
                Clear();
            }

            return removed;
        }

        public void Clear()
        {
            _item = null;
            _quantity = 0;
        }
    }

    public sealed class InventoryGrid
    {
        private readonly InventorySlot[] _slots;

        public InventoryGrid(int columns = 6, int rows = 4)
        {
            if (columns <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(columns));
            }

            if (rows <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(rows));
            }

            Columns = columns;
            Rows = rows;
            _slots = new InventorySlot[columns * rows];
            for (int index = 0; index < _slots.Length; index++)
            {
                _slots[index] = new InventorySlot();
            }
        }

        public int Columns { get; }

        public int Rows { get; }

        public int SlotCount => _slots.Length;

        public IReadOnlyList<InventorySlot> Slots => _slots;

        public int OccupiedSlotCount
        {
            get
            {
                int occupied = 0;
                foreach (InventorySlot slot in _slots)
                {
                    if (!slot.IsEmpty)
                    {
                        occupied++;
                    }
                }

                return occupied;
            }
        }

        public int EmptySlotCount => SlotCount - OccupiedSlotCount;

        public InventorySlot GetSlot(int index)
        {
            if (index < 0 || index >= _slots.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return _slots[index];
        }

        public void Clear()
        {
            foreach (InventorySlot slot in _slots)
            {
                slot.Clear();
            }
        }

        public bool ContainsItem(ItemData item, int quantity = 1)
        {
            return GetItemCount(item) >= quantity;
        }

        public int GetItemCount(ItemData item)
        {
            if (item == null)
            {
                return 0;
            }

            int count = 0;
            foreach (InventorySlot slot in _slots)
            {
                if (slot.Item == item)
                {
                    count += slot.Quantity;
                }
            }

            return count;
        }

        public int GetRemainingCapacity(ItemData item)
        {
            if (item == null)
            {
                return 0;
            }

            int maxStack = item.GetEffectiveMaxStack();
            int remaining = 0;

            foreach (InventorySlot slot in _slots)
            {
                if (slot.IsEmpty)
                {
                    remaining += maxStack;
                }
                else if (slot.Item == item)
                {
                    remaining += Mathf.Max(0, maxStack - slot.Quantity);
                }
            }

            return remaining;
        }

        public bool CanAddItem(ItemData item, int quantity)
        {
            return GetRemainingCapacity(item) >= quantity && quantity > 0;
        }

        public int AddItem(ItemData item, int quantity)
        {
            if (item == null || quantity <= 0)
            {
                return 0;
            }

            int maxStack = item.GetEffectiveMaxStack();
            int remaining = quantity;

            foreach (InventorySlot slot in _slots)
            {
                if (remaining <= 0)
                {
                    break;
                }

                if (!slot.IsEmpty && slot.Item == item && slot.Quantity < maxStack)
                {
                    remaining -= slot.Add(item, remaining, maxStack);
                }
            }

            foreach (InventorySlot slot in _slots)
            {
                if (remaining <= 0)
                {
                    break;
                }

                if (slot.IsEmpty)
                {
                    remaining -= slot.Add(item, remaining, maxStack);
                }
            }

            return quantity - remaining;
        }

        public bool TryAddItem(ItemData item, int quantity)
        {
            return AddItem(item, quantity) == quantity;
        }

        public int RemoveItem(ItemData item, int quantity)
        {
            if (item == null || quantity <= 0)
            {
                return 0;
            }

            int remaining = quantity;
            foreach (InventorySlot slot in _slots)
            {
                if (remaining <= 0)
                {
                    break;
                }

                if (slot.Item == item)
                {
                    remaining -= slot.Remove(remaining);
                }
            }

            return quantity - remaining;
        }

        public bool TryRemoveItem(ItemData item, int quantity)
        {
            return RemoveItem(item, quantity) == quantity;
        }
    }
}
