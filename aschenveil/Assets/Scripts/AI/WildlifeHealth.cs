using System;
using Ashenveil.Combat;
using Ashenveil.Core;
using UnityEngine;

namespace Ashenveil.AI
{
    /// <summary>
    /// IDamageable adapter for wildlife health.
    /// Referenced GDD section: Kernsysteme / Wildlife.
    /// </summary>
    public sealed class WildlifeHealth : MonoBehaviour, IDamageable
    {
        [Header("Configuration")]
        [SerializeField] private WildlifeSpecies _species;

        private float _currentHealth;
        private bool _initialized;

        /// <summary>
        /// Raised when this animal receives damage.
        /// </summary>
        public event Action<DamageInfo> Damaged;

        /// <summary>
        /// Raised once when health reaches zero.
        /// </summary>
        public event Action Died;

        /// <summary>
        /// Indicates whether this animal is alive.
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
        /// Current health.
        /// </summary>
        public float CurrentHealth
        {
            get
            {
                EnsureInitialized();
                return _currentHealth;
            }
        }

        /// <summary>
        /// Maximum health.
        /// </summary>
        public float MaxHealth => _species != null ? _species.Health : 1f;

        /// <summary>
        /// Current health fraction from 0..1.
        /// </summary>
        public float HealthFraction => Mathf.Clamp01(CurrentHealth / MaxHealth);

        private void Awake()
        {
            EnsureInitialized();
        }

        /// <summary>
        /// Initializes health from the supplied species when spawned.
        /// </summary>
        public void Initialize(WildlifeSpecies species)
        {
            _species = species;
            _currentHealth = MaxHealth;
            _initialized = true;
        }

        /// <summary>
        /// Applies damage and raises wildlife death signals when depleted.
        /// </summary>
        public void TakeDamage(DamageInfo damageInfo)
        {
            EnsureInitialized();
            if (!IsAlive || damageInfo.Amount <= 0f)
            {
                return;
            }

            _currentHealth = Mathf.Max(0f, _currentHealth - damageInfo.Amount);
            Damaged?.Invoke(damageInfo);
            HitReaction.RequestHitFlash(gameObject, damageInfo);

            if (_currentHealth <= 0f)
            {
                GameSignals.RaiseAnimalKilled(_species != null ? _species.SpeciesId.ToString() : "Wildlife", transform.position);
                Died?.Invoke();
            }
        }

        private void EnsureInitialized()
        {
            if (_initialized)
            {
                return;
            }

            _currentHealth = MaxHealth;
            _initialized = true;
        }
    }
}
