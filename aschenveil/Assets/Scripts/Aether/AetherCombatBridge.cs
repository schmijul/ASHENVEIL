using Ashenveil.Combat;
using Ashenveil.Core;
using Ashenveil.Player;
using UnityEngine;

namespace Ashenveil.Aether
{
    /// <summary>
    /// Bridges the aether reserve into combat. While aether mode is armed (toggled by
    /// the player's aether input) and enough charge is available, the next swing is
    /// converted into an empowered <see cref="DamageType.Aether"/> strike with bonus
    /// damage; charge is spent and corruption rises. This is the only way to damage the
    /// mutated wolf boss meaningfully. Referenced GDD section: Kernsysteme / Äther.
    /// </summary>
    [RequireComponent(typeof(CombatController))]
    public sealed class AetherCombatBridge : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private AetherPool _aetherPool;
        [SerializeField] private PlayerMovementController _movementController;
        [SerializeField] private CombatController _combatController;

        [Header("Empowerment")]
        [SerializeField] private float _chargeCostPerStrike = 15f;
        [SerializeField] private float _bonusDamage = 20f;

        private bool _armed;

        /// <summary>
        /// Whether aether mode is currently armed.
        /// </summary>
        public bool IsArmed => _armed;

        private void Awake()
        {
            if (_combatController == null)
            {
                TryGetComponent(out _combatController);
            }
        }

        private void OnEnable()
        {
            if (_movementController != null)
            {
                _movementController.AetherModePressed += ToggleArmed;
            }

            _combatController?.SetDamageModifier(ModifyDamage);
        }

        private void OnDisable()
        {
            if (_movementController != null)
            {
                _movementController.AetherModePressed -= ToggleArmed;
            }

            _combatController?.SetDamageModifier(null);
        }

        private void ToggleArmed()
        {
            _armed = !_armed;
        }

        private DamageInfo ModifyDamage(DamageInfo baseDamage)
        {
            if (!_armed || _aetherPool == null)
            {
                return baseDamage;
            }

            if (!_aetherPool.Model.SpendForEmpoweredAttack(_chargeCostPerStrike))
            {
                // Not enough charge: fall back to a mundane strike.
                return baseDamage;
            }

            return new DamageInfo(
                baseDamage.Amount + _bonusDamage,
                DamageType.Aether,
                baseDamage.SourcePosition,
                baseDamage.Knockback);
        }
    }
}
