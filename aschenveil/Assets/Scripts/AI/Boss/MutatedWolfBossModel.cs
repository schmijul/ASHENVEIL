using System;
using Ashenveil.Core;
using UnityEngine;

namespace Ashenveil.AI.Boss
{
    /// <summary>
    /// Pure logic for the demo boss, a wolf mutated by aether. Its defining rule is the
    /// ÄTHER GATE: mundane weapons are almost useless (physical damage reduced to
    /// <see cref="PhysicalDamageMultiplier"/>), so the player must use aether-empowered
    /// strikes to win. Referenced GDD section: Demo-Ablauf Phase 6.
    /// </summary>
    public sealed class MutatedWolfBossModel
    {
        /// <summary>
        /// Fraction of physical damage that actually lands ("fast wirkungslos").
        /// </summary>
        public const float PhysicalDamageMultiplier = 0.1f;

        private readonly float _maxHealth;
        private readonly float _enrageHealthFraction;
        private readonly float _lungeCooldown;
        private readonly IRandomSource _random;
        private float _health;
        private BossPhase _phase;
        private float _attackCooldownRemaining;
        private bool _defeated;

        /// <summary>
        /// Raised when the boss phase changes. Args: new phase.
        /// </summary>
        public event Action<BossPhase> PhaseChanged;

        /// <summary>
        /// Raised when health changes. Args: health fraction 0..1.
        /// </summary>
        public event Action<float> HealthChanged;

        /// <summary>
        /// Raised exactly once when the boss is defeated.
        /// </summary>
        public event Action Defeated;

        /// <summary>
        /// Creates a boss model.
        /// </summary>
        public MutatedWolfBossModel(
            float maxHealth,
            float enrageHealthFraction,
            float lungeCooldown,
            IRandomSource random)
        {
            if (maxHealth <= 0f)
            {
                throw new ArgumentException("maxHealth must be positive.", nameof(maxHealth));
            }

            _maxHealth = maxHealth;
            _enrageHealthFraction = Mathf.Clamp01(enrageHealthFraction);
            _lungeCooldown = Mathf.Max(0.1f, lungeCooldown);
            _random = random ?? throw new ArgumentNullException(nameof(random));
            _health = maxHealth;
            _phase = BossPhase.Stalk;
        }

        /// <summary>
        /// Current phase.
        /// </summary>
        public BossPhase Phase => _phase;

        /// <summary>
        /// Current health.
        /// </summary>
        public float Health => _health;

        /// <summary>
        /// Normalized health for the boss bar (0..1).
        /// </summary>
        public float HealthFraction => Mathf.Clamp01(_health / _maxHealth);

        /// <summary>
        /// Whether the boss has been defeated.
        /// </summary>
        public bool IsDefeated => _defeated;

        /// <summary>
        /// Applies the aether gate and returns the damage that actually landed.
        /// </summary>
        public float ApplyDamage(DamageInfo damageInfo)
        {
            if (_defeated)
            {
                return 0f;
            }

            float effective = damageInfo.DamageType == DamageType.Aether
                ? damageInfo.Amount
                : damageInfo.Amount * PhysicalDamageMultiplier;

            if (effective <= 0f)
            {
                return 0f;
            }

            _health = Mathf.Max(0f, _health - effective);
            HealthChanged?.Invoke(HealthFraction);

            if (_health <= 0f)
            {
                _defeated = true;
                SetPhase(BossPhase.Dead);
                Defeated?.Invoke();
                return effective;
            }

            if (_phase != BossPhase.Enrage && HealthFraction <= _enrageHealthFraction)
            {
                SetPhase(BossPhase.Enrage);
            }

            return effective;
        }

        /// <summary>
        /// Advances attack cadence and returns the action chosen this tick (or None).
        /// </summary>
        public BossAction Tick(float deltaTime)
        {
            if (_defeated || deltaTime <= 0f)
            {
                return BossAction.None;
            }

            _attackCooldownRemaining -= deltaTime;
            if (_attackCooldownRemaining > 0f)
            {
                return BossAction.None;
            }

            // Enrage attacks faster.
            float cooldown = _phase == BossPhase.Enrage ? _lungeCooldown * 0.6f : _lungeCooldown;
            _attackCooldownRemaining = cooldown;

            if (_phase == BossPhase.Stalk)
            {
                SetPhase(BossPhase.Lunge);
            }

            // Randomized pattern: bias toward lunge, occasional swipe.
            return _random.Next01() < 0.65f ? BossAction.Lunge : BossAction.Swipe;
        }

        private void SetPhase(BossPhase phase)
        {
            if (_phase == phase)
            {
                return;
            }

            _phase = phase;
            PhaseChanged?.Invoke(phase);
        }
    }
}
