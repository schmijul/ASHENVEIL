using System;
using UnityEngine;

namespace Ashenveil.AI
{
    /// <summary>
    /// Serializable loot entry for wildlife death drops.
    /// </summary>
    [Serializable]
    public sealed class WildlifeLootDrop
    {
        [Header("Identity")]
        public string ItemId = string.Empty;

        [Header("Chance")]
        [Range(0f, 1f)] public float DropChance = 1f;

        [Header("Amount")]
        [Min(0)] public int MinAmount = 1;
        [Min(0)] public int MaxAmount = 1;

        public WildlifeLootDrop Clone()
        {
            return new WildlifeLootDrop
            {
                ItemId = ItemId,
                DropChance = DropChance,
                MinAmount = MinAmount,
                MaxAmount = MaxAmount
            };
        }
    }

    /// <summary>
    /// Resolved loot result from a wildlife death roll.
    /// </summary>
    public readonly struct WildlifeLootResult
    {
        public WildlifeLootResult(string itemId, int amount)
        {
            ItemId = itemId;
            Amount = amount;
        }

        public string ItemId { get; }

        public int Amount { get; }
    }
}
