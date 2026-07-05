using UnityEngine;

namespace Ashenveil.Player
{
    /// <summary>
    /// Deterministic third-person movement model for grounded movement, jumping, dodging, and gravity.
    /// Referenced GDD section: Kernsysteme / Movement/Camera.
    /// </summary>
    public sealed class PlayerMovementModel
    {
        private const float DirectionEpsilon = 0.0001f;

        private readonly Settings _settings;
        private Vector3 _horizontalVelocity;
        private float _verticalVelocity;
        private float _yaw;
        private float _currentSpeed;
        private float _timeSinceGrounded;
        private bool _jumpConsumed;
        private bool _isDodging;
        private float _dodgeElapsed;
        private Vector3 _dodgeDirection;

        /// <summary>
        /// Creates a movement model with explicit tunables.
        /// </summary>
        public PlayerMovementModel(Settings settings)
        {
            _settings = settings.Normalized();
        }

        /// <summary>
        /// Current movement-facing yaw in degrees.
        /// </summary>
        public float Yaw => _yaw;

        /// <summary>
        /// Indicates whether a dodge roll is currently active.
        /// </summary>
        public bool IsDodging => _isDodging;

        /// <summary>
        /// Advances movement simulation by one frame.
        /// </summary>
        public State Tick(Input input, float deltaTime)
        {
            deltaTime = Mathf.Max(0f, deltaTime);
            Vector3 moveDirection = FlattenAndNormalize(input.MoveDirection);

            UpdateGrounding(input.IsGrounded, deltaTime);
            TryStartJump(input.JumpPressed);
            TryStartDodge(input.DodgePressed, moveDirection);

            bool hasIFrames = false;
            if (_isDodging)
            {
                _dodgeElapsed += deltaTime;
                hasIFrames = _dodgeElapsed >= _settings.DodgeIFrameStart && _dodgeElapsed <= _settings.DodgeIFrameEnd;
                _horizontalVelocity = _dodgeDirection * _settings.DodgeSpeed;

                if (_dodgeElapsed >= _settings.DodgeDuration)
                {
                    _isDodging = false;
                    hasIFrames = false;
                }
            }
            else
            {
                UpdateGroundMovement(input, moveDirection, deltaTime);
            }

            ApplyGravity(input.IsGrounded, deltaTime);

            return new State
            {
                Velocity = _horizontalVelocity + Vector3.up * _verticalVelocity,
                CurrentSpeed = _currentSpeed,
                Yaw = _yaw,
                IsGrounded = input.IsGrounded,
                IsDodging = _isDodging,
                HasIFrames = hasIFrames
            };
        }

        private void UpdateGrounding(bool isGrounded, float deltaTime)
        {
            if (isGrounded)
            {
                _timeSinceGrounded = 0f;
                _jumpConsumed = false;
                return;
            }

            _timeSinceGrounded += deltaTime;
        }

        private void TryStartJump(bool jumpPressed)
        {
            if (!jumpPressed || _jumpConsumed || _timeSinceGrounded > _settings.CoyoteTime)
            {
                return;
            }

            _verticalVelocity = Mathf.Sqrt(_settings.JumpHeight * -2f * _settings.Gravity);
            _jumpConsumed = true;
            _timeSinceGrounded = _settings.CoyoteTime + 1f;
        }

        private void TryStartDodge(bool dodgePressed, Vector3 moveDirection)
        {
            if (!dodgePressed || _isDodging || moveDirection.sqrMagnitude <= DirectionEpsilon)
            {
                return;
            }

            _isDodging = true;
            _dodgeElapsed = 0f;
            _dodgeDirection = moveDirection;
        }

        private void UpdateGroundMovement(Input input, Vector3 moveDirection, float deltaTime)
        {
            float targetSpeed = GetTargetSpeed(input, moveDirection);
            _currentSpeed = Mathf.MoveTowards(_currentSpeed, targetSpeed, _settings.Acceleration * deltaTime);
            _horizontalVelocity = moveDirection * _currentSpeed;

            if (moveDirection.sqrMagnitude > DirectionEpsilon)
            {
                float targetYaw = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;
                _yaw = Mathf.LerpAngle(_yaw, targetYaw, Mathf.Clamp01(_settings.RotationSmoothing * deltaTime));
            }
        }

        private float GetTargetSpeed(Input input, Vector3 moveDirection)
        {
            if (moveDirection.sqrMagnitude <= DirectionEpsilon)
            {
                return 0f;
            }

            if (input.WalkHeld)
            {
                return _settings.WalkSpeed;
            }

            return input.SprintHeld ? _settings.SprintSpeed : _settings.RunSpeed;
        }

        private void ApplyGravity(bool isGrounded, float deltaTime)
        {
            if (isGrounded && _verticalVelocity <= 0f)
            {
                _verticalVelocity = _settings.GroundedGravity;
                return;
            }

            _verticalVelocity += _settings.Gravity * deltaTime;
        }

        private static Vector3 FlattenAndNormalize(Vector3 direction)
        {
            direction.y = 0f;
            return direction.sqrMagnitude > DirectionEpsilon ? direction.normalized : Vector3.zero;
        }

        /// <summary>
        /// Tunables used by the movement model.
        /// </summary>
        public struct Settings
        {
            /// <summary>
            /// Conservative movement speed.
            /// </summary>
            public float WalkSpeed { get; set; }

            /// <summary>
            /// Default movement speed.
            /// </summary>
            public float RunSpeed { get; set; }

            /// <summary>
            /// Fast movement speed gated by stamina.
            /// </summary>
            public float SprintSpeed { get; set; }

            /// <summary>
            /// Horizontal speed change rate.
            /// </summary>
            public float Acceleration { get; set; }

            /// <summary>
            /// Yaw interpolation strength toward movement direction.
            /// </summary>
            public float RotationSmoothing { get; set; }

            /// <summary>
            /// Jump apex height in meters.
            /// </summary>
            public float JumpHeight { get; set; }

            /// <summary>
            /// Downward acceleration in meters per second squared.
            /// </summary>
            public float Gravity { get; set; }

            /// <summary>
            /// Small downward velocity kept while grounded.
            /// </summary>
            public float GroundedGravity { get; set; }

            /// <summary>
            /// Time after leaving ground where jump is still accepted.
            /// </summary>
            public float CoyoteTime { get; set; }

            /// <summary>
            /// Total dodge roll duration.
            /// </summary>
            public float DodgeDuration { get; set; }

            /// <summary>
            /// Horizontal dodge speed.
            /// </summary>
            public float DodgeSpeed { get; set; }

            /// <summary>
            /// Seconds after dodge start where invulnerability begins.
            /// </summary>
            public float DodgeIFrameStart { get; set; }

            /// <summary>
            /// Seconds after dodge start where invulnerability ends.
            /// </summary>
            public float DodgeIFrameEnd { get; set; }

            /// <summary>
            /// Sensible default movement settings for the demo player.
            /// </summary>
            public static Settings Default => new Settings
            {
                WalkSpeed = 1.8f,
                RunSpeed = 4.2f,
                SprintSpeed = 6.2f,
                Acceleration = 18f,
                RotationSmoothing = 12f,
                JumpHeight = 1.1f,
                Gravity = -24f,
                GroundedGravity = -2f,
                CoyoteTime = 0.12f,
                DodgeDuration = 0.45f,
                DodgeSpeed = 8f,
                DodgeIFrameStart = 0.08f,
                DodgeIFrameEnd = 0.28f
            };

            /// <summary>
            /// Returns settings clamped to usable values.
            /// </summary>
            public Settings Normalized()
            {
                Settings settings = this;
                settings.WalkSpeed = Mathf.Max(0f, WalkSpeed);
                settings.RunSpeed = Mathf.Max(settings.WalkSpeed, RunSpeed);
                settings.SprintSpeed = Mathf.Max(settings.RunSpeed, SprintSpeed);
                settings.Acceleration = Mathf.Max(0f, Acceleration);
                settings.RotationSmoothing = Mathf.Max(0f, RotationSmoothing);
                settings.JumpHeight = Mathf.Max(0f, JumpHeight);
                settings.Gravity = Gravity < 0f ? Gravity : -Mathf.Max(0.01f, Gravity);
                settings.GroundedGravity = GroundedGravity <= 0f ? GroundedGravity : -GroundedGravity;
                settings.CoyoteTime = Mathf.Max(0f, CoyoteTime);
                settings.DodgeDuration = Mathf.Max(0.01f, DodgeDuration);
                settings.DodgeSpeed = Mathf.Max(0f, DodgeSpeed);
                settings.DodgeIFrameStart = Mathf.Clamp(DodgeIFrameStart, 0f, settings.DodgeDuration);
                settings.DodgeIFrameEnd = Mathf.Clamp(Mathf.Max(settings.DodgeIFrameStart, DodgeIFrameEnd), 0f, settings.DodgeDuration);
                return settings;
            }
        }

        /// <summary>
        /// Input sample consumed by one movement tick.
        /// </summary>
        public struct Input
        {
            /// <summary>
            /// Desired world-space movement direction.
            /// </summary>
            public Vector3 MoveDirection { get; set; }

            /// <summary>
            /// Whether the player requests walking speed.
            /// </summary>
            public bool WalkHeld { get; set; }

            /// <summary>
            /// Whether the player requests sprint speed.
            /// </summary>
            public bool SprintHeld { get; set; }

            /// <summary>
            /// Whether jump was pressed this tick.
            /// </summary>
            public bool JumpPressed { get; set; }

            /// <summary>
            /// Whether dodge was pressed this tick.
            /// </summary>
            public bool DodgePressed { get; set; }

            /// <summary>
            /// Whether the controller is currently grounded.
            /// </summary>
            public bool IsGrounded { get; set; }
        }

        /// <summary>
        /// Movement output produced by one simulation tick.
        /// </summary>
        public struct State
        {
            /// <summary>
            /// World-space velocity to apply this tick.
            /// </summary>
            public Vector3 Velocity { get; set; }

            /// <summary>
            /// Current horizontal speed.
            /// </summary>
            public float CurrentSpeed { get; set; }

            /// <summary>
            /// Movement-facing yaw in degrees.
            /// </summary>
            public float Yaw { get; set; }

            /// <summary>
            /// Whether the controller reported grounded this tick.
            /// </summary>
            public bool IsGrounded { get; set; }

            /// <summary>
            /// Whether the dodge roll is active after this tick.
            /// </summary>
            public bool IsDodging { get; set; }

            /// <summary>
            /// Whether the player is inside the dodge invulnerability window.
            /// </summary>
            public bool HasIFrames { get; set; }
        }
    }
}
