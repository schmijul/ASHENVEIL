namespace Ashenveil.Core
{
    /// <summary>
    /// High-level item category used by inventory, trade, and quest systems.
    /// </summary>
    public enum ItemCategory
    {
        /// <summary>
        /// Crafting or trade material.
        /// </summary>
        Material,

        /// <summary>
        /// Consumable food item.
        /// </summary>
        Food,

        /// <summary>
        /// Weapon equipment.
        /// </summary>
        Weapon,

        /// <summary>
        /// Tool equipment or usable tool.
        /// </summary>
        Tool,

        /// <summary>
        /// Quest-critical item.
        /// </summary>
        QuestItem
    }
}
