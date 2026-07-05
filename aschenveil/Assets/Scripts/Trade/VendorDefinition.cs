using System;
using System.Collections.Generic;
using Ashenveil.Core;
using UnityEngine;

namespace Ashenveil.Trade
{
    /// <summary>
    /// ScriptableObject configuration for village vendors and their trade stock.
    /// Referenced GDD section: Kernsysteme / Inventar &amp; Handel.
    /// </summary>
    [CreateAssetMenu(fileName = "NewVendorDefinition", menuName = "Ashenveil/Trade/Vendor Definition")]
    public sealed class VendorDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string _vendorName = "Händler";

        [Header("Economy")]
        [SerializeField] private int _goldReserve = 50;
        [SerializeField] private float _buybackMultiplier = 0.5f;
        [SerializeField] private StockEntry[] _stock = Array.Empty<StockEntry>();

        public string VendorName => _vendorName;

        public int GoldReserve => Math.Max(0, _goldReserve);

        public float BuybackMultiplier => Mathf.Max(0f, _buybackMultiplier);

        public IReadOnlyList<StockEntry> Stock => _stock;

        [Serializable]
        public sealed class StockEntry
        {
            [SerializeField] private ItemDefinition _item;
            [SerializeField] private int _quantity = 1;
            [SerializeField] private float _priceMultiplier = 1f;

            public StockEntry()
            {
            }

            public StockEntry(ItemDefinition item, int quantity, float priceMultiplier)
            {
                _item = item;
                _quantity = quantity;
                _priceMultiplier = priceMultiplier;
            }

            public ItemDefinition Item => _item;

            public int Quantity => Math.Max(0, _quantity);

            public float PriceMultiplier => Mathf.Max(0f, _priceMultiplier);
        }
    }
}
