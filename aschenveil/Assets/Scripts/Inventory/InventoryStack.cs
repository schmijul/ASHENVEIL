using Ashenveil.Core;

namespace Ashenveil.Inventory
{
    /// <summary>
    /// Immutable inventory stack data for a single item definition.
    /// Referenced GDD section: Kernsysteme / Inventar &amp; Handel.
    /// </summary>
    public readonly struct InventoryStack
    {
        public InventoryStack(ItemDefinition item, int quantity)
        {
            Item = item;
            Quantity = quantity;
        }

        public ItemDefinition Item { get; }

        public int Quantity { get; }
    }
}
