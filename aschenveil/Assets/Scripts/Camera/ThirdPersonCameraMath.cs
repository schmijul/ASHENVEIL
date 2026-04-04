using UnityEngine;

namespace Ashenveil.Camera
{
    /// <summary>
    /// Pure helper math for orbit camera placement and pitch limits.
    /// Referenced GDD sections: 5.3
    /// </summary>
    public static class ThirdPersonCameraMath
    {
        public static float ClampPitch(float pitch, float minPitch, float maxPitch)
        {
            return Mathf.Clamp(pitch, minPitch, maxPitch);
        }

        public static Quaternion GetOrbitRotation(float yaw, float pitch)
        {
            return Quaternion.Euler(pitch, yaw, 0f);
        }

        public static Vector3 GetPivotPosition(Vector3 targetPosition, Vector3 targetRight, float heightOffset, float shoulderOffset)
        {
            Vector3 shoulderDirection = Vector3.ProjectOnPlane(targetRight, Vector3.up);
            if (shoulderDirection.sqrMagnitude <= 0f)
            {
                shoulderDirection = Vector3.right;
            }

            shoulderDirection.Normalize();
            return targetPosition + (Vector3.up * heightOffset) + (shoulderDirection * shoulderOffset);
        }

        public static Vector3 GetOrbitPosition(Vector3 pivotPosition, Quaternion orbitRotation, float followDistance)
        {
            return pivotPosition - (orbitRotation * Vector3.forward * followDistance);
        }

        public static float GetAdjustedFollowDistance(float currentDistance, float scrollDelta, float adjustmentSpeed, float minDistance, float maxDistance)
        {
            float normalizedScrollDelta = Mathf.Abs(scrollDelta) > 1f ? scrollDelta / 120f : scrollDelta;
            normalizedScrollDelta = Mathf.Clamp(normalizedScrollDelta, -1f, 1f);
            float adjustedDistance = currentDistance - (normalizedScrollDelta * adjustmentSpeed);
            return Mathf.Clamp(adjustedDistance, minDistance, maxDistance);
        }
    }
}
