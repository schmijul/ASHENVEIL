using System;
using System.Collections.Generic;
using Ashenveil.Core;
using Ashenveil.Inventory;

namespace Ashenveil.Trade
{
    /// <summary>
    /// Deterministic trade session model for buying from and selling to one vendor.
    /// Referenced GDD section: Kernsysteme / Inventar &amp; Handel.
    /// </summary>
    public sealed class TradeModel
    {
        private readonly InventoryModel _playerInventory;
        private readonly List<StockState> _stock = new List<StockState>();
        private readonly float _buybackMultiplier;

        private int _vendorGold;

        public TradeModel(VendorDefinition vendorDefinition, InventoryModel playerInventory)
        {
            if (vendorDefinition == null)
            {
                throw new ArgumentNullException(nameof(vendorDefinition));
            }

            _playerInventory = playerInventory ?? throw new ArgumentNullException(nameof(playerInventory));
            VendorName = vendorDefinition.VendorName;
            _vendorGold = vendorDefinition.GoldReserve;
            _buybackMultiplier = vendorDefinition.BuybackMultiplier;

            IReadOnlyList<VendorDefinition.StockEntry> stock = vendorDefinition.Stock;
            for (int i = 0; i < stock.Count; i++)
            {
                VendorDefinition.StockEntry entry = stock[i];
                if (entry.Item == null || entry.Quantity <= 0)
                {
                    continue;
                }

                _stock.Add(new StockState(entry.Item, entry.Quantity, entry.PriceMultiplier));
            }
        }

        public string VendorName { get; }

        public int VendorGold => _vendorGold;

        public int QuoteBuyPrice(ItemDefinition item, int quantity)
        {
            if (item == null || quantity <= 0)
            {
                return 0;
            }

            StockState stock = FindStock(item);
            if (stock == null || stock.Quantity < quantity)
            {
                return 0;
            }

            return ScalePrice(item.GoldValue, quantity, stock.PriceMultiplier, true);
        }

        public int QuoteSellPrice(ItemDefinition item, int quantity)
        {
            if (item == null || quantity <= 0)
            {
                return 0;
            }

            return ScalePrice(item.GoldValue, quantity, _buybackMultiplier, false);
        }

        public int GetStockQuantity(ItemDefinition item)
        {
            StockState stock = FindStock(item);
            return stock == null ? 0 : stock.Quantity;
        }

        public bool CanBuy(ItemDefinition item, int quantity)
        {
            int price = QuoteBuyPrice(item, quantity);
            return item != null
                && quantity > 0
                && price >= 0
                && GetStockQuantity(item) >= quantity
                && _playerInventory.Gold >= price
                && _playerInventory.CanAdd(item, quantity);
        }

        public bool CanSell(ItemDefinition item, int quantity)
        {
            int price = QuoteSellPrice(item, quantity);
            return item != null
                && quantity > 0
                && price >= 0
                && _playerInventory.GetQuantity(item) >= quantity
                && _vendorGold >= price;
        }

        public bool ExecuteBuy(ItemDefinition item, int quantity)
        {
            if (!CanBuy(item, quantity))
            {
                return false;
            }

            int price = QuoteBuyPrice(item, quantity);
            StockState stock = FindStock(item);
            if (stock == null || !_playerInventory.SpendGold(price))
            {
                return false;
            }

            if (!_playerInventory.Add(item, quantity))
            {
                _playerInventory.EarnGold(price);
                return false;
            }

            stock.Quantity -= quantity;
            _vendorGold += price;
            GameSignals.RaiseVendorTransaction(item, quantity, -price, true);
            return true;
        }

        public bool ExecuteSell(ItemDefinition item, int quantity)
        {
            if (!CanSell(item, quantity))
            {
                return false;
            }

            int price = QuoteSellPrice(item, quantity);
            if (!_playerInventory.Remove(item, quantity))
            {
                return false;
            }

            _vendorGold -= price;
            if (!_playerInventory.EarnGold(price))
            {
                _vendorGold += price;
                _playerInventory.Add(item, quantity);
                return false;
            }

            AddStock(item, quantity);
            GameSignals.RaiseVendorTransaction(item, quantity, price, false);
            return true;
        }

        private static int ScalePrice(int baseGoldValue, int quantity, float multiplier, bool roundUp)
        {
            if (baseGoldValue <= 0 || quantity <= 0 || multiplier <= 0f)
            {
                return 0;
            }

            double rawPrice = baseGoldValue * quantity * multiplier;
            int scaled = roundUp ? (int)Math.Ceiling(rawPrice) : (int)Math.Floor(rawPrice);
            return Math.Max(1, scaled);
        }

        private StockState FindStock(ItemDefinition item)
        {
            for (int i = 0; i < _stock.Count; i++)
            {
                if (_stock[i].Item == item)
                {
                    return _stock[i];
                }
            }

            return null;
        }

        private void AddStock(ItemDefinition item, int quantity)
        {
            StockState stock = FindStock(item);
            if (stock == null)
            {
                _stock.Add(new StockState(item, quantity, 1f));
                return;
            }

            stock.Quantity += quantity;
        }

        private sealed class StockState
        {
            public StockState(ItemDefinition item, int quantity, float priceMultiplier)
            {
                Item = item;
                Quantity = quantity;
                PriceMultiplier = priceMultiplier;
            }

            public ItemDefinition Item { get; }

            public int Quantity { get; set; }

            public float PriceMultiplier { get; }
        }
    }
}
