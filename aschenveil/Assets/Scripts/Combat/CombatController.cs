using System;
using UnityEngine;
using Ashenveil.Player;

namespace Ashenveil.Combat
{
    /// <summary>
    /// Player-facing combat controller that coordinates stamina, runtime state, animator triggers, and weapon hitboxes.
    /// </summary>
    public class CombatController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private StaminaSystem _staminaSystem;
        [SerializeField] private WeaponData _weaponData;
        [SerializeField] private WeaponHitbox _weaponHitbox;
        [SerializeField] private Animator _animator;

        [Header("Animation Triggers")]
        [SerializeField] private string _blockStartTrigger = "BlockStart";
        [SerializeField] private string _blockEndTrigger = "BlockEnd";
        [SerializeField] private string _dodgeTrigger = "Dodge";

        private WeaponData _runtimeWeaponData;
        private CombatRuntimeState _runtimeState;
        private AttackStepDefinition _currentAttackStep;
        private Vector2 _movementInput;
        private CombatActionKind _lastReportedState = CombatActionKind.Idle;

        public event Action<CombatActionKind> CombatStateChanged;
        public event Action<AttackStepDefinition> AttackStarted;
        public event Action<AttackStepDefinition> AttackEnded;
        public event Action BlockStarted;
        public event Action BlockEnded;
        public event Action<DodgeSettings, Vector2> DodgeStarted;
        public event Action DodgeEnded;
        public event Action<IDamageable, float, DamageType> DamageApplied;

        public WeaponData WeaponData => EffectiveWeaponData;

        public CombatActionKind CurrentState => _runtimeState != null ? _runtimeState.CurrentAction : CombatActionKind.Idle;

        public bool IsBlocking => _runtimeState != null && _runtimeState.IsBlocking;

        public bool IsDodging => _runtimeState != null && _runtimeState.IsDodging;

        public bool IsAttacking => _runtimeState != null && _runtimeState.IsAttacking;

        public AttackStepDefinition CurrentAttackStep => _currentAttackStep;

        public float AnimationSpeedMultiplier => EffectiveWeaponData != null ? EffectiveWeaponData.AttackSpeed : 1f;

        private void Awake()
        {
            ResolveReferences();
            InitializeRuntimeState();
        }

        private void OnEnable()
        {
            ResolveReferences();
            InitializeRuntimeState();

            if (_weaponHitbox != null)
            {
                _weaponHitbox.TargetHit += HandleWeaponHitboxTargetHit;
            }

            PublishCombatStateIfChanged();
        }

        private void Update()
        {
            if (_runtimeState == null)
            {
                return;
            }

            _runtimeState.Tick(Time.deltaTime);
            PublishCombatStateIfChanged();
        }

        private void OnDisable()
        {
            if (_weaponHitbox != null)
            {
                _weaponHitbox.TargetHit -= HandleWeaponHitboxTargetHit;
                _weaponHitbox.EndSwing();
            }

            _runtimeState?.Reset();
            _currentAttackStep = null;
            _lastReportedState = CombatActionKind.Idle;
        }

        private void OnValidate()
        {
            ResolveReferences();
        }

        public void SetMovementInput(Vector2 movementInput)
        {
            _movementInput = movementInput;
        }

        public bool RequestLightAttack()
        {
            if (_runtimeState == null)
            {
                return false;
            }

            float currentStamina = GetCurrentStamina();
            if (!_runtimeState.TryBeginLightAttack(currentStamina, out AttackStepDefinition attackStep, out float staminaCost))
            {
                return false;
            }

            if (!ConsumeStamina(staminaCost))
            {
                return false;
            }

            _currentAttackStep = attackStep;
            SetAnimatorTrigger(ResolveAttackTrigger(attackStep, _runtimeState.CurrentComboStepIndex));
            AttackStarted?.Invoke(attackStep);
            PublishCombatStateIfChanged();
            return true;
        }

        public bool RequestHeavyAttack()
        {
            if (_runtimeState == null)
            {
                return false;
            }

            float currentStamina = GetCurrentStamina();
            if (!_runtimeState.TryBeginHeavyAttack(currentStamina, out AttackStepDefinition attackStep, out float staminaCost))
            {
                return false;
            }

            if (!ConsumeStamina(staminaCost))
            {
                return false;
            }

            _currentAttackStep = attackStep;
            SetAnimatorTrigger(string.IsNullOrWhiteSpace(attackStep.AnimationTrigger) ? "HeavyAttack" : attackStep.AnimationTrigger);
            AttackStarted?.Invoke(attackStep);
            PublishCombatStateIfChanged();
            return true;
        }

        public bool BeginBlock()
        {
            bool wasBlocking = IsBlocking;
            bool wasAttacking = IsAttacking;
            if (_runtimeState == null || !_runtimeState.TryBeginBlock())
            {
                return false;
            }

            if (wasAttacking)
            {
                CancelCurrentAttackVisuals();
            }

            if (!wasBlocking)
            {
                SetAnimatorTrigger(_blockStartTrigger);
                BlockStarted?.Invoke();
            }

            PublishCombatStateIfChanged();
            return true;
        }

        public bool EndBlock()
        {
            if (_runtimeState == null || !_runtimeState.EndBlock())
            {
                return false;
            }

            SetAnimatorTrigger(_blockEndTrigger);
            BlockEnded?.Invoke();
            PublishCombatStateIfChanged();
            return true;
        }

        public bool RequestDodge()
        {
            bool wasBlocking = IsBlocking;
            bool wasAttacking = IsAttacking;

            if (_runtimeState == null)
            {
                return false;
            }

            float currentStamina = GetCurrentStamina();
            if (!_runtimeState.TryBeginDodge(currentStamina, out DodgeSettings dodgeSettings, out float staminaCost))
            {
                return false;
            }

            if (!ConsumeStamina(staminaCost))
            {
                return false;
            }

            if (wasBlocking)
            {
                SetAnimatorTrigger(_blockEndTrigger);
                BlockEnded?.Invoke();
            }

            if (wasAttacking)
            {
                CancelCurrentAttackVisuals();
            }

            SetAnimatorTrigger(_dodgeTrigger);
            DodgeStarted?.Invoke(dodgeSettings, ResolveDodgeDirection());
            PublishCombatStateIfChanged();
            return true;
        }

        public void OnWeaponSwingBegin()
        {
            if (_weaponHitbox != null)
            {
                _weaponHitbox.BeginSwing();
            }
        }

        public void OnWeaponSwingEnd()
        {
            if (_weaponHitbox != null)
            {
                _weaponHitbox.EndSwing();
            }

            if (_currentAttackStep != null)
            {
                AttackStepDefinition endedStep = _currentAttackStep;
                _currentAttackStep = null;
                AttackEnded?.Invoke(endedStep);
            }
        }

        public void OnDodgeEnd()
        {
            DodgeEnded?.Invoke();
            PublishCombatStateIfChanged();
        }

        private void CancelCurrentAttackVisuals()
        {
            if (_weaponHitbox != null)
            {
                _weaponHitbox.EndSwing();
            }

            if (_currentAttackStep != null)
            {
                AttackStepDefinition endedStep = _currentAttackStep;
                _currentAttackStep = null;
                AttackEnded?.Invoke(endedStep);
            }
        }

        private WeaponData EffectiveWeaponData
        {
            get
            {
                if (_weaponData != null)
                {
                    return _weaponData;
                }

                if (_runtimeWeaponData == null)
                {
                    _runtimeWeaponData = WeaponData.CreateRuntimeDefaults();
                }

                return _runtimeWeaponData;
            }
        }

        private void ResolveReferences()
        {
            if (_staminaSystem == null)
            {
                TryGetComponent(out _staminaSystem);
            }

            if (_weaponHitbox == null)
            {
                _weaponHitbox = GetComponentInChildren<WeaponHitbox>(true);
            }

            if (_animator == null)
            {
                TryGetComponent(out _animator);
            }
        }

        private void InitializeRuntimeState()
        {
            WeaponData weaponData = EffectiveWeaponData;
            if (weaponData == null)
            {
                _runtimeState = null;
                return;
            }

            if (_runtimeState == null || _runtimeWeaponData != weaponData)
            {
                _runtimeState = new CombatRuntimeState(
                    weaponData.LightAttackCombo,
                    weaponData.HeavyAttack,
                    weaponData.BlockSettings,
                    weaponData.DodgeSettings);
                _runtimeWeaponData = weaponData;
                _lastReportedState = _runtimeState.CurrentAction;
            }
        }

        private void HandleWeaponHitboxTargetHit(IDamageable target, Collider collider)
        {
            if (_runtimeState == null || target == null || _currentAttackStep == null || _weaponData == null && _runtimeWeaponData == null)
            {
                return;
            }

            WeaponData weaponData = EffectiveWeaponData;
            if (weaponData == null)
            {
                return;
            }

            float finalDamage = DamageResolver.CalculateFinalDamage(weaponData, _currentAttackStep, target.Armor);
            DamageType damageType = ResolveDamageType(weaponData.Type);
            target.TakeDamage(finalDamage, damageType);
            DamageApplied?.Invoke(target, finalDamage, damageType);
        }

        private float GetCurrentStamina()
        {
            return _staminaSystem != null ? _staminaSystem.CurrentStamina : 0f;
        }

        private bool ConsumeStamina(float amount)
        {
            if (_staminaSystem == null)
            {
                return false;
            }

            return _staminaSystem.TryConsume(amount);
        }

        private Vector2 ResolveDodgeDirection()
        {
            if (_movementInput.sqrMagnitude > 0.0001f)
            {
                return _movementInput.normalized;
            }

            return Vector2.up;
        }

        private void PublishCombatStateIfChanged()
        {
            if (_runtimeState == null)
            {
                return;
            }

            CombatActionKind currentState = _runtimeState.CurrentAction;
            if (currentState == _lastReportedState)
            {
                return;
            }

            _lastReportedState = currentState;
            CombatStateChanged?.Invoke(currentState);
        }

        private void SetAnimatorTrigger(string triggerName)
        {
            if (_animator == null || string.IsNullOrWhiteSpace(triggerName))
            {
                return;
            }

            _animator.SetTrigger(triggerName);
        }

        private static string ResolveAttackTrigger(AttackStepDefinition attackStep, int comboStep)
        {
            if (attackStep != null && !string.IsNullOrWhiteSpace(attackStep.AnimationTrigger))
            {
                return attackStep.AnimationTrigger;
            }

            return "LightAttack" + Mathf.Clamp(comboStep, 1, 3);
        }

        private static DamageType ResolveDamageType(WeaponType weaponType)
        {
            switch (weaponType)
            {
                case WeaponType.Axe:
                case WeaponType.Sword:
                case WeaponType.Dagger:
                    return DamageType.Slash;
                case WeaponType.Spear:
                case WeaponType.Bow:
                    return DamageType.Pierce;
                case WeaponType.Mace:
                    return DamageType.Blunt;
                default:
                    return DamageType.Physical;
            }
        }
    }
}
