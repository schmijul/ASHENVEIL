using System;
using Ashenveil.Core;
using UnityEngine;

namespace Ashenveil.AI.Boss
{
    /// <summary>
    /// MonoBehaviour adapter for the mutated wolf boss. Implements <see cref="IDamageable"/>
    /// and routes every hit through <see cref="MutatedWolfBossModel"/> so the aether gate
    /// applies. Exposes events for the boss health bar and raises
    /// <see cref="GameSignals.BossDefeated"/> on death. Referenced GDD section:
    /// Demo-Ablauf Phase 6.
    /// </summary>
    public sealed class MutatedWolfBossController : MonoBehaviour, IDamageable
    {
        [Header("Identity")]
        [SerializeField] private string _bossId = "mutated_wolf";
        [SerializeField] private string _displayName = "Mutierter Wolf";

        [Header("Stats")]
        [SerializeField] private float _maxHealth = 240f;
        [SerializeField] private float _enrageHealthFraction = 0.4f;
        [SerializeField] private float _lungeCooldown = 2.5f;

        private MutatedWolfBossModel _model;
        private bool _active;

        /// <summary>
        /// Raised when the boss health fraction changes. Args: displayName, fraction.
        /// </summary>
        public event Action<string, float> HealthChanged;

        /// <summary>
        /// Raised when the boss is activated (arena entered). Args: displayName, fraction.
        /// </summary>
        public event Action<string, float> Activated;

        /// <summary>
        /// Raised when the boss phase changes. Args: phase.
        /// </summary>
        public event Action<BossPhase> PhaseChanged;

        /// <inheritdoc />
        public bool IsAlive => _model != null && !_model.IsDefeated;

        /// <summary>
        /// Display name shown on the boss bar.
        /// </summary>
        public string DisplayName => _displayName;

        private void Awake()
        {
            EnsureModel();
        }

        /// <summary>
        /// Activates the boss (called by the arena trigger). Shows the boss bar.
        /// </summary>
        public void Activate()
        {
            EnsureModel();
            if (_active)
            {
                return;
            }

            _active = true;
            Activated?.Invoke(_displayName, _model.HealthFraction);
        }

        /// <inheritdoc />
        public void TakeDamage(DamageInfo damageInfo)
        {
            EnsureModel();
            if (_model.IsDefeated)
            {
                return;
            }

            // Auto-activate on first hit so the boss can be fought even before the trigger fires.
            if (!_active)
            {
                Activate();
            }

            _model.ApplyDamage(damageInfo);
        }

        private void Update()
        {
            if (!_active || _model == null || _model.IsDefeated)
            {
                return;
            }

            BossAction action = _model.Tick(Time.deltaTime);
            // Locomotion/attack playback for the chosen action is wired by the scene
            // builder via animation hooks; the model owns the decision + cadence.
            _ = action;
        }

        private void EnsureModel()
        {
            if (_model != null)
            {
                return;
            }

            _model = new MutatedWolfBossModel(
                _maxHealth,
                _enrageHealthFraction,
                _lungeCooldown,
                new SystemRandomSource());

            _model.HealthChanged += fraction => HealthChanged?.Invoke(_displayName, fraction);
            _model.PhaseChanged += phase => PhaseChanged?.Invoke(phase);
            _model.Defeated += OnDefeated;
        }

        private void OnDefeated()
        {
            GameSignals.RaiseBossDefeated(_bossId);
        }
    }
}
