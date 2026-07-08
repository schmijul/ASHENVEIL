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

        [Header("Chase & attack")]
        [SerializeField] private Transform _target;
        [SerializeField] private float _moveSpeed = 3.2f;
        [SerializeField] private float _enrageSpeedMultiplier = 1.6f;
        [SerializeField] private float _attackRange = 2.6f;
        [SerializeField] private float _attackDamage = 14f;
        [SerializeField] private float _attackKnockback = 4f;

        private MutatedWolfBossModel _model;
        private bool _active;
        private IDamageable _targetDamageable;

        /// <summary>
        /// Assigns the pursuit target (the player) and its damageable, wired by the scene builder.
        /// </summary>
        public void SetTarget(Transform target, IDamageable targetDamageable)
        {
            _target = target;
            _targetDamageable = targetDamageable;
        }

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

            if (_target == null)
            {
                return;
            }

            if (_targetDamageable == null)
            {
                _targetDamageable = _target.GetComponentInParent<IDamageable>();
            }

            Vector3 toTarget = _target.position - transform.position;
            toTarget.y = 0f;
            float distance = toTarget.magnitude;

            // Face and close on the player.
            if (distance > 0.01f)
            {
                Vector3 dir = toTarget / distance;
                transform.rotation = Quaternion.Slerp(
                    transform.rotation, Quaternion.LookRotation(dir), 6f * Time.deltaTime);

                if (distance > _attackRange)
                {
                    float speed = _model.Phase == BossPhase.Enrage ? _moveSpeed * _enrageSpeedMultiplier : _moveSpeed;
                    transform.position += dir * speed * Time.deltaTime;
                }
            }

            // On a chosen attack within range, hit the player.
            if (action != BossAction.None && distance <= _attackRange && _targetDamageable != null && _targetDamageable.IsAlive)
            {
                float dmg = action == BossAction.Lunge ? _attackDamage * 1.5f : _attackDamage;
                _targetDamageable.TakeDamage(new DamageInfo(dmg, DamageType.Physical, transform.position, _attackKnockback));
            }
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
