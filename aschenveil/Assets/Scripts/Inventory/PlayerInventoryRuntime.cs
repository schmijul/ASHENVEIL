using System;
using Ashenveil.Data;
using UnityEngine;

namespace Ashenveil.Inventory
{
    /// <summary>
    /// Scene-facing runtime inventory and gold state for the opening demo loop.
    /// Referenced GDD section: 5.10
    /// </summary>
    public class PlayerInventoryRuntime : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField, Min(0)] private int _startingGold;
        [SerializeField, Min(1)] private int _columns = InventoryModel.DefaultColumns;
        [SerializeField, Min(1)] private int _rows = InventoryModel.DefaultRows;

        private InventoryModel _inventory;

        public event Action<ItemData, int> ItemAdded;
        public event Action<int> GoldChanged;

        public InventoryModel Inventory => _inventory;

        public int Gold { get; private set; }

        private void Awake()
        {
            _inventory = new InventoryModel(_columns, _rows);
            Gold = Mathf.Max(0, _startingGold);
        }

        public bool TryAddItem(ItemData item, int quantity)
        {
            if (_inventory == null || item == null || quantity <= 0)
            {
                return false;
            }

            if (!_inventory.TryAddItem(item, quantity))
            {
                return false;
            }

            ItemAdded?.Invoke(item, quantity);
            return true;
        }

        public bool TryCollect(InventoryPickup pickup)
        {
            if (pickup == null || _inventory == null)
            {
                return false;
            }

            ItemData item = pickup.Item;
            int quantity = pickup.Quantity;
            if (!pickup.TryCollect(_inventory))
            {
                return false;
            }

            if (item != null && quantity > 0)
            {
                ItemAdded?.Invoke(item, quantity);
            }

            return true;
        }

        public int GetItemCount(ItemData item)
        {
            return _inventory != null && item != null ? _inventory.GetItemCount(item) : 0;
        }

        public void AddGold(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            Gold += amount;
            GoldChanged?.Invoke(Gold);
        }
    }
}
