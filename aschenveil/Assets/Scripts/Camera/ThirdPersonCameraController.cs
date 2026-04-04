using UnityEngine;
using Ashenveil.Player;

namespace Ashenveil.Camera
{
    /// <summary>
    /// Third-person over-the-shoulder camera with orbit, smoothing and collision.
    /// Referenced GDD sections: 5.3
    /// </summary>
    [RequireComponent(typeof(global::UnityEngine.Camera))]
    public class ThirdPersonCameraController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform _target;
        [SerializeField] private PlayerInputHandler _inputHandler;

        [Header("Orbit")]
        [SerializeField, Range(1.5f, 5f)] private float _followDistance = 2.5f;
        [SerializeField, Range(0f, 10f)] private float _distanceAdjustmentSpeed = 1.5f;
        [SerializeField, Min(0f)] private float _heightOffset = 1.5f;
        [SerializeField, Min(0f)] private float _shoulderOffset = 0.5f;
        [SerializeField, Min(0.01f)] private float _lookSensitivity = 2f;
        [SerializeField] private float _minPitch = -30f;
        [SerializeField] private float _maxPitch = 60f;

        [Header("Smoothing")]
        [SerializeField, Min(0f)] private float _positionLerpSpeed = 10f;
        [SerializeField, Min(0f)] private float _rotationLerpSpeed = 15f;

        [Header("Collision")]
        [SerializeField, Min(0f)] private float _collisionRadius = 0.2f;
        [SerializeField] private LayerMask _collisionMask = ~0;

        private global::UnityEngine.Camera _camera;
        private float _yaw;
        private float _pitch;

        private void Awake()
        {
            if (!TryGetComponent(out _camera))
            {
                Debug.LogError($"{nameof(ThirdPersonCameraController)} on {name} requires a Camera component.");
                enabled = false;
                return;
            }

            _yaw = transform.eulerAngles.y;
            _pitch = ClampPitch(GetSignedPitch(transform.eulerAngles.x), _minPitch, _maxPitch);
        }

        private void OnEnable()
        {
            if (_inputHandler != null)
            {
                _inputHandler.LookInputChanged += HandleLookInputChanged;
                _inputHandler.ZoomInputChanged += HandleZoomInputChanged;
            }
        }

        private void LateUpdate()
        {
            if (_target == null)
            {
                return;
            }

            Vector3 targetRight = _target.right;
            Vector3 pivotPosition = ThirdPersonCameraMath.GetPivotPosition(_target.position, targetRight, _heightOffset, _shoulderOffset);
            Quaternion orbitRotation = ThirdPersonCameraMath.GetOrbitRotation(_yaw, _pitch);
            Vector3 desiredPosition = ThirdPersonCameraMath.GetOrbitPosition(pivotPosition, orbitRotation, _followDistance);
            Vector3 adjustedPosition = ResolveCollision(pivotPosition, desiredPosition);

            transform.position = Vector3.Lerp(transform.position, adjustedPosition, Time.deltaTime * _positionLerpSpeed);

            Quaternion desiredRotation = Quaternion.LookRotation(pivotPosition - transform.position, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, Time.deltaTime * _rotationLerpSpeed);
        }

        private void OnDisable()
        {
            if (_inputHandler != null)
            {
                _inputHandler.LookInputChanged -= HandleLookInputChanged;
                _inputHandler.ZoomInputChanged -= HandleZoomInputChanged;
            }
        }

        private void OnValidate()
        {
            _followDistance = Mathf.Clamp(_followDistance, 1.5f, 5f);
            _pitch = ClampPitch(_pitch, _minPitch, _maxPitch);
        }

        public void AdjustFollowDistance(float delta)
        {
            _followDistance = ThirdPersonCameraMath.GetAdjustedFollowDistance(
                _followDistance,
                delta,
                _distanceAdjustmentSpeed,
                1.5f,
                5f);
        }

        private void HandleLookInputChanged(Vector2 lookDelta)
        {
            _yaw += lookDelta.x * _lookSensitivity;
            _pitch = ClampPitch(_pitch - (lookDelta.y * _lookSensitivity), _minPitch, _maxPitch);
        }

        private void HandleZoomInputChanged(float zoomDelta)
        {
            AdjustFollowDistance(zoomDelta);
        }

        private Vector3 ResolveCollision(Vector3 pivotPosition, Vector3 desiredPosition)
        {
            Vector3 toCamera = desiredPosition - pivotPosition;
            float distance = toCamera.magnitude;
            if (distance <= 0.0001f)
            {
                return desiredPosition;
            }

            Vector3 direction = toCamera / distance;
            if (Physics.SphereCast(pivotPosition, _collisionRadius, direction, out RaycastHit hit, distance, _collisionMask, QueryTriggerInteraction.Ignore))
            {
                return hit.point + (hit.normal * _collisionRadius);
            }

            return desiredPosition;
        }

        private static float ClampPitch(float pitch, float minPitch, float maxPitch)
        {
            return ThirdPersonCameraMath.ClampPitch(pitch, minPitch, maxPitch);
        }

        private static float GetSignedPitch(float xRotation)
        {
            return xRotation > 180f ? xRotation - 360f : xRotation;
        }
    }
}
