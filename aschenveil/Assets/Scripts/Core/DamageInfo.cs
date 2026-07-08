using UnityEngine;

namespace Ashenveil.Core
{
    /// <summary>
    /// Immutable damage payload passed to objects that can receive damage.
    /// Referenced GDD section: Kernsysteme / Combat.
    /// </summary>
    public readonly struct DamageInfo
    {
        /// <summary>
        /// Creates a damage payload.
        /// </summary>
        public DamageInfo(float amount, DamageType damageType, Vector3 sourcePosition, float knockback)
        {
            Amount = amount;
            DamageType = damageType;
            SourcePosition = sourcePosition;
            Knockback = knockback;
        }

        /// <summary>
        /// Raw damage amount before target-specific mitigation.
        /// </summary>
        public float Amount { get; }

        /// <summary>
        /// Damage channel used by resistances or special boss gates.
        /// </summary>
        public DamageType DamageType { get; }

        /// <summary>
        /// World position the damage came from.
        /// </summary>
        public Vector3 SourcePosition { get; }

        /// <summary>
        /// Requested knockback impulse strength.
        /// </summary>
        public float Knockback { get; }
    }
}
