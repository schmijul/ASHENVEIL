using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ashenveil.Combat
{
    /// <summary>
    /// Trigger-based weapon hitbox that guarantees one hit per target per swing.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class WeaponHitbox : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Collider _hitboxCollider;

        private readonly HashSet<IDamageable> _hitTargets = new HashSet<IDamageable>();
        private bool _isSwingActive;

        public event Action<IDamageable, Collider> TargetHit;

        public bool IsSwingActive => _isSwingActive;

        private void Awake()
        {
            ResolveReferences();
            ConfigureCollider(false);
        }

        private void OnDisable()
        {
            EndSwing();
        }

        private void OnValidate()
        {
            ResolveReferences();
            if (_hitboxCollider != null)
            {
                _hitboxCollider.isTrigger = true;
            }
        }

        public void BeginSwing()
        {
            _hitTargets.Clear();
            _isSwingActive = true;
            ConfigureCollider(true);
        }

        public void EndSwing()
        {
            _isSwingActive = false;
            _hitTargets.Clear();
            ConfigureCollider(false);
        }

        private void OnTriggerEnter(Collider other)
        {
            TryRegisterHit(other);
        }

        private void OnTriggerStay(Collider other)
        {
            TryRegisterHit(other);
        }

        private void ResolveReferences()
        {
            if (_hitboxCollider == null)
            {
                TryGetComponent(out _hitboxCollider);
            }

            if (_hitboxCollider == null)
            {
                _hitboxCollider = GetComponentInChildren<Collider>(true);
            }
        }

        private void ConfigureCollider(bool enabled)
        {
            if (_hitboxCollider == null)
            {
                return;
            }

            _hitboxCollider.isTrigger = true;
            _hitboxCollider.enabled = enabled;
        }

        private void TryRegisterHit(Collider other)
        {
            if (!_isSwingActive || other == null)
            {
                return;
            }

            if (other.transform.root == transform.root)
            {
                return;
            }

            if (!TryResolveDamageable(other, out IDamageable damageable))
            {
                return;
            }

            if (damageable == null || damageable.IsDead || _hitTargets.Contains(damageable))
            {
                return;
            }

            _hitTargets.Add(damageable);
            TargetHit?.Invoke(damageable, other);
        }

        private static bool TryResolveDamageable(Collider other, out IDamageable damageable)
        {
            damageable = null;

            MonoBehaviour[] behaviours = other.GetComponentsInParent<MonoBehaviour>(true);
            for (int index = 0; index < behaviours.Length; index++)
            {
                if (behaviours[index] is IDamageable candidate)
                {
                    damageable = candidate;
                    return true;
                }
            }

            return false;
        }
    }
}
