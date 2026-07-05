using Ashenveil.Core;
using UnityEngine;

namespace Ashenveil.Combat
{
    /// <summary>
    /// ScriptableObject containing melee weapon combat tunables.
    /// Referenced GDD section: Kernsysteme / Combat.
    /// </summary>
    [CreateAssetMenu(fileName = "NewWeaponDefinition", menuName = "Ashenveil/Combat/Weapon Definition")]
    public sealed class WeaponDefinition : ScriptableObject
    {
        [Header("Damage")]
        [SerializeField] private float _damage = 18f;
        [SerializeField] private DamageType _damageType = DamageType.Physical;
        [SerializeField] private float _aetherDamageBonus;
        [SerializeField] private float _knockback = 3f;

        [Header("Stamina")]
        [SerializeField] private float _lightStaminaCost = 12f;
        [SerializeField] private float _heavyStaminaCost = 28f;
        [SerializeField] private float _maxCombatStamina = 100f;

        [Header("Light Attack")]
        [SerializeField] private float _lightWindupDuration = 0.12f;
        [SerializeField] private float _lightActiveDuration = 0.16f;
        [SerializeField] private float _lightRecoveryDuration = 0.28f;

        [Header("Heavy Attack")]
        [SerializeField] private float _heavyWindupDuration = 0.24f;
        [SerializeField] private float _heavyActiveDuration = 0.22f;
        [SerializeField] private float _heavyRecoveryDuration = 0.42f;

        [Header("Reach")]
        [SerializeField] private float _range = 1.8f;

        [Header("Defense")]
        [Range(0f, 1f)]
        [SerializeField] private float _blockDamageReduction = 0.55f;
        [SerializeField] private float _blockStaminaDrainPerDamage = 0.45f;
        [SerializeField] private float _comboWindowDuration = 0.22f;

        /// <summary>
        /// Base weapon damage before optional aether bonus.
        /// </summary>
        public float Damage => Mathf.Max(0f, _damage);

        /// <summary>
        /// Damage channel applied by this weapon.
        /// </summary>
        public DamageType DamageType => _damageType;

        /// <summary>
        /// Extra damage applied when this weapon is aether-empowered.
        /// </summary>
        public float AetherDamageBonus => Mathf.Max(0f, _aetherDamageBonus);

        /// <summary>
        /// Knockback strength requested on successful hits.
        /// </summary>
        public float Knockback => Mathf.Max(0f, _knockback);

        /// <summary>
        /// Stamina required for a light attack.
        /// </summary>
        public float LightStaminaCost => Mathf.Max(0f, _lightStaminaCost);

        /// <summary>
        /// Stamina required for a heavy attack.
        /// </summary>
        public float HeavyStaminaCost => Mathf.Max(0f, _heavyStaminaCost);

        /// <summary>
        /// Maximum stamina used by standalone combat simulations.
        /// </summary>
        public float MaxCombatStamina => Mathf.Max(1f, _maxCombatStamina);

        /// <summary>
        /// Maximum weapon reach in meters.
        /// </summary>
        public float Range => Mathf.Max(0.1f, _range);

        /// <summary>
        /// Creates deterministic settings for the pure combat model.
        /// </summary>
        public CombatModel.Settings ToSettings()
        {
            return new CombatModel.Settings
            {
                Damage = Damage,
                DamageType = DamageType,
                AetherDamageBonus = AetherDamageBonus,
                Knockback = Knockback,
                LightStaminaCost = LightStaminaCost,
                HeavyStaminaCost = HeavyStaminaCost,
                MaxStamina = MaxCombatStamina,
                LightWindupDuration = _lightWindupDuration,
                LightActiveDuration = _lightActiveDuration,
                LightRecoveryDuration = _lightRecoveryDuration,
                HeavyWindupDuration = _heavyWindupDuration,
                HeavyActiveDuration = _heavyActiveDuration,
                HeavyRecoveryDuration = _heavyRecoveryDuration,
                Range = Range,
                BlockDamageReduction = _blockDamageReduction,
                BlockStaminaDrainPerDamage = _blockStaminaDrainPerDamage,
                ComboWindowDuration = _comboWindowDuration
            };
        }
    }
}
