using UnityEngine;

namespace Ashenveil.Player
{
    /// <summary>
    /// Drives Mecanim player parameters from movement and combat input events.
    /// Referenced GDD section: Kernsysteme / Movement/Camera.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PlayerAnimationController : MonoBehaviour
    {
        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int GroundedHash = Animator.StringToHash("Grounded");
        private static readonly int DodgeHash = Animator.StringToHash("Dodge");
        private static readonly int AttackLightHash = Animator.StringToHash("AttackLight");
        private static readonly int AttackHeavyHash = Animator.StringToHash("AttackHeavy");

        [Header("Configuration")]
        [SerializeField] private float _sprintSpeed = 6.2f;
        [SerializeField] private float _speedDamping = 10f;
        [SerializeField] private float _dodgeTriggerSpeed = 7f;

        [Header("References")]
        [SerializeField] private Animator _animator;
        [SerializeField] private PlayerMovementController _movementController;

        private CharacterController _characterController;
        private Vector3 _lastPosition;
        private float _normalizedSpeed;
        private bool _wasAboveDodgeSpeed;
        private bool _loggedMissingAnimator;
        private bool _subscribed;

        private void Awake()
        {
            if (_animator == null && !TryGetComponent(out _animator))
            {
                DisableMissingAnimator();
                return;
            }

            if (_movementController == null)
            {
                TryGetComponent(out _movementController);
            }

            TryGetComponent(out _characterController);
            _lastPosition = transform.position;
        }

        private void OnEnable()
        {
            _lastPosition = transform.position;
            _wasAboveDodgeSpeed = false;

            if (_animator == null)
            {
                DisableMissingAnimator();
                return;
            }

            SubscribeToMovementEvents();
        }

        private void OnDisable()
        {
            UnsubscribeFromMovementEvents();
        }

        private void Update()
        {
            if (_animator == null)
            {
                DisableMissingAnimator();
                return;
            }

            float deltaTime = Time.deltaTime;
            float rawSpeed = GetPlanarSpeed(deltaTime);
            _normalizedSpeed = PlayerAnimationMath.DampNormalizedSpeed(
                _normalizedSpeed,
                rawSpeed,
                _sprintSpeed,
                _speedDamping,
                deltaTime);

            _animator.SetFloat(SpeedHash, _normalizedSpeed);
            _animator.SetBool(GroundedHash, _characterController != null && _characterController.isGrounded);
            UpdateDodgeTrigger(rawSpeed);
        }

        private float GetPlanarSpeed(float deltaTime)
        {
            Vector3 currentPosition = transform.position;
            Vector3 delta = currentPosition - _lastPosition;
            delta.y = 0f;
            _lastPosition = currentPosition;

            return deltaTime > 0f ? delta.magnitude / deltaTime : 0f;
        }

        private void UpdateDodgeTrigger(float rawSpeed)
        {
            bool aboveDodgeSpeed = rawSpeed >= Mathf.Max(0f, _dodgeTriggerSpeed);
            if (aboveDodgeSpeed && !_wasAboveDodgeSpeed)
            {
                _animator.SetTrigger(DodgeHash);
            }

            _wasAboveDodgeSpeed = aboveDodgeSpeed;
        }

        private void SubscribeToMovementEvents()
        {
            if (_movementController == null || _subscribed)
            {
                return;
            }

            _movementController.LightAttackPressed += OnLightAttackPressed;
            _movementController.BlockOrHeavyPressed += OnBlockOrHeavyPressed;
            _subscribed = true;
        }

        private void UnsubscribeFromMovementEvents()
        {
            if (_movementController == null || !_subscribed)
            {
                return;
            }

            _movementController.LightAttackPressed -= OnLightAttackPressed;
            _movementController.BlockOrHeavyPressed -= OnBlockOrHeavyPressed;
            _subscribed = false;
        }

        private void OnLightAttackPressed()
        {
            _animator.SetTrigger(AttackLightHash);
        }

        private void OnBlockOrHeavyPressed()
        {
            _animator.SetTrigger(AttackHeavyHash);
        }

        private void DisableMissingAnimator()
        {
            if (!_loggedMissingAnimator)
            {
                Debug.LogError("PlayerAnimationController requires an Animator reference or same-object Animator.", this);
                _loggedMissingAnimator = true;
            }

            enabled = false;
        }
    }
}
