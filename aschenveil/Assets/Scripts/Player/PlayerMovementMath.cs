using UnityEngine;

namespace Ashenveil.Player
{
    /// <summary>
    /// Pure helper math for camera-relative movement and jump velocity.
    /// Referenced GDD sections: 5.2
    /// </summary>
    public static class PlayerMovementMath
    {
        public static Vector3 GetCameraRelativeMovement(Vector2 input, Vector3 cameraForward, Vector3 cameraRight)
        {
            if (input.sqrMagnitude <= 0f)
            {
                return Vector3.zero;
            }

            Vector3 forward = Vector3.ProjectOnPlane(cameraForward, Vector3.up);
            if (forward.sqrMagnitude <= 0f)
            {
                forward = Vector3.forward;
            }

            Vector3 right = Vector3.ProjectOnPlane(cameraRight, Vector3.up);
            if (right.sqrMagnitude <= 0f)
            {
                right = Vector3.right;
            }

            forward.Normalize();
            right.Normalize();

            Vector3 direction = (forward * input.y) + (right * input.x);
            return direction.sqrMagnitude > 1f ? direction.normalized : direction;
        }

        public static float CalculateJumpVelocity(float gravity, float jumpHeight)
        {
            return Mathf.Sqrt(2f * Mathf.Abs(gravity) * Mathf.Max(0f, jumpHeight));
        }

        public static float GetMoveSpeed(bool sprintHeld, bool canSprint, float walkSpeed, float sprintSpeed, Vector2 input)
        {
            if (input.sqrMagnitude <= 0.0001f)
            {
                return 0f;
            }

            return sprintHeld && canSprint ? sprintSpeed : walkSpeed;
        }
    }
}
