using System.Reflection;
using Ashenveil.Core;
using Ashenveil.Inventory;
using Ashenveil.Trade;
using NUnit.Framework;
using UnityEngine;

namespace Ashenveil.Tests.EditMode
{
    /// <summary>
    /// EditMode tests for vendor trade pricing and transaction guards.
    /// </summary>
    public sealed class TradeModelTests
    {
        [Test]
        public void QuotePrices_UseVendorAndBuybackMultipliers()
        {
            ItemDefinition herb = CreateItem("herb", "Kraut", 0.1f, 10, true, 20);
            VendorDefinition vendor = CreateVendor(
                100,
                0.5f,
                new VendorDefinition.StockEntry(herb, 5, 1.25f));
            InventoryModel player = new InventoryModel(20f, 100);
            TradeModel trade = new TradeModel(vendor, player);

            Assert.That(trade.QuoteBuyPrice(herb, 2), Is.EqualTo(25));
            Assert.That(trade.QuoteSellPrice(herb, 2), Is.EqualTo(10));
        }

        [Test]
        public void ExecuteBuy_InsufficientPlayerGold_FailsWithoutMutation()
        {
            ItemDefinition axe = CreateItem("axe", "Axt", 2f, 20, false, 1);
            VendorDefinition vendor = CreateVendor(
                30,
                0.5f,
                new VendorDefinition.StockEntry(axe, 2, 1f));
            InventoryModel player = new InventoryModel(20f, 5);
            TradeModel trade = new TradeModel(vendor, player);

            bool bought = trade.ExecuteBuy(axe, 1);

            Assert.That(bought, Is.False);
            Assert.That(player.Gold, Is.EqualTo(5));
            Assert.That(player.GetQuantity(axe), Is.EqualTo(0));
            Assert.That(trade.GetStockQuantity(axe), Is.EqualTo(2));
            Assert.That(trade.VendorGold, Is.EqualTo(30));
        }

        [Test]
        public void ExecuteSell_VendorCannotPay_FailsWithoutMutation()
        {
            ItemDefinition pelt = CreateItem("pelt", "Fell", 1f, 10, true, 10);
            VendorDefinition vendor = CreateVendor(
                5,
                1f,
                new VendorDefinition.StockEntry(pelt, 1, 1f));
            InventoryModel player = new InventoryModel(20f, 3);
            player.Add(pelt, 2);
            TradeModel trade = new TradeModel(vendor, player);

            bool sold = trade.ExecuteSell(pelt, 1);

            Assert.That(sold, Is.False);
            Assert.That(player.Gold, Is.EqualTo(3));
            Assert.That(player.GetQuantity(pelt), Is.EqualTo(2));
            Assert.That(trade.GetStockQuantity(pelt), Is.EqualTo(1));
            Assert.That(trade.VendorGold, Is.EqualTo(5));
        }

        [Test]
        public void ExecuteBuy_Succeeds_DecrementsStockAndTransfersGold()
        {
            ItemDefinition bread = CreateItem("bread", "Brot", 0.2f, 10, true, 5);
            VendorDefinition vendor = CreateVendor(
                10,
                0.5f,
                new VendorDefinition.StockEntry(bread, 3, 1f));
            InventoryModel player = new InventoryModel(20f, 100);
            TradeModel trade = new TradeModel(vendor, player);

            bool bought = trade.ExecuteBuy(bread, 2);

            Assert.That(bought, Is.True);
            Assert.That(player.Gold, Is.EqualTo(80));
            Assert.That(player.GetQuantity(bread), Is.EqualTo(2));
            Assert.That(trade.GetStockQuantity(bread), Is.EqualTo(1));
            Assert.That(trade.VendorGold, Is.EqualTo(30));
        }

        [Test]
        public void ExecuteBuy_ExceedsPlayerCarryWeight_FailsAtomically()
        {
            ItemDefinition ore = CreateItem("ore", "Erz", 2f, 10, true, 10);
            VendorDefinition vendor = CreateVendor(
                15,
                0.5f,
                new VendorDefinition.StockEntry(ore, 4, 1f));
            InventoryModel player = new InventoryModel(1f, 100);
            TradeModel trade = new TradeModel(vendor, player);

            bool bought = trade.ExecuteBuy(ore, 1);

            Assert.That(bought, Is.False);
            Assert.That(player.Gold, Is.EqualTo(100));
            Assert.That(player.GetQuantity(ore), Is.EqualTo(0));
            Assert.That(trade.GetStockQuantity(ore), Is.EqualTo(4));
            Assert.That(trade.VendorGold, Is.EqualTo(15));
        }

        private static ItemDefinition CreateItem(
            string id,
            string displayName,
            float weight,
            int goldValue,
            bool stackable,
            int maxStack)
        {
            ItemDefinition item = ScriptableObject.CreateInstance<ItemDefinition>();
            SetField(item, "_id", id);
            SetField(item, "_displayName", displayName);
            SetField(item, "_weight", weight);
            SetField(item, "_goldValue", goldValue);
            SetField(item, "_stackable", stackable);
            SetField(item, "_maxStack", maxStack);
            return item;
        }

        private static VendorDefinition CreateVendor(
            int goldReserve,
            float buybackMultiplier,
            params VendorDefinition.StockEntry[] stock)
        {
            VendorDefinition vendor = ScriptableObject.CreateInstance<VendorDefinition>();
            SetField(vendor, "_vendorName", "Händler");
            SetField(vendor, "_goldReserve", goldReserve);
            SetField(vendor, "_buybackMultiplier", buybackMultiplier);
            SetField(vendor, "_stock", stock);
            return vendor;
        }

        private static void SetField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null);
            field.SetValue(target, value);
        }
    }
}
