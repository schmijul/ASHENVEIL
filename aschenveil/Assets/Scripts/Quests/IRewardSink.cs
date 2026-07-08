using Ashenveil.Core;

namespace Ashenveil.Quests
{
    /// <summary>
    /// Minimal reward delivery surface for quest completion.
    /// Referenced GDD section: Kernsysteme / Inventar &amp; Handel.
    /// </summary>
    public interface IRewardSink
    {
        void GrantGold(int amount);

        void GrantItem(ItemDefinition item, int amount);
    }
}
