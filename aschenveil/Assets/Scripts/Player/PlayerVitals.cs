using System;
using Ashenveil.Core;
using UnityEngine;

namespace Ashenveil.Player
{
    /// <summary>
    /// MonoBehaviour adapter for player health and stamina.
    /// Referenced GDD section: Kernsysteme / Movement/Camera and Combat.
    /// </summary>
    public sealed class PlayerVitals : MonoBehaviour, IDamageable
    {
        [Header("Configuration")]
        [SerializeField] private PlayerInputConfig _inputConfig;
        [SerializeField] private float _maxHealth = 100f;

        private StaminaModel _stamina;
        private float _currentHealth;
        private bool _initialized;

        /// <summary>
        /// Raised when health changes. Args: current health, maximum health.
        /// </summary>
        public event Action<float, float> HealthChanged;

        /// <summary>
        /// Raised when stamina changes. Args: current stamina, maximum stamina.
        /// </summary>
        public event Action<float, float> StaminaChanged;

        /// <summary>
        /// Raised once when health reaches zero.
        /// </summary>
        public event Action Died;

        /// <summary>
        /// Indicates whether the player is alive.
        /// </summary>
        public bool IsAlive
        {
            get
            {
                EnsureInitialized();
                return _currentHealth > 0f;
            }
        }

        /// <summary>
        /// Current health value.
        /// </summary>
        public float CurrentHealth
        {
            get
            {
                EnsureInitialized();
                return _currentHealth;
            }
        }

        private float _corruptionPenalty;

        /// <summary>
        /// Maximum health value, reduced by the current aether corruption penalty.
        /// </summary>
        public float MaxHealth => Mathf.Max(1f, _maxHealth * (1f - _corruptionPenalty));

        /// <summary>
        /// Sets the fraction (0..1) by which corruption reduces max health, and clamps
        /// current health to the new maximum. Referenced GDD section: Kernsysteme / Äther.
        /// </summary>
        public void SetCorruptionPenalty(float penalty01)
        {
            EnsureInitialized();
            _corruptionPenalty = Mathf.Clamp01(penalty01);
            if (_currentHealth > MaxHealth)
            {
                _currentHealth = MaxHealth;
            }

            HealthChanged?.Invoke(_currentHealth, MaxHealth);
        }

        /// <summary>
        /// Player stamina model.
        /// </summary>
        public StaminaModel Stamina
        {
            get
            {
                EnsureInitialized();
                return _stamina;
            }
        }

        private void Awake()
        {
            EnsureInitialized();
        }

        /// <summary>
        /// Applies damage to player health and raises the death event when depleted.
        /// </summary>
        public void TakeDamage(DamageInfo damageInfo)
        {
            EnsureInitialized();
            if (!IsAlive || damageInfo.Amount <= 0f)
            {
                return;
            }

            _currentHealth = Mathf.Max(0f, _currentHealth - damageInfo.Amount);
            HealthChanged?.Invoke(_currentHealth, MaxHealth);

            if (_currentHealth <= 0f)
            {
                Died?.Invoke();
            }
        }

        /// <summary>
        /// Advances the stamina model and emits change notifications.
        /// </summary>
        public StaminaModel.State UpdateStamina(bool sprintRequested, float deltaTime)
        {
            EnsureInitialized();
            StaminaModel.State state = _stamina.Update(sprintRequested, deltaTime);
            StaminaChanged?.Invoke(state.CurrentStamina, _stamina.MaxStamina);
            return state;
        }

        private void EnsureInitialized()
        {
            if (_stamina == null)
            {
                StaminaModel.Settings settings = _inputConfig != null
                    ? _inputConfig.ToStaminaSettings()
                    : StaminaModel.Settings.Default;
                _stamina = new StaminaModel(settings);
            }

            if (!_initialized)
            {
                _currentHealth = MaxHealth;
                _initialized = true;
            }
        }
    }
}
