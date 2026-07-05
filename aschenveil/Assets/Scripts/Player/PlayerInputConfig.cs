using UnityEngine;

namespace Ashenveil.Player
{
    /// <summary>
    /// ScriptableObject holding player movement and stamina tunables.
    /// Referenced GDD section: Kernsysteme / Movement/Camera.
    /// </summary>
    [CreateAssetMenu(fileName = "NewPlayerInputConfig", menuName = "Ashenveil/Player/Input Config")]
    public sealed class PlayerInputConfig : ScriptableObject
    {
        [Header("Movement")]
        [SerializeField] private float _walkSpeed = 1.8f;
        [SerializeField] private float _runSpeed = 4.2f;
        [SerializeField] private float _sprintSpeed = 6.2f;
        [SerializeField] private float _acceleration = 18f;
        [SerializeField] private float _rotationSmoothing = 12f;
        [SerializeField] private float _jumpHeight = 1.1f;
        [SerializeField] private float _gravity = -24f;
        [SerializeField] private float _groundedGravity = -2f;
        [SerializeField] private float _coyoteTime = 0.12f;
        [SerializeField] private float _dodgeDuration = 0.45f;
        [SerializeField] private float _dodgeSpeed = 8f;
        [SerializeField] private float _dodgeIFrameStart = 0.08f;
        [SerializeField] private float _dodgeIFrameEnd = 0.28f;

        [Header("Stamina")]
        [SerializeField] private float _maxStamina = 100f;
        [SerializeField] private float _sprintDrainPerSecond = 18f;
        [SerializeField] private float _dodgeCost = 25f;
        [SerializeField] private float _lightAttackCost = 12f;
        [SerializeField] private float _heavyAttackCost = 28f;
        [SerializeField] private float _regenPerSecond = 22f;
        [SerializeField] private float _regenDelay = 0.8f;
        [SerializeField] private float _minimumSprintStamina = 0f;

        /// <summary>
        /// Creates movement settings from serialized values.
        /// </summary>
        public PlayerMovementModel.Settings ToMovementSettings()
        {
            return new PlayerMovementModel.Settings
            {
                WalkSpeed = _walkSpeed,
                RunSpeed = _runSpeed,
                SprintSpeed = _sprintSpeed,
                Acceleration = _acceleration,
                RotationSmoothing = _rotationSmoothing,
                JumpHeight = _jumpHeight,
                Gravity = _gravity,
                GroundedGravity = _groundedGravity,
                CoyoteTime = _coyoteTime,
                DodgeDuration = _dodgeDuration,
                DodgeSpeed = _dodgeSpeed,
                DodgeIFrameStart = _dodgeIFrameStart,
                DodgeIFrameEnd = _dodgeIFrameEnd
            };
        }

        /// <summary>
        /// Creates stamina settings from serialized values.
        /// </summary>
        public StaminaModel.Settings ToStaminaSettings()
        {
            return new StaminaModel.Settings
            {
                MaxStamina = _maxStamina,
                SprintDrainPerSecond = _sprintDrainPerSecond,
                DodgeCost = _dodgeCost,
                LightAttackCost = _lightAttackCost,
                HeavyAttackCost = _heavyAttackCost,
                RegenPerSecond = _regenPerSecond,
                RegenDelay = _regenDelay,
                MinimumSprintStamina = _minimumSprintStamina
            };
        }
    }
}
