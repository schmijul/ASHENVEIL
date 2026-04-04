using System;
using UnityEngine;
using UnityEngine.InputSystem;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Ashenveil.Player
{
    /// <summary>
    /// Converts Input System actions into player-facing state and events.
    /// Referenced GDD sections: 5.1
    /// </summary>
    public class PlayerInputHandler : MonoBehaviour
    {
        private const string PreferredInputAssetPath = "Assets/Settings/AshenveilInputActions.inputactions";
        private const string FallbackInputAssetPath = "Assets/InputSystem_Actions.inputactions";

        [Header("References")]
        [SerializeField] private InputActionAsset _inputActions;

        [Header("Player Action Map")]
        [SerializeField] private string _playerActionMapName = "Player";
        [SerializeField] private string _moveActionName = "Move";
        [SerializeField] private string _lookActionName = "Look";
        [SerializeField] private string _sprintActionName = "Sprint";
        [SerializeField] private string _jumpActionName = "Jump";
        [SerializeField] private string _interactActionName = "Interact";

        [Header("Camera Action Map")]
        [SerializeField] private string _cameraActionMapName = "UI";
        [SerializeField] private string _zoomActionName = "ScrollWheel";

        private InputAction _moveAction;
        private InputAction _lookAction;
        private InputAction _zoomAction;
        private InputAction _sprintAction;
        private InputAction _jumpAction;
        private InputAction _interactAction;
        private bool _isBound;

        public event Action<Vector2> MoveInputChanged;
        public event Action<Vector2> LookInputChanged;
        public event Action<float> ZoomInputChanged;
        public event Action<bool> SprintChanged;
        public event Action JumpPressed;
        public event Action InteractPressed;

        public Vector2 MoveInput { get; private set; }
        public Vector2 LookInput { get; private set; }
        public float ZoomInput { get; private set; }
        public bool IsSprinting { get; private set; }

        private void Awake()
        {
            ResolveInputAsset();
        }

        private void OnEnable()
        {
            ResolveInputAsset();
            BindActions();
        }

        private void Update()
        {
            PollContinuousActions();
        }

        private void OnDisable()
        {
            UnbindActions();
        }

        private void OnValidate()
        {
            ResolveInputAsset();
        }

        private void ResolveInputAsset()
        {
            if (_inputActions != null)
            {
                return;
            }

#if UNITY_EDITOR
            _inputActions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(PreferredInputAssetPath);
            if (_inputActions == null)
            {
                _inputActions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(FallbackInputAssetPath);
            }
#endif
        }

        private void BindActions()
        {
            if (_isBound)
            {
                return;
            }

            if (_inputActions == null)
            {
                Debug.LogError($"{nameof(PlayerInputHandler)} on {name} has no InputActionAsset assigned or discoverable.");
                enabled = false;
                return;
            }

            InputActionMap playerMap = _inputActions.FindActionMap(_playerActionMapName, false);
            if (playerMap == null)
            {
                Debug.LogError($"{nameof(PlayerInputHandler)} on {name} could not find action map '{_playerActionMapName}' in '{_inputActions.name}'.");
                enabled = false;
                return;
            }

            _moveAction = RequireAction(playerMap, _moveActionName);
            _lookAction = RequireAction(playerMap, _lookActionName);
            _sprintAction = RequireAction(playerMap, _sprintActionName);
            _jumpAction = RequireAction(playerMap, _jumpActionName);
            _interactAction = playerMap.FindAction(_interactActionName, false);
            InputActionMap cameraMap = _inputActions.FindActionMap(_cameraActionMapName, false);
            _zoomAction = cameraMap?.FindAction(_zoomActionName, false);

            if (_moveAction == null || _lookAction == null || _sprintAction == null || _jumpAction == null)
            {
                Debug.LogError($"{nameof(PlayerInputHandler)} on {name} is missing one or more required player actions.");
                enabled = false;
                return;
            }

            _jumpAction.performed += HandleJumpPerformed;
            if (_interactAction != null)
            {
                _interactAction.performed += HandleInteractPerformed;
            }

            _inputActions.Enable();
            _isBound = true;
        }

        private void UnbindActions()
        {
            if (!_isBound)
            {
                return;
            }

            if (_jumpAction != null)
            {
                _jumpAction.performed -= HandleJumpPerformed;
            }

            if (_interactAction != null)
            {
                _interactAction.performed -= HandleInteractPerformed;
            }

            if (_inputActions != null)
            {
                _inputActions.Disable();
            }

            _isBound = false;
        }

        private void PollContinuousActions()
        {
            if (_moveAction != null)
            {
                Vector2 moveValue = _moveAction.ReadValue<Vector2>();
                if (moveValue != MoveInput)
                {
                    MoveInput = moveValue;
                    MoveInputChanged?.Invoke(MoveInput);
                }
            }

            if (_lookAction != null)
            {
                Vector2 lookValue = _lookAction.ReadValue<Vector2>();
                if (lookValue != LookInput)
                {
                    LookInput = lookValue;
                    LookInputChanged?.Invoke(LookInput);
                }
            }

            if (_zoomAction != null)
            {
                Vector2 zoomValue = _zoomAction.ReadValue<Vector2>();
                float zoomAmount = zoomValue.y;
                if (!Mathf.Approximately(zoomAmount, ZoomInput))
                {
                    ZoomInput = zoomAmount;
                    ZoomInputChanged?.Invoke(ZoomInput);
                }
            }

            if (_sprintAction != null)
            {
                bool sprintValue = _sprintAction.IsPressed();
                if (sprintValue != IsSprinting)
                {
                    IsSprinting = sprintValue;
                    SprintChanged?.Invoke(IsSprinting);
                }
            }
        }

        private void HandleJumpPerformed(InputAction.CallbackContext context)
        {
            JumpPressed?.Invoke();
        }

        private void HandleInteractPerformed(InputAction.CallbackContext context)
        {
            InteractPressed?.Invoke();
        }

        private static InputAction RequireAction(InputActionMap map, string actionName)
        {
            InputAction action = map.FindAction(actionName, false);
            if (action == null)
            {
                Debug.LogError($"Required action '{actionName}' was not found in map '{map.name}'.");
            }

            return action;
        }
    }
}
