using System;
using UnityEngine;

namespace Ashenveil.Combat
{
    /// <summary>
    /// Serializable blocking timings and mitigation values.
    /// </summary>
    [Serializable]
    public sealed class BlockSettings
    {
        [Header("Mitigation")]
        [Range(0f, 1f)] public float DamageReduction = 0.7f;

        [Header("Timing")]
        [Min(0f)] public float PerfectBlockWindow = 0.2f;
        [Min(0f)] public float PerfectBlockStaggerDuration = 1f;
        [Min(0f)] public float BlockBreakStaggerDuration = 1.5f;

        [Header("Resource")]
        [Min(0f)] public float StaminaDrainPerHit = 5f;

        public static BlockSettings CreateDefault()
        {
            return new BlockSettings();
        }

        public BlockSettings Clone()
        {
            return new BlockSettings
            {
                DamageReduction = DamageReduction,
                PerfectBlockWindow = PerfectBlockWindow,
                PerfectBlockStaggerDuration = PerfectBlockStaggerDuration,
                BlockBreakStaggerDuration = BlockBreakStaggerDuration,
                StaminaDrainPerHit = StaminaDrainPerHit
            };
        }
    }
}
