using System.Collections.Generic;
using UnityEngine;

namespace Ashenveil.AI
{
    /// <summary>
    /// Deterministic loot rolling helper shared by wildlife agents and tests.
    /// Referenced GDD section: Kernsysteme / Wildlife.
    /// </summary>
    public static class WildlifeLootRoller
    {
        /// <summary>
        /// Rolls serialized loot entries with the provided random source.
        /// </summary>
        public static List<LootStack> Roll(IReadOnlyList<WildlifeLootEntry> entries, IRandomSource randomSource)
        {
            List<LootStack> loot = new List<LootStack>();
            if (entries == null || randomSource == null)
            {
                return loot;
            }

            for (int i = 0; i < entries.Count; i++)
            {
                WildlifeLootEntry entry = entries[i];
                if (entry.Item == null || entry.Chance <= 0f || randomSource.Next01() > entry.Chance)
                {
                    continue;
                }

                int amount = randomSource.RangeInclusive(entry.Min, entry.Max);
                if (amount > 0)
                {
                    loot.Add(new LootStack(entry.Item, amount));
                }
            }

            return loot;
        }

        /// <summary>
        /// Rolls a pure amount for tests or simple callers.
        /// </summary>
        public static int RollAmount(int min, int max, float chance, IRandomSource randomSource)
        {
            if (randomSource == null || Mathf.Clamp01(chance) <= 0f || randomSource.Next01() > Mathf.Clamp01(chance))
            {
                return 0;
            }

            min = Mathf.Max(0, min);
            max = Mathf.Max(min, max);
            return randomSource.RangeInclusive(min, max);
        }
    }
}
