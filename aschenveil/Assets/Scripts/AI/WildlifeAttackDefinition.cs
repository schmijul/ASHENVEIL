using Ashenveil.Core;
using UnityEngine;

namespace Ashenveil.AI
{
    /// <summary>
    /// Serialized melee attack tunables for aggressive wildlife.
    /// Referenced GDD section: Kernsysteme / Wildlife.
    /// </summary>
    [System.Serializable]
    public struct WildlifeAttackDefinition
    {
        [SerializeField] private float _damage;
        [SerializeField] private DamageType _damageType;
        [SerializeField] private float _range;
        [SerializeField] private float _cooldown;
        [SerializeField] private float _knockback;

        /// <summary>
        /// Damage amount dealt by this attack.
        /// </summary>
        public float Damage => Mathf.Max(0f, _damage);

        /// <summary>
        /// Damage type dealt by this attack.
        /// </summary>
        public DamageType DamageType => _damageType;

        /// <summary>
        /// Attack range in meters.
        /// </summary>
        public float Range => Mathf.Max(0.1f, _range);

        /// <summary>
        /// Seconds between attacks.
        /// </summary>
        public float Cooldown => Mathf.Max(0.01f, _cooldown);

        /// <summary>
        /// Requested knockback strength.
        /// </summary>
        public float Knockback => Mathf.Max(0f, _knockback);

        /// <summary>
        /// Sensible default attack for aggressive wildlife.
        /// </summary>
        public static WildlifeAttackDefinition Default => new WildlifeAttackDefinition
        {
            _damage = 12f,
            _damageType = DamageType.Physical,
            _range = 1.3f,
            _cooldown = 1.4f,
            _knockback = 2f
        };
    }
}
