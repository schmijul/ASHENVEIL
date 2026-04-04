using System.Collections.Generic;
using Ashenveil.Data;
using Ashenveil.Inventory;
using NUnit.Framework;
using UnityEngine;

namespace Ashenveil.Tests.EditMode
{
    public class InventorySystemTests
    {
        private readonly List<Object> _createdObjects = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (Object createdObject in _createdObjects)
            {
                if (createdObject != null)
                {
                    Object.DestroyImmediate(createdObject);
                }
            }

            _createdObjects.Clear();
        }

        [Test]
        public void AddItem_StackableItemSpillsIntoAdditionalSlotWhenStackFull()
        {
            InventoryModel inventory = new InventoryModel();
            ItemData herb = CreateItem("Herb", ItemType.Consumable, 10);

            Assert.That(inventory.AddItem(herb, 14), Is.EqualTo(14));
            Assert.That(inventory.Grid.GetItemCount(herb), Is.EqualTo(14));
            Assert.That(inventory.Grid.GetSlot(0).Quantity, Is.EqualTo(10));
            Assert.That(inventory.Grid.GetSlot(1).Quantity, Is.EqualTo(4));
        }

        [Test]
        public void RemoveItem_ConsumesAcrossMultipleStacksAndClearsEmptySlots()
        {
            InventoryModel inventory = new InventoryModel();
            ItemData herb = CreateItem("Herb", ItemType.Consumable, 10);

            inventory.AddItem(herb, 14);

            Assert.That(inventory.RemoveItem(herb, 12), Is.EqualTo(12));
            Assert.That(inventory.Grid.GetItemCount(herb), Is.EqualTo(2));
            Assert.That(inventory.Grid.GetSlot(0).IsEmpty, Is.True);
            Assert.That(inventory.Grid.GetSlot(1).Quantity, Is.EqualTo(2));
        }

        [Test]
        public void TryEquipItem_WhenSlotOccupiedAndSpaceExists_SwapsEquipment()
        {
            InventoryModel inventory = new InventoryModel();
            ItemData swordA = CreateItem("Sword A", ItemType.Weapon, 1, EquipSlot.MainHand);
            ItemData swordB = CreateItem("Sword B", ItemType.Weapon, 1, EquipSlot.MainHand);

            inventory.AddItem(swordA, 1);
            inventory.AddItem(swordB, 1);

            Assert.That(inventory.TryEquip(swordA), Is.True);
            Assert.That(inventory.TryEquip(swordB), Is.True);
            Assert.That(inventory.Equipment.GetEquippedItem(EquipSlot.MainHand), Is.SameAs(swordB));
            Assert.That(inventory.Grid.GetItemCount(swordA), Is.EqualTo(1));
            Assert.That(inventory.Grid.GetItemCount(swordB), Is.EqualTo(0));
        }

        [Test]
        public void TryUnequipItem_WhenInventoryHasSpace_ReturnsEquipmentToInventory()
        {
            InventoryModel inventory = new InventoryModel();
            ItemData sword = CreateItem("Sword", ItemType.Weapon, 1, EquipSlot.MainHand);

            inventory.AddItem(sword, 1);

            Assert.That(inventory.TryEquip(sword), Is.True);
            Assert.That(inventory.TryUnequip(EquipSlot.MainHand), Is.True);
            Assert.That(inventory.Equipment.GetEquippedItem(EquipSlot.MainHand), Is.Null);
            Assert.That(inventory.Grid.GetItemCount(sword), Is.EqualTo(1));
        }

        [Test]
        public void TryAddItem_WhenInventoryIsFull_ReturnsFalseForAdditionalItem()
        {
            InventoryModel inventory = new InventoryModel();
            ItemData trinket = CreateItem("Trinket", ItemType.Quest, 1);

            Assert.That(inventory.TryAddItem(trinket, 24), Is.True);
            Assert.That(inventory.TryAddItem(trinket, 1), Is.False);
            Assert.That(inventory.AddItem(trinket, 1), Is.EqualTo(0));
        }

        private ItemData CreateItem(string itemName, ItemType itemType, int maxStack, EquipSlot equipSlot = EquipSlot.None)
        {
            ItemData item = ScriptableObject.CreateInstance<ItemData>();
            item.itemName = itemName;
            item.type = itemType;
            item.maxStack = maxStack;
            item.equipSlot = equipSlot;
            item.buyPrice = 10;
            item.sellPrice = 5;
            _createdObjects.Add(item);
            return item;
        }
    }
}
