using System.Collections.Generic;
using UnityEngine;

namespace Ashenveil.AI
{
    /// <summary>
    /// Pure loot resolution logic for wildlife deaths.
    /// </summary>
    public sealed class WildlifeLootTable
    {
        public static List<WildlifeLootResult> ResolveDrops(WildlifeLootDrop[] lootDrops, IWildlifeRandomSource randomSource)
        {
            List<WildlifeLootResult> results = new List<WildlifeLootResult>();
            if (lootDrops == null || lootDrops.Length == 0)
            {
                return results;
            }

            IWildlifeRandomSource source = randomSource ?? new UnityWildlifeRandomSource();
            for (int index = 0; index < lootDrops.Length; index++)
            {
                WildlifeLootDrop lootDrop = lootDrops[index];
                if (lootDrop == null || string.IsNullOrWhiteSpace(lootDrop.ItemId))
                {
                    continue;
                }

                float chance = Mathf.Clamp01(lootDrop.DropChance);
                if (chance <= 0f)
                {
                    continue;
                }

                if (chance < 1f && source.Value() > chance)
                {
                    continue;
                }

                int minAmount = Mathf.Max(0, lootDrop.MinAmount);
                int maxAmount = Mathf.Max(minAmount, lootDrop.MaxAmount);
                int amount = minAmount == maxAmount ? minAmount : source.Range(minAmount, maxAmount + 1);
                results.Add(new WildlifeLootResult(lootDrop.ItemId, amount));
            }

            return results;
        }
    }
}
