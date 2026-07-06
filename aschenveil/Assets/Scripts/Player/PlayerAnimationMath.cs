using UnityEngine;

namespace Ashenveil.Player
{
    /// <summary>
    /// Pure animation parameter math for player locomotion.
    /// Referenced GDD section: Kernsysteme / Movement/Camera.
    /// </summary>
    public static class PlayerAnimationMath
    {
        /// <summary>
        /// Converts raw planar speed to a damped normalized animation speed.
        /// </summary>
        public static float DampNormalizedSpeed(
            float currentNormalizedSpeed,
            float rawSpeed,
            float maxSpeed,
            float damping,
            float deltaTime)
        {
            float current = Mathf.Clamp01(currentNormalizedSpeed);
            float maxDelta = Mathf.Max(0f, damping) * Mathf.Max(0f, deltaTime);

            if (maxSpeed <= 0f)
            {
                return Mathf.MoveTowards(current, 0f, maxDelta);
            }

            float target = Mathf.Clamp01(Mathf.Max(0f, rawSpeed) / maxSpeed);
            return Mathf.MoveTowards(current, target, maxDelta);
        }
    }
}
