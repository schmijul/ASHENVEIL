using System.Reflection;
using Ashenveil.Core;
using Ashenveil.Inventory;
using NUnit.Framework;
using UnityEngine;

namespace Ashenveil.Tests.EditMode
{
    /// <summary>
    /// EditMode tests for inventory stacking, weight, and gold rules.
    /// </summary>
    public sealed class InventoryModelTests
    {
        [Test]
        public void Add_StackableItem_SplitsByStackLimit()
        {
            ItemDefinition meat = CreateItem("meat", "Fleisch", 0.5f, 3, true, 5);
            InventoryModel model = new InventoryModel(20f);

            bool added = model.Add(meat, 12);

            Assert.That(added, Is.True);
            Assert.That(model.GetQuantity(meat), Is.EqualTo(12));
            Assert.That(model.Stacks.Count, Is.EqualTo(3));
            Assert.That(model.Stacks[0].Quantity, Is.EqualTo(5));
            Assert.That(model.Stacks[1].Quantity, Is.EqualTo(5));
            Assert.That(model.Stacks[2].Quantity, Is.EqualTo(2));
        }

        [Test]
        public void Add_ExceedsCarryWeight_RejectsWithoutChangingInventory()
        {
            ItemDefinition pelt = CreateItem("pelt", "Fell", 2f, 8, true, 10);
            InventoryModel model = new InventoryModel(5f);

            Assert.That(model.Add(pelt, 2), Is.True);
            Assert.That(model.Add(pelt, 1), Is.False);

            Assert.That(model.GetQuantity(pelt), Is.EqualTo(2));
            Assert.That(model.TotalWeight, Is.EqualTo(4f).Within(0.001f));
        }

        [Test]
        public void SpendAndEarnGold_GuardInvalidAmounts()
        {
            InventoryModel model = new InventoryModel(10f, 10);

            Assert.That(model.SpendGold(4), Is.True);
            Assert.That(model.Gold, Is.EqualTo(6));
            Assert.That(model.SpendGold(7), Is.False);
            Assert.That(model.SpendGold(-1), Is.False);
            Assert.That(model.Gold, Is.EqualTo(6));
            Assert.That(model.EarnGold(5), Is.True);
            Assert.That(model.EarnGold(-1), Is.False);
            Assert.That(model.Gold, Is.EqualTo(11));
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

        private static void SetField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null);
            field.SetValue(target, value);
        }
    }
}
