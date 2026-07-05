using Ashenveil.Core;
using UnityEngine;

namespace Ashenveil.Combat
{
    /// <summary>
    /// Deterministic melee combat model for attacks, combo timing, block mitigation, and dodge invulnerability hooks.
    /// Referenced GDD section: Kernsysteme / Combat.
    /// </summary>
    public sealed class CombatModel
    {
        private readonly Settings _settings;
        private CombatAttackPhase _phase = CombatAttackPhase.Idle;
        private CombatAttackType _attackType = CombatAttackType.None;
        private CombatAttackType _queuedAttackType = CombatAttackType.None;
        private float _phaseElapsed;
        private float _comboWindowRemaining;
        private bool _isBlocking;
        private bool _hasDodgeIFrames;

        /// <summary>
        /// Creates a combat model with full standalone combat stamina.
        /// </summary>
        public CombatModel(Settings settings)
        {
            _settings = settings.Normalized();
            CurrentStamina = _settings.MaxStamina;
        }

        /// <summary>
        /// Current attack phase.
        /// </summary>
        public CombatAttackPhase Phase => _phase;

        /// <summary>
        /// Current attack type.
        /// </summary>
        public CombatAttackType AttackType => _attackType;

        /// <summary>
        /// Current standalone combat stamina.
        /// </summary>
        public float CurrentStamina { get; private set; }

        /// <summary>
        /// Whether damage frames are currently active.
        /// </summary>
        public bool IsAttackActive => _phase == CombatAttackPhase.Active;

        /// <summary>
        /// Whether the player is currently holding block.
        /// </summary>
        public bool IsBlocking => _isBlocking;

        /// <summary>
        /// Whether a follow-up attack can currently be queued.
        /// </summary>
        public bool IsComboWindowOpen => _comboWindowRemaining > 0f && _phase == CombatAttackPhase.Recovery;

        /// <summary>
        /// Weapon range in meters.
        /// </summary>
        public float Range => _settings.Range;

        /// <summary>
        /// Attempts to start a light attack using model-owned stamina.
        /// </summary>
        public bool TryStartLightAttack()
        {
            return TryStartAttack(CombatAttackType.Light, true);
        }

        /// <summary>
        /// Attempts to start a heavy attack using model-owned stamina.
        /// </summary>
        public bool TryStartHeavyAttack()
        {
            return TryStartAttack(CombatAttackType.Heavy, true);
        }

        /// <summary>
        /// Starts a light attack after an external input layer has already authorized stamina.
        /// </summary>
        public bool TryStartPrepaidLightAttack()
        {
            return TryStartAttack(CombatAttackType.Light, false);
        }

        /// <summary>
        /// Starts a heavy attack after an external input layer has already authorized stamina.
        /// </summary>
        public bool TryStartPrepaidHeavyAttack()
        {
            return TryStartAttack(CombatAttackType.Heavy, false);
        }

        /// <summary>
        /// Sets whether block is being held.
        /// </summary>
        public void SetBlocking(bool isBlocking)
        {
            _isBlocking = isBlocking && _phase == CombatAttackPhase.Idle;
        }

        /// <summary>
        /// Sets the current dodge invulnerability hook from movement.
        /// </summary>
        public void SetDodgeIFrames(bool hasDodgeIFrames)
        {
            _hasDodgeIFrames = hasDodgeIFrames;
        }

        /// <summary>
        /// Advances combat timers by an explicit delta time.
        /// </summary>
        public State Tick(float deltaTime)
        {
            deltaTime = Mathf.Max(0f, deltaTime);
            if (_phase == CombatAttackPhase.Idle)
            {
                return CreateState();
            }

            _phaseElapsed += deltaTime;
            if (_comboWindowRemaining > 0f)
            {
                _comboWindowRemaining = Mathf.Max(0f, _comboWindowRemaining - deltaTime);
            }

            if (_phase == CombatAttackPhase.Windup && _phaseElapsed >= GetWindupDuration(_attackType))
            {
                ChangePhase(CombatAttackPhase.Active);
            }
            else if (_phase == CombatAttackPhase.Active && _phaseElapsed >= GetActiveDuration(_attackType))
            {
                _comboWindowRemaining = _settings.ComboWindowDuration;
                ChangePhase(CombatAttackPhase.Recovery);
            }
            else if (_phase == CombatAttackPhase.Recovery && _phaseElapsed >= GetRecoveryDuration(_attackType))
            {
                if (_queuedAttackType != CombatAttackType.None)
                {
                    CombatAttackType queued = _queuedAttackType;
                    _queuedAttackType = CombatAttackType.None;
                    StartAttack(queued);
                }
                else
                {
                    _attackType = CombatAttackType.None;
                    ChangePhase(CombatAttackPhase.Idle);
                }
            }

            return CreateState();
        }

        /// <summary>
        /// Creates the outgoing damage payload for the current swing.
        /// </summary>
        public DamageInfo CreateDamageInfo(Vector3 sourcePosition)
        {
            return new DamageInfo(
                _settings.Damage + _settings.AetherDamageBonus,
                _settings.DamageType,
                sourcePosition,
                _settings.Knockback);
        }

        /// <summary>
        /// Applies dodge and block mitigation to incoming damage.
        /// </summary>
        public DamageInfo ResolveIncomingDamage(DamageInfo incomingDamage)
        {
            if (_hasDodgeIFrames || incomingDamage.Amount <= 0f)
            {
                return new DamageInfo(0f, incomingDamage.DamageType, incomingDamage.SourcePosition, 0f);
            }

            if (!_isBlocking)
            {
                return incomingDamage;
            }

            float staminaDrain = incomingDamage.Amount * _settings.BlockStaminaDrainPerDamage;
            if (!SpendStamina(staminaDrain))
            {
                _isBlocking = false;
                return incomingDamage;
            }

            float reducedAmount = incomingDamage.Amount * (1f - _settings.BlockDamageReduction);
            return new DamageInfo(reducedAmount, incomingDamage.DamageType, incomingDamage.SourcePosition, incomingDamage.Knockback);
        }

        private bool TryStartAttack(CombatAttackType attackType, bool spendStamina)
        {
            if (_phase == CombatAttackPhase.Idle)
            {
                if (spendStamina && !SpendStamina(GetStaminaCost(attackType)))
                {
                    return false;
                }

                StartAttack(attackType);
                return true;
            }

            if (!IsComboWindowOpen || _queuedAttackType != CombatAttackType.None)
            {
                return false;
            }

            if (spendStamina && !SpendStamina(GetStaminaCost(attackType)))
            {
                return false;
            }

            _queuedAttackType = attackType;
            return true;
        }

        private void StartAttack(CombatAttackType attackType)
        {
            _isBlocking = false;
            _attackType = attackType;
            ChangePhase(CombatAttackPhase.Windup);
        }

        private void ChangePhase(CombatAttackPhase phase)
        {
            _phase = phase;
            _phaseElapsed = 0f;
            if (phase != CombatAttackPhase.Recovery)
            {
                _comboWindowRemaining = 0f;
            }
        }

        private bool SpendStamina(float amount)
        {
            amount = Mathf.Max(0f, amount);
            if (CurrentStamina < amount)
            {
                return false;
            }

            CurrentStamina = Mathf.Max(0f, CurrentStamina - amount);
            return true;
        }

        private float GetStaminaCost(CombatAttackType attackType)
        {
            return attackType == CombatAttackType.Heavy ? _settings.HeavyStaminaCost : _settings.LightStaminaCost;
        }

        private float GetWindupDuration(CombatAttackType attackType)
        {
            return attackType == CombatAttackType.Heavy ? _settings.HeavyWindupDuration : _settings.LightWindupDuration;
        }

        private float GetActiveDuration(CombatAttackType attackType)
        {
            return attackType == CombatAttackType.Heavy ? _settings.HeavyActiveDuration : _settings.LightActiveDuration;
        }

        private float GetRecoveryDuration(CombatAttackType attackType)
        {
            return attackType == CombatAttackType.Heavy ? _settings.HeavyRecoveryDuration : _settings.LightRecoveryDuration;
        }

        private State CreateState()
        {
            return new State
            {
                Phase = _phase,
                AttackType = _attackType,
                IsAttackActive = IsAttackActive,
                IsBlocking = _isBlocking,
                IsComboWindowOpen = IsComboWindowOpen,
                CurrentStamina = CurrentStamina
            };
        }

        /// <summary>
        /// Tunables used by the combat model.
        /// </summary>
        public struct Settings
        {
            /// <summary>
            /// Base outgoing damage.
            /// </summary>
            public float Damage { get; set; }

            /// <summary>
            /// Outgoing damage type.
            /// </summary>
            public DamageType DamageType { get; set; }

            /// <summary>
            /// Optional extra aether damage.
            /// </summary>
            public float AetherDamageBonus { get; set; }

            /// <summary>
            /// Requested knockback on hit.
            /// </summary>
            public float Knockback { get; set; }

            /// <summary>
            /// Standalone maximum stamina.
            /// </summary>
            public float MaxStamina { get; set; }

            /// <summary>
            /// Light attack stamina cost.
            /// </summary>
            public float LightStaminaCost { get; set; }

            /// <summary>
            /// Heavy attack stamina cost.
            /// </summary>
            public float HeavyStaminaCost { get; set; }

            /// <summary>
            /// Light attack windup duration.
            /// </summary>
            public float LightWindupDuration { get; set; }

            /// <summary>
            /// Light attack active duration.
            /// </summary>
            public float LightActiveDuration { get; set; }

            /// <summary>
            /// Light attack recovery duration.
            /// </summary>
            public float LightRecoveryDuration { get; set; }

            /// <summary>
            /// Heavy attack windup duration.
            /// </summary>
            public float HeavyWindupDuration { get; set; }

            /// <summary>
            /// Heavy attack active duration.
            /// </summary>
            public float HeavyActiveDuration { get; set; }

            /// <summary>
            /// Heavy attack recovery duration.
            /// </summary>
            public float HeavyRecoveryDuration { get; set; }

            /// <summary>
            /// Weapon reach in meters.
            /// </summary>
            public float Range { get; set; }

            /// <summary>
            /// Damage fraction prevented while block succeeds.
            /// </summary>
            public float BlockDamageReduction { get; set; }

            /// <summary>
            /// Stamina drained per incoming damage point blocked.
            /// </summary>
            public float BlockStaminaDrainPerDamage { get; set; }

            /// <summary>
            /// Recovery time during which a follow-up can be queued.
            /// </summary>
            public float ComboWindowDuration { get; set; }

            /// <summary>
            /// Sensible default combat settings for demo melee.
            /// </summary>
            public static Settings Default => new Settings
            {
                Damage = 18f,
                DamageType = DamageType.Physical,
                AetherDamageBonus = 0f,
                Knockback = 3f,
                MaxStamina = 100f,
                LightStaminaCost = 12f,
                HeavyStaminaCost = 28f,
                LightWindupDuration = 0.12f,
                LightActiveDuration = 0.16f,
                LightRecoveryDuration = 0.28f,
                HeavyWindupDuration = 0.24f,
                HeavyActiveDuration = 0.22f,
                HeavyRecoveryDuration = 0.42f,
                Range = 1.8f,
                BlockDamageReduction = 0.55f,
                BlockStaminaDrainPerDamage = 0.45f,
                ComboWindowDuration = 0.22f
            };

            /// <summary>
            /// Returns settings clamped to usable values.
            /// </summary>
            public Settings Normalized()
            {
                Settings settings = this;
                settings.Damage = Mathf.Max(0f, Damage);
                settings.AetherDamageBonus = Mathf.Max(0f, AetherDamageBonus);
                settings.Knockback = Mathf.Max(0f, Knockback);
                settings.MaxStamina = Mathf.Max(1f, MaxStamina);
                settings.LightStaminaCost = Mathf.Max(0f, LightStaminaCost);
                settings.HeavyStaminaCost = Mathf.Max(0f, HeavyStaminaCost);
                settings.LightWindupDuration = Mathf.Max(0.01f, LightWindupDuration);
                settings.LightActiveDuration = Mathf.Max(0.01f, LightActiveDuration);
                settings.LightRecoveryDuration = Mathf.Max(0.01f, LightRecoveryDuration);
                settings.HeavyWindupDuration = Mathf.Max(0.01f, HeavyWindupDuration);
                settings.HeavyActiveDuration = Mathf.Max(0.01f, HeavyActiveDuration);
                settings.HeavyRecoveryDuration = Mathf.Max(0.01f, HeavyRecoveryDuration);
                settings.Range = Mathf.Max(0.1f, Range);
                settings.BlockDamageReduction = Mathf.Clamp01(BlockDamageReduction);
                settings.BlockStaminaDrainPerDamage = Mathf.Max(0f, BlockStaminaDrainPerDamage);
                settings.ComboWindowDuration = Mathf.Max(0f, ComboWindowDuration);
                return settings;
            }
        }

        /// <summary>
        /// Snapshot returned by one combat tick.
        /// </summary>
        public struct State
        {
            /// <summary>
            /// Current attack phase.
            /// </summary>
            public CombatAttackPhase Phase { get; set; }

            /// <summary>
            /// Current attack type.
            /// </summary>
            public CombatAttackType AttackType { get; set; }

            /// <summary>
            /// Whether the weapon hitbox should be active.
            /// </summary>
            public bool IsAttackActive { get; set; }

            /// <summary>
            /// Whether block is held.
            /// </summary>
            public bool IsBlocking { get; set; }

            /// <summary>
            /// Whether a combo follow-up can be queued.
            /// </summary>
            public bool IsComboWindowOpen { get; set; }

            /// <summary>
            /// Current standalone combat stamina.
            /// </summary>
            public float CurrentStamina { get; set; }
        }
    }
}
