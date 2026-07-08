using UnityEngine;

namespace Ashenveil.Core
{
    /// <summary>
    /// Shared item data used by loot, inventory, trade, and quest systems.
    /// Referenced GDD section: Kernsysteme / Inventar &amp; Handel.
    /// </summary>
    [CreateAssetMenu(fileName = "NewItemDefinition", menuName = "Ashenveil/Core/Item Definition")]
    public sealed class ItemDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string _id = "item_id";
        [SerializeField] private string _displayName = "Unbenannter Gegenstand";
        [TextArea]
        [SerializeField] private string _description = "Keine Beschreibung.";
        [SerializeField] private ItemCategory _category = ItemCategory.Material;
        [SerializeField] private Sprite _icon;

        [Header("Trade")]
        [SerializeField] private float _weight = 0.1f;
        [SerializeField] private int _goldValue = 1;

        [Header("Stacking")]
        [SerializeField] private bool _stackable = true;
        [SerializeField] private int _maxStack = 20;

        /// <summary>
        /// Stable item identifier used by saves, quests, and content wiring.
        /// </summary>
        public string Id => _id;

        /// <summary>
        /// German player-facing item name.
        /// </summary>
        public string DisplayName => _displayName;

        /// <summary>
        /// German player-facing item description.
        /// </summary>
        public string Description => _description;

        /// <summary>
        /// Inventory and trade category.
        /// </summary>
        public ItemCategory Category => _category;

        /// <summary>
        /// Icon shown in inventory-style UI.
        /// </summary>
        public Sprite Icon => _icon;

        /// <summary>
        /// Weight contribution per item.
        /// </summary>
        public float Weight => _weight;

        /// <summary>
        /// Base gold value per item.
        /// </summary>
        public int GoldValue => _goldValue;

        /// <summary>
        /// Indicates whether multiple copies can share one inventory slot.
        /// </summary>
        public bool Stackable => _stackable;

        /// <summary>
        /// Maximum amount in one inventory stack.
        /// </summary>
        public int MaxStack => _stackable ? Mathf.Max(1, _maxStack) : 1;
    }
}
