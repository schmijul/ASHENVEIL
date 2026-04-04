using UnityEngine;

namespace Ashenveil.Player
{
    /// <summary>
    /// Third-person movement using CharacterController and camera-relative input.
    /// Referenced GDD sections: 5.2
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovementController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerInputHandler _inputHandler;
        [SerializeField] private PlayerStats _playerStats;
        [SerializeField] private StaminaSystem _staminaSystem;
        [SerializeField] private Transform _cameraTransform;

        private CharacterController _characterController;
        private Vector2 _moveInput;
        private bool _jumpQueued;
        private float _verticalVelocity;

        private void Awake()
        {
            if (!TryGetComponent(out _characterController))
            {
                Debug.LogError($"{nameof(PlayerMovementController)} on {name} requires a {nameof(CharacterController)}.");
                enabled = false;
                return;
            }

            ResolveReferences();
            ApplyStatsToCharacterController();
        }

        private void OnEnable()
        {
            if (_inputHandler != null)
            {
                _inputHandler.MoveInputChanged += HandleMoveInputChanged;
                _inputHandler.JumpPressed += HandleJumpPressed;
            }
        }

        private void Update()
        {
            if (_characterController == null || _playerStats == null)
            {
                return;
            }

            float deltaTime = Time.deltaTime;
            bool grounded = CheckGrounded();

            if (grounded && _verticalVelocity < 0f)
            {
                _verticalVelocity = -2f;
            }

            Vector3 cameraForward = _cameraTransform != null ? _cameraTransform.forward : transform.forward;
            Vector3 cameraRight = _cameraTransform != null ? _cameraTransform.right : transform.right;
            Vector3 moveDirection = PlayerMovementMath.GetCameraRelativeMovement(_moveInput, cameraForward, cameraRight);
            float moveMagnitude = Mathf.Clamp01(_moveInput.magnitude);
            bool sprinting = _staminaSystem != null && _staminaSystem.CanSprint && moveMagnitude > 0.01f && _inputHandler != null && _inputHandler.IsSprinting;

            float speed = PlayerMovementMath.GetMoveSpeed(sprinting, sprinting, _playerStats.WalkSpeed, _playerStats.SprintSpeed, _moveInput);
            if (sprinting && _staminaSystem != null)
            {
                if (!_staminaSystem.TryConsumeSprint(deltaTime))
                {
                    speed = _playerStats.WalkSpeed;
                }
            }

            if (_jumpQueued && grounded)
            {
                _verticalVelocity = PlayerMovementMath.CalculateJumpVelocity(_playerStats.Gravity, _playerStats.JumpHeight);
            }

            _jumpQueued = false;
            _verticalVelocity += _playerStats.Gravity * deltaTime;

            Vector3 horizontalVelocity = moveDirection * speed;
            Vector3 totalVelocity = horizontalVelocity + (Vector3.up * _verticalVelocity);
            _characterController.Move(totalVelocity * deltaTime);

            if (moveDirection.sqrMagnitude > 0.0001f)
            {
                transform.rotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            }
        }

        private void OnDisable()
        {
            if (_inputHandler != null)
            {
                _inputHandler.MoveInputChanged -= HandleMoveInputChanged;
                _inputHandler.JumpPressed -= HandleJumpPressed;
            }
        }

        private void OnValidate()
        {
            ResolveReferences();
        }

        private void ResolveReferences()
        {
            if (_playerStats == null)
            {
                _playerStats = PlayerStats.CreateRuntimeDefaults();
            }

            if (_staminaSystem == null && TryGetComponent(out StaminaSystem staminaSystem))
            {
                _staminaSystem = staminaSystem;
            }
        }

        private void ApplyStatsToCharacterController()
        {
            if (_characterController == null || _playerStats == null)
            {
                return;
            }

            _characterController.height = _playerStats.CharacterHeight;
            _characterController.radius = _playerStats.CharacterRadius;
            _characterController.slopeLimit = _playerStats.SlopeLimit;
            _characterController.stepOffset = _playerStats.StepOffset;
        }

        private bool CheckGrounded()
        {
            Vector3 origin = transform.position + Vector3.up * 0.1f;
            float castDistance = _playerStats.GroundCheckDistance;
            return Physics.SphereCast(
                origin,
                _playerStats.GroundCheckRadius,
                Vector3.down,
                out _,
                castDistance,
                _playerStats.GroundLayers,
                QueryTriggerInteraction.Ignore);
        }

        private void HandleMoveInputChanged(Vector2 value)
        {
            _moveInput = value;
        }

        private void HandleJumpPressed()
        {
            _jumpQueued = true;
        }
    }
}
