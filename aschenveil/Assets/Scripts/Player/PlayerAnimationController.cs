using UnityEngine;

namespace Ashenveil.Player
{
    /// <summary>
    /// Drives Animator parameters from player movement and input state.
    /// Referenced GDD sections: 5.2
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class PlayerAnimationController : MonoBehaviour
    {
        [SerializeField] private PlayerInputHandler _inputHandler;

        private Animator _animator;
        private CharacterController _characterController;

        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
        private static readonly int JumpHash = Animator.StringToHash("Jump");

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _characterController = GetComponentInParent<CharacterController>();
        }

        private void OnEnable()
        {
            if (_inputHandler != null)
            {
                _inputHandler.JumpPressed += HandleJumpPressed;
            }
        }

        private void Update()
        {
            if (_animator == null)
            {
                return;
            }

            float speed = 0f;
            if (_inputHandler != null)
            {
                float inputMagnitude = _inputHandler.MoveInput.magnitude;
                if (inputMagnitude > 0.01f)
                {
                    speed = _inputHandler.IsSprinting ? 2f : Mathf.Lerp(0.5f, 1f, inputMagnitude);
                }
            }

            _animator.SetFloat(SpeedHash, speed, 0.1f, Time.deltaTime);
            _animator.SetBool(IsGroundedHash, _characterController != null && _characterController.isGrounded);
        }

        private void OnDisable()
        {
            if (_inputHandler != null)
            {
                _inputHandler.JumpPressed -= HandleJumpPressed;
            }
        }

        private void HandleJumpPressed()
        {
            if (_animator != null && _characterController != null && _characterController.isGrounded)
            {
                _animator.SetTrigger(JumpHash);
            }
        }
    }
}
