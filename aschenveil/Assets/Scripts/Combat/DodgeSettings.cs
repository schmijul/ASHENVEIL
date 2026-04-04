using System;
using UnityEngine;

namespace Ashenveil.Combat
{
    /// <summary>
    /// Serializable dodge roll timings and stamina cost.
    /// </summary>
    [Serializable]
    public sealed class DodgeSettings
    {
        [Header("Movement")]
        [Min(0f)] public float Distance = 3f;
        [Min(0f)] public float Duration = 0.5f;

        [Header("Invulnerability")]
        [Min(0f)] public float IFrameDuration = 0.3f;

        [Header("Recovery")]
        [Min(0f)] public float Cooldown = 0.2f;
        [Min(0f)] public float StaminaCost = 25f;

        public static DodgeSettings CreateDefault()
        {
            return new DodgeSettings();
        }

        public DodgeSettings Clone()
        {
            return new DodgeSettings
            {
                Distance = Distance,
                Duration = Duration,
                IFrameDuration = IFrameDuration,
                Cooldown = Cooldown,
                StaminaCost = StaminaCost
            };
        }
    }
}
