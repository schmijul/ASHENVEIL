using System;
using Ashenveil.Core;
using UnityEngine;

namespace Ashenveil.Inventory
{
    /// <summary>
    /// World pickup interactable that transfers configured loot into the player inventory.
    /// Referenced GDD section: Kernsysteme / Inventar &amp; Handel.
    /// </summary>
    public sealed class ItemPickup : MonoBehaviour, IInteractable
    {
        [Header("Configuration")]
        [SerializeField] private PickupEntry[] _items = Array.Empty<PickupEntry>();
        [SerializeField] private bool _disableAfterPickup = true;

        [Header("References")]
        [SerializeField] private PlayerInventory _playerInventory;

        public string InteractionPrompt => "Aufheben (E)";

        public void Interact(PlayerContext playerContext)
        {
            PlayerInventory inventory = ResolveInventory(playerContext);
            if (inventory == null)
            {
                Debug.LogError("ItemPickup needs a PlayerInventory reference or a PlayerInventory on the interacting player.", this);
                return;
            }

            InventoryModel model = inventory.Model;
            if (!CanGrantAll(model))
            {
                return;
            }

            for (int i = 0; i < _items.Length; i++)
            {
                PickupEntry entry = _items[i];
                if (entry.Item == null || entry.Quantity <= 0)
                {
                    continue;
                }

                model.Add(entry.Item, entry.Quantity);
                GameSignals.RaiseItemLooted(entry.Item, entry.Quantity);
            }

            if (_disableAfterPickup)
            {
                gameObject.SetActive(false);
            }
        }

        private PlayerInventory ResolveInventory(PlayerContext playerContext)
        {
            if (_playerInventory != null)
            {
                return _playerInventory;
            }

            if (playerContext?.PlayerTransform != null
                && playerContext.PlayerTransform.TryGetComponent(out PlayerInventory inventory))
            {
                return inventory;
            }

            return null;
        }

        private bool CanGrantAll(InventoryModel model)
        {
            float addedWeight = 0f;
            bool hasItem = false;

            for (int i = 0; i < _items.Length; i++)
            {
                PickupEntry entry = _items[i];
                if (entry.Item == null || entry.Quantity <= 0)
                {
                    continue;
                }

                hasItem = true;
                addedWeight += entry.Item.Weight * entry.Quantity;
            }

            return hasItem && model.TotalWeight + addedWeight <= model.MaxCarryWeight + 0.0001f;
        }

        [Serializable]
        private struct PickupEntry
        {
            [SerializeField] private ItemDefinition _item;
            [SerializeField] private int _quantity;

            public ItemDefinition Item => _item;

            public int Quantity => _quantity;
        }
    }
}
