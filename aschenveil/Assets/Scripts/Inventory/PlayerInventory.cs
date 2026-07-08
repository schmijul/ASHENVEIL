using System;
using Ashenveil.Core;
using UnityEngine;

namespace Ashenveil.Inventory
{
    /// <summary>
    /// Thin player-facing adapter that owns the runtime inventory model.
    /// Referenced GDD section: Kernsysteme / Inventar &amp; Handel.
    /// </summary>
    public sealed class PlayerInventory : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private float _maxCarryWeight = 40f;
        [SerializeField] private int _initialGold = 25;
        [SerializeField] private LoadoutEntry[] _initialLoadout = Array.Empty<LoadoutEntry>();

        private InventoryModel _model;

        public InventoryModel Model
        {
            get
            {
                EnsureInitialized();
                return _model;
            }
        }

        private void Awake()
        {
            EnsureInitialized();
        }

        private void EnsureInitialized()
        {
            if (_model != null)
            {
                return;
            }

            _model = new InventoryModel(_maxCarryWeight, _initialGold);
            for (int i = 0; i < _initialLoadout.Length; i++)
            {
                LoadoutEntry entry = _initialLoadout[i];
                if (entry.Item == null || entry.Quantity <= 0)
                {
                    continue;
                }

                if (!_model.Add(entry.Item, entry.Quantity))
                {
                    Debug.LogWarning($"Initial inventory could not add {entry.Quantity}x {entry.Item.DisplayName}.", this);
                }
            }
        }

        [Serializable]
        private struct LoadoutEntry
        {
            [SerializeField] private ItemDefinition _item;
            [SerializeField] private int _quantity;

            public ItemDefinition Item => _item;

            public int Quantity => _quantity;
        }
    }
}
