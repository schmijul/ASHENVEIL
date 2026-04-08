using System;
using UnityEngine;
using Ashenveil.Combat;

namespace Ashenveil.AI
{
    /// <summary>
    /// Wildlife health and resistance component that implements the combat damage contract.
    /// </summary>
    public class WildlifeHealth : MonoBehaviour, IDamageable
    {
        [Header("References")]
        [SerializeField] private WildlifeProfile _profile;

        [Header("Runtime")]
        [SerializeField, Min(0f)] private float _startingHealth = -1f;
        [SerializeField] private WildlifeSpecies _fallbackSpecies = WildlifeSpecies.Boar;

        private WildlifeDamageModel _damageModel;
        private float _currentHealth;
        private float _staggerEndsAt = -1f;
        private bool _isDead;

        public event Action<float, float> HealthChanged;
        public event Action<float, DamageType> Damaged;
        public event Action<float> StaggerStarted;
        public event Action Died;

        public WildlifeProfile Profile => _profile;

        public float CurrentHealth => _currentHealth;

        public float MaxHealth => _profile != null ? _profile.MaxHealth : 0f;

        public float Armor => _profile != null ? _profile.Armor : 0f;

        public bool IsDead => _isDead;

        public bool IsStaggered => Time.time < _staggerEndsAt;

        public float HealthRatio => MaxHealth > 0f ? Mathf.Clamp01(_currentHealth / MaxHealth) : 0f;

        private void Awake()
        {
            ResolveProfile();
            InitializeRuntime();
        }

        private void OnValidate()
        {
            ResolveProfile();
        }

        private void Update()
        {
            if (!_isDead && _currentHealth <= 0f)
            {
                HandleDeath();
            }
        }

        public void ResetToFullHealth()
        {
            if (_profile == null)
            {
                ResolveProfile();
            }

            InitializeRuntime();
        }

        public void TakeDamage(float amount, DamageType damageType)
        {
            if (_isDead || amount <= 0f)
            {
                return;
            }

            float effectiveDamage = _damageModel != null ? _damageModel.CalculateDamage(amount, damageType) : Mathf.Max(0f, amount);
            if (effectiveDamage <= 0f)
            {
                return;
            }

            _currentHealth = Mathf.Max(0f, _currentHealth - effectiveDamage);
            Damaged?.Invoke(effectiveDamage, damageType);
            HealthChanged?.Invoke(_currentHealth, MaxHealth);

            if (_currentHealth <= 0f)
            {
                HandleDeath();
            }
        }

        public void ApplyStagger(float duration)
        {
            float staggerDuration = Mathf.Max(0f, duration);
            if (staggerDuration <= 0f)
            {
                return;
            }

            _staggerEndsAt = Mathf.Max(_staggerEndsAt, Time.time + staggerDuration);
            StaggerStarted?.Invoke(staggerDuration);
        }

        private void ResolveProfile()
        {
            if (_profile != null)
            {
                return;
            }

            _profile = WildlifeProfile.CreateRuntimeDefaults(_fallbackSpecies);
        }

        private void InitializeRuntime()
        {
            if (_profile == null)
            {
                Debug.LogError($"{nameof(WildlifeHealth)} on {name} requires a {nameof(WildlifeProfile)} reference.");
                enabled = false;
                return;
            }

            _damageModel = _profile.CreateDamageModel();
            _currentHealth = _startingHealth < 0f ? _profile.MaxHealth : Mathf.Clamp(_startingHealth, 0f, _profile.MaxHealth);
            _staggerEndsAt = -1f;
            _isDead = _currentHealth <= 0f;
            HealthChanged?.Invoke(_currentHealth, MaxHealth);
        }

        private void HandleDeath()
        {
            if (_isDead)
            {
                return;
            }

            _isDead = true;
            _currentHealth = 0f;
            HealthChanged?.Invoke(_currentHealth, MaxHealth);
            Died?.Invoke();
        }
    }
}
