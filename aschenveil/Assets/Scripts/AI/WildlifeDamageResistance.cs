using System;
using UnityEngine;
using Ashenveil.Combat;

namespace Ashenveil.AI
{
    /// <summary>
    /// Damage type multiplier used by wildlife health to model resistances.
    /// </summary>
    [Serializable]
    public sealed class WildlifeDamageResistance
    {
        public DamageType DamageType = DamageType.Physical;

        [Range(0f, 2f)]
        public float Multiplier = 1f;

        public WildlifeDamageResistance Clone()
        {
            return new WildlifeDamageResistance
            {
                DamageType = DamageType,
                Multiplier = Multiplier
            };
        }
    }
}
