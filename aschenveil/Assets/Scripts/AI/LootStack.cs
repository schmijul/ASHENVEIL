using Ashenveil.Core;

namespace Ashenveil.AI
{
    /// <summary>
    /// Runtime stack of rolled loot.
    /// Referenced GDD section: Kernsysteme / Wildlife.
    /// </summary>
    public readonly struct LootStack
    {
        /// <summary>
        /// Creates one rolled loot stack.
        /// </summary>
        public LootStack(ItemDefinition item, int amount)
        {
            Item = item;
            Amount = amount;
        }

        /// <summary>
        /// Looted item definition.
        /// </summary>
        public ItemDefinition Item { get; }

        /// <summary>
        /// Looted item amount.
        /// </summary>
        public int Amount { get; }
    }
}
