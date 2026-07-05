using System.Collections.Generic;
using Ashenveil.Core;
using UnityEngine;

namespace Ashenveil.Combat
{
    /// <summary>
    /// Trigger hitbox that applies one damage payload once per target per swing.
    /// Referenced GDD section: Kernsysteme / Combat.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public sealed class WeaponHitbox : MonoBehaviour
    {
        private readonly HashSet<IDamageable> _hitTargets = new HashSet<IDamageable>();
        private Collider _collider;
        private DamageInfo _damageInfo;
        private bool _isActive;

        /// <summary>
        /// Whether this hitbox is inside an active swing.
        /// </summary>
        public bool IsActive => _isActive;

        private void Awake()
        {
            if (!TryGetComponent(out _collider))
            {
                Debug.LogError("WeaponHitbox requires a Collider.", this);
                enabled = false;
                return;
            }

            _collider.isTrigger = true;
            _collider.enabled = false;
        }

        /// <summary>
        /// Starts a new swing and clears previous target memory.
        /// </summary>
        public void BeginSwing(DamageInfo damageInfo)
        {
            _damageInfo = damageInfo;
            _hitTargets.Clear();
            _isActive = true;
            if (_collider != null)
            {
                _collider.enabled = true;
            }
        }

        /// <summary>
        /// Ends the active swing.
        /// </summary>
        public void EndSwing()
        {
            _isActive = false;
            if (_collider != null)
            {
                _collider.enabled = false;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            TryDamage(other);
        }

        private void OnTriggerStay(Collider other)
        {
            TryDamage(other);
        }

        private void TryDamage(Collider other)
        {
            if (!_isActive || other == null)
            {
                return;
            }

            IDamageable damageable = null;
            if (!other.TryGetComponent(out damageable))
            {
                damageable = other.GetComponentInParent<IDamageable>();
            }

            if (damageable == null || !damageable.IsAlive || _hitTargets.Contains(damageable))
            {
                return;
            }

            _hitTargets.Add(damageable);
            damageable.TakeDamage(_damageInfo);
        }
    }
}
