using System;
using Ashenveil.Combat;
using UnityEngine;

namespace Ashenveil.Data
{
    [CreateAssetMenu(fileName = "NewItem", menuName = "Ashenveil/Items/Item")]
    public class ItemData : ScriptableObject
    {
        [Header("Base Properties")]
        public string itemName;
        [TextArea]
        public string description;
        public Sprite icon;

        [Header("Item Properties")]
        public ItemType type = ItemType.Material;
        public int maxStack = 1;
        public int buyPrice;
        public int sellPrice;
        public EquipSlot equipSlot = EquipSlot.None;

        [Header("References")]
        public WeaponData weaponData;
        public ArmorData armorData;
        public ConsumableEffect consumableEffect;

        public string DisplayName => string.IsNullOrWhiteSpace(itemName) ? name : itemName;

        public bool IsEquippable => equipSlot != EquipSlot.None;

        public bool IsStackable => GetEffectiveMaxStack() > 1;

        public int GetEffectiveMaxStack()
        {
            if (IsEquippable || type == ItemType.Weapon || type == ItemType.Armor)
            {
                return 1;
            }

            if (maxStack > 0)
            {
                return Mathf.Max(1, maxStack);
            }

            return type == ItemType.Consumable || type == ItemType.Material ? 10 : 1;
        }
    }

    [CreateAssetMenu(fileName = "NewArmor", menuName = "Ashenveil/Items/Armor")]
    public class ArmorData : ScriptableObject
    {
        [Header("Base Properties")]
        public string armorName;
        [TextArea]
        public string description;

        [Header("Armor Properties")]
        public float armorRating = 1f;
        public float staminaPenalty;
    }

    [CreateAssetMenu(fileName = "NewConsumableEffect", menuName = "Ashenveil/Items/Consumable Effect")]
    public class ConsumableEffect : ScriptableObject
    {
        [Header("Base Properties")]
        public string effectName;
        [TextArea]
        public string description;

        [Header("Effect Properties")]
        public ConsumableEffectType type = ConsumableEffectType.HealHP;
        public float value = 10f;
        public float duration;
    }
}
