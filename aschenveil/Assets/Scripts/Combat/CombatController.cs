using System;
using Ashenveil.Core;
using Ashenveil.Player;
using UnityEngine;

namespace Ashenveil.Combat
{
    /// <summary>
    /// MonoBehaviour adapter that connects player attack events to the combat model and weapon hitbox.
    /// Referenced GDD section: Kernsysteme / Combat.
    /// </summary>
    public sealed class CombatController : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private WeaponDefinition _weaponDefinition;

        [Header("References")]
        [SerializeField] private PlayerMovementController _movementController;
        [SerializeField] private WeaponHitbox _weaponHitbox;
        [SerializeField] private Transform _damageOrigin;

        private CombatModel _model;
        private bool _hitboxActive;
        private Func<DamageInfo, DamageInfo> _damageModifier;

        /// <summary>
        /// Current deterministic combat model.
        /// </summary>
        public CombatModel Model => _model;

        /// <summary>
        /// Installs an optional post-processor applied to each swing's damage payload
        /// just before the hitbox opens. Used by the aether system to convert a swing
        /// into an empowered <see cref="DamageType.Aether"/> strike. Pass null to clear.
        /// </summary>
        public void SetDamageModifier(Func<DamageInfo, DamageInfo> modifier)
        {
            _damageModifier = modifier;
        }

        private void Awake()
        {
            if (_weaponDefinition == null)
            {
                Debug.LogError("CombatController requires a WeaponDefinition.", this);
                enabled = false;
                return;
            }

            if (_movementController == null && !TryGetComponent(out _movementController))
            {
                Debug.LogError("CombatController requires a PlayerMovementController reference.", this);
                enabled = false;
                return;
            }

            if (_weaponHitbox == null)
            {
                Debug.LogError("CombatController requires a WeaponHitbox reference.", this);
                enabled = false;
                return;
            }

            if (_damageOrigin == null)
            {
                _damageOrigin = transform;
            }

            _model = new CombatModel(_weaponDefinition.ToSettings());
        }

        private void OnEnable()
        {
            if (_movementController == null)
            {
                return;
            }

            _movementController.LightAttackPressed += OnLightAttackPressed;
            _movementController.BlockOrHeavyPressed += OnBlockOrHeavyPressed;
        }

        private void OnDisable()
        {
            if (_movementController != null)
            {
                _movementController.LightAttackPressed -= OnLightAttackPressed;
                _movementController.BlockOrHeavyPressed -= OnBlockOrHeavyPressed;
            }

            if (_weaponHitbox != null)
            {
                _weaponHitbox.EndSwing();
            }
        }

        private void Update()
        {
            if (_model == null)
            {
                return;
            }

            CombatModel.State state = _model.Tick(Time.deltaTime);
            if (state.IsAttackActive && !_hitboxActive)
            {
                DamageInfo damage = _model.CreateDamageInfo(_damageOrigin.position);
                if (_damageModifier != null)
                {
                    damage = _damageModifier(damage);
                }

                _weaponHitbox.BeginSwing(damage);
                _hitboxActive = true;
            }
            else if (!state.IsAttackActive && _hitboxActive)
            {
                _weaponHitbox.EndSwing();
                _hitboxActive = false;
            }
        }

        /// <summary>
        /// Allows future input adapters to update block hold state without changing the model.
        /// </summary>
        public void SetBlocking(bool isBlocking)
        {
            _model?.SetBlocking(isBlocking);
        }

        /// <summary>
        /// Allows movement adapters to forward dodge invulnerability state.
        /// </summary>
        public void SetDodgeIFrames(bool hasDodgeIFrames)
        {
            _model?.SetDodgeIFrames(hasDodgeIFrames);
        }

        private void OnLightAttackPressed()
        {
            _model?.TryStartPrepaidLightAttack();
        }

        private void OnBlockOrHeavyPressed()
        {
            _model?.TryStartPrepaidHeavyAttack();
        }
    }
}
