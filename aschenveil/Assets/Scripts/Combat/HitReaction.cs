using System;
using Ashenveil.Core;
using UnityEngine;

namespace Ashenveil.Combat
{
    /// <summary>
    /// Helper for basic knockback application and event-only hit flash hooks.
    /// Referenced GDD section: Kernsysteme / Combat.
    /// </summary>
    public static class HitReaction
    {
        /// <summary>
        /// Raised when a target should display a hit flash. Args: target, damage info.
        /// </summary>
        public static event Action<GameObject, DamageInfo> HitFlashRequested;

        /// <summary>
        /// Applies a horizontal knockback impulse through a CharacterController.
        /// </summary>
        public static void ApplyKnockback(CharacterController controller, DamageInfo damageInfo)
        {
            if (controller == null || damageInfo.Knockback <= 0f)
            {
                return;
            }

            Vector3 direction = controller.transform.position - damageInfo.SourcePosition;
            direction.y = 0f;
            if (direction.sqrMagnitude <= 0.0001f)
            {
                direction = controller.transform.forward;
            }

            controller.Move(direction.normalized * damageInfo.Knockback * Time.deltaTime);
        }

        /// <summary>
        /// Applies a horizontal knockback impulse through a Rigidbody.
        /// </summary>
        public static void ApplyKnockback(Rigidbody rigidbody, DamageInfo damageInfo)
        {
            if (rigidbody == null || damageInfo.Knockback <= 0f)
            {
                return;
            }

            Vector3 direction = rigidbody.worldCenterOfMass - damageInfo.SourcePosition;
            direction.y = 0f;
            if (direction.sqrMagnitude <= 0.0001f)
            {
                direction = rigidbody.transform.forward;
            }

            rigidbody.AddForce(direction.normalized * damageInfo.Knockback, ForceMode.Impulse);
        }

        /// <summary>
        /// Raises a hit flash request without rendering anything directly.
        /// </summary>
        public static void RequestHitFlash(GameObject target, DamageInfo damageInfo)
        {
            if (target == null)
            {
                return;
            }

            HitFlashRequested?.Invoke(target, damageInfo);
        }
    }
}
