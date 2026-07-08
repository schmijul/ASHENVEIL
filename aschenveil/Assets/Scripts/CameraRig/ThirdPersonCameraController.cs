using UnityEngine;
using UnityEngine.InputSystem;

namespace Ashenveil.CameraRig
{
    /// <summary>
    /// MonoBehaviour adapter for third-person camera follow, orbit, shoulder offset, and collision.
    /// Referenced GDD section: Kernsysteme / Movement/Camera.
    /// </summary>
    public sealed class ThirdPersonCameraController : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private float _minPitch = -35f;
        [SerializeField] private float _maxPitch = 65f;
        [SerializeField] private float _defaultPitch = 20f;
        [SerializeField] private float _minZoom = 2f;
        [SerializeField] private float _maxZoom = 6f;
        [SerializeField] private float _defaultZoom = 4.5f;
        [SerializeField] private float _yawSensitivity = 0.18f;
        [SerializeField] private float _pitchSensitivity = 0.16f;
        [SerializeField] private float _zoomSensitivity = 0.2f;
        [SerializeField] private float _targetHeight = 1.55f;
        [SerializeField] private Vector3 _shoulderOffset = new Vector3(0.45f, 0f, 0f);
        [SerializeField] private float _collisionRadius = 0.22f;
        [SerializeField] private float _collisionPadding = 0.25f;
        [SerializeField] private float _minCollisionBoomLength = 0.35f;
        [SerializeField] private LayerMask _collisionLayers = ~0;

        [Header("References")]
        [SerializeField] private Transform _target;

        private ThirdPersonCameraModel _cameraModel;
        private InputAction _lookAction;
        private InputAction _zoomAction;

        private void Awake()
        {
            if (_target == null)
            {
                Debug.LogError("ThirdPersonCameraController requires a target transform reference.", this);
                enabled = false;
                return;
            }

            _cameraModel = new ThirdPersonCameraModel(new ThirdPersonCameraModel.Settings
            {
                MinPitch = _minPitch,
                MaxPitch = _maxPitch,
                DefaultPitch = _defaultPitch,
                MinZoom = _minZoom,
                MaxZoom = _maxZoom,
                DefaultZoom = _defaultZoom,
                YawSensitivity = _yawSensitivity,
                PitchSensitivity = _pitchSensitivity,
                ZoomSensitivity = _zoomSensitivity,
                CollisionPadding = _collisionPadding,
                MinCollisionBoomLength = _minCollisionBoomLength
            });

            _lookAction = new InputAction("CameraLook", InputActionType.Value, "<Mouse>/delta", expectedControlType: "Vector2");
            _zoomAction = new InputAction("CameraZoom", InputActionType.Value, "<Mouse>/scroll/y", expectedControlType: "Axis");
        }

        private void OnEnable()
        {
            _lookAction?.Enable();
            _zoomAction?.Enable();
        }

        private void OnDisable()
        {
            _lookAction?.Disable();
            _zoomAction?.Disable();
        }

        private void OnDestroy()
        {
            _lookAction?.Dispose();
            _zoomAction?.Dispose();
        }

        private void LateUpdate()
        {
            if (_cameraModel == null || _target == null)
            {
                return;
            }

            _cameraModel.AddOrbitDelta(_lookAction.ReadValue<Vector2>());
            _cameraModel.AddZoomDelta(_zoomAction.ReadValue<float>());

            Quaternion yawRotation = Quaternion.Euler(0f, _cameraModel.Yaw, 0f);
            Quaternion cameraRotation = Quaternion.Euler(_cameraModel.Pitch, _cameraModel.Yaw, 0f);
            Vector3 pivot = _target.position + Vector3.up * _targetHeight + yawRotation * _shoulderOffset;
            Vector3 boomDirection = cameraRotation * Vector3.back;
            float boomLength = GetCollisionCorrectedBoomLength(pivot, boomDirection, _cameraModel.Zoom);

            transform.SetPositionAndRotation(pivot + boomDirection * boomLength, cameraRotation);
        }

        private float GetCollisionCorrectedBoomLength(Vector3 pivot, Vector3 boomDirection, float desiredBoomLength)
        {
            if (Physics.SphereCast(
                pivot,
                _collisionRadius,
                boomDirection,
                out RaycastHit hit,
                desiredBoomLength,
                _collisionLayers,
                QueryTriggerInteraction.Ignore))
            {
                return _cameraModel.CalculateSafeBoomLength(desiredBoomLength, hit.distance);
            }

            return _cameraModel.CalculateSafeBoomLength(desiredBoomLength, 0f);
        }
    }
}
