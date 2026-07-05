using System;
using Ashenveil.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Ashenveil.Player
{
    /// <summary>
    /// CharacterController adapter for player movement input and stamina-gated actions.
    /// Referenced GDD section: Kernsysteme / Movement/Camera.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerMovementController : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private PlayerInputConfig _inputConfig;

        [Header("References")]
        [SerializeField] private Transform _cameraTransform;
        [SerializeField] private PlayerVitals _vitals;

        private CharacterController _characterController;
        private PlayerMovementModel _movementModel;
        private InputAction _moveAction;
        private InputAction _lookAction;
        private InputAction _sprintAction;
        private InputAction _jumpAction;
        private InputAction _dodgeAction;
        private InputAction _interactAction;
        private InputAction _inventoryAction;
        private InputAction _lightAttackAction;
        private InputAction _blockOrHeavyAction;
        private InputAction _aetherModeAction;
        private InputAction _lootAction;
        private PlayerContext _playerContext;

        /// <summary>
        /// Raised when the interact input is pressed.
        /// </summary>
        public event Action<PlayerContext> InteractPressed;

        /// <summary>
        /// Raised when mouse look input changes. Args: look delta.
        /// </summary>
        public event Action<Vector2> LookChanged;

        /// <summary>
        /// Raised when the inventory input is pressed.
        /// </summary>
        public event Action InventoryPressed;

        /// <summary>
        /// Raised when the light attack input is pressed and stamina is available.
        /// </summary>
        public event Action LightAttackPressed;

        /// <summary>
        /// Raised when block or heavy input is pressed and stamina is available.
        /// </summary>
        public event Action BlockOrHeavyPressed;

        /// <summary>
        /// Raised when aether mode input is pressed.
        /// </summary>
        public event Action AetherModePressed;

        /// <summary>
        /// Raised when loot input is pressed.
        /// </summary>
        public event Action LootPressed;

        /// <summary>
        /// Current player context for interaction systems.
        /// </summary>
        public PlayerContext Context => _playerContext;

        private void Awake()
        {
            if (!TryGetComponent(out _characterController))
            {
                Debug.LogError("PlayerMovementController requires a CharacterController.", this);
                enabled = false;
                return;
            }

            if (_cameraTransform == null)
            {
                Debug.LogError("PlayerMovementController requires a camera transform reference.", this);
                enabled = false;
                return;
            }

            if (_vitals == null && !TryGetComponent(out _vitals))
            {
                Debug.LogError("PlayerMovementController requires a PlayerVitals reference.", this);
                enabled = false;
                return;
            }

            PlayerMovementModel.Settings settings = _inputConfig != null
                ? _inputConfig.ToMovementSettings()
                : PlayerMovementModel.Settings.Default;
            _movementModel = new PlayerMovementModel(settings);
            _playerContext = new PlayerContext(transform, this, _vitals, _vitals.Stamina);
            CreateInputActions();
        }

        private void OnEnable()
        {
            SetInputEnabled(true);
        }

        private void OnDisable()
        {
            SetInputEnabled(false);
        }

        private void OnDestroy()
        {
            DisposeInputActions();
        }

        private void Update()
        {
            if (_movementModel == null || _characterController == null)
            {
                return;
            }

            float deltaTime = Time.deltaTime;
            Vector2 moveInput = _moveAction.ReadValue<Vector2>();
            Vector2 lookInput = _lookAction.ReadValue<Vector2>();
            Vector3 moveDirection = GetCameraRelativeMove(moveInput);
            if (lookInput.sqrMagnitude > 0.001f)
            {
                LookChanged?.Invoke(lookInput);
            }

            bool wantsSprint = _sprintAction.IsPressed() && moveInput.sqrMagnitude > 0.01f;
            StaminaModel.State staminaState = _vitals.UpdateStamina(wantsSprint, deltaTime);
            bool dodgePressed = _dodgeAction.WasPressedThisFrame()
                && moveInput.sqrMagnitude > 0.01f
                && !_movementModel.IsDodging
                && _vitals.Stamina.TrySpendDodge();

            PlayerMovementModel.State movementState = _movementModel.Tick(
                new PlayerMovementModel.Input
                {
                    MoveDirection = moveDirection,
                    SprintHeld = staminaState.IsSprinting,
                    JumpPressed = _jumpAction.WasPressedThisFrame(),
                    DodgePressed = dodgePressed,
                    IsGrounded = _characterController.isGrounded
                },
                deltaTime);

            _characterController.Move(movementState.Velocity * deltaTime);
            transform.rotation = Quaternion.Euler(0f, movementState.Yaw, 0f);
        }

        private Vector3 GetCameraRelativeMove(Vector2 moveInput)
        {
            Vector3 forward = _cameraTransform.forward;
            Vector3 right = _cameraTransform.right;
            forward.y = 0f;
            right.y = 0f;
            forward = forward.sqrMagnitude > 0f ? forward.normalized : transform.forward;
            right = right.sqrMagnitude > 0f ? right.normalized : transform.right;
            return forward * moveInput.y + right * moveInput.x;
        }

        private void CreateInputActions()
        {
            _moveAction = new InputAction("Move", InputActionType.Value, expectedControlType: "Vector2");
            _moveAction.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");

            _lookAction = new InputAction("Look", InputActionType.Value, "<Mouse>/delta", expectedControlType: "Vector2");
            _sprintAction = new InputAction("Sprint", InputActionType.Button, "<Keyboard>/leftShift");
            _jumpAction = new InputAction("Jump", InputActionType.Button, "<Keyboard>/space");
            _dodgeAction = new InputAction("Dodge", InputActionType.Button);
            _dodgeAction.AddBinding("<Keyboard>/leftAlt");
            _dodgeAction.AddBinding("<Keyboard>/leftCtrl");
            _interactAction = new InputAction("Interact", InputActionType.Button, "<Keyboard>/e");
            _inventoryAction = new InputAction("Inventory", InputActionType.Button, "<Keyboard>/tab");
            _lightAttackAction = new InputAction("LightAttack", InputActionType.Button, "<Mouse>/leftButton");
            _blockOrHeavyAction = new InputAction("BlockOrHeavy", InputActionType.Button, "<Mouse>/rightButton");
            _aetherModeAction = new InputAction("AetherMode", InputActionType.Button, "<Keyboard>/q");
            _lootAction = new InputAction("Loot", InputActionType.Button, "<Keyboard>/f");

            _interactAction.performed += OnInteract;
            _inventoryAction.performed += OnInventory;
            _lightAttackAction.performed += OnLightAttack;
            _blockOrHeavyAction.performed += OnBlockOrHeavy;
            _aetherModeAction.performed += OnAetherMode;
            _lootAction.performed += OnLoot;
        }

        private void SetInputEnabled(bool enabledState)
        {
            if (_moveAction == null)
            {
                return;
            }

            if (enabledState)
            {
                _moveAction.Enable();
                _lookAction.Enable();
                _sprintAction.Enable();
                _jumpAction.Enable();
                _dodgeAction.Enable();
                _interactAction.Enable();
                _inventoryAction.Enable();
                _lightAttackAction.Enable();
                _blockOrHeavyAction.Enable();
                _aetherModeAction.Enable();
                _lootAction.Enable();
                return;
            }

            _moveAction.Disable();
            _lookAction.Disable();
            _sprintAction.Disable();
            _jumpAction.Disable();
            _dodgeAction.Disable();
            _interactAction.Disable();
            _inventoryAction.Disable();
            _lightAttackAction.Disable();
            _blockOrHeavyAction.Disable();
            _aetherModeAction.Disable();
            _lootAction.Disable();
        }

        private void DisposeInputActions()
        {
            if (_moveAction == null)
            {
                return;
            }

            _interactAction.performed -= OnInteract;
            _inventoryAction.performed -= OnInventory;
            _lightAttackAction.performed -= OnLightAttack;
            _blockOrHeavyAction.performed -= OnBlockOrHeavy;
            _aetherModeAction.performed -= OnAetherMode;
            _lootAction.performed -= OnLoot;

            _moveAction.Dispose();
            _lookAction.Dispose();
            _sprintAction.Dispose();
            _jumpAction.Dispose();
            _dodgeAction.Dispose();
            _interactAction.Dispose();
            _inventoryAction.Dispose();
            _lightAttackAction.Dispose();
            _blockOrHeavyAction.Dispose();
            _aetherModeAction.Dispose();
            _lootAction.Dispose();
        }

        private void OnInteract(InputAction.CallbackContext context)
        {
            InteractPressed?.Invoke(_playerContext);
        }

        private void OnInventory(InputAction.CallbackContext context)
        {
            InventoryPressed?.Invoke();
        }

        private void OnLightAttack(InputAction.CallbackContext context)
        {
            if (_vitals.Stamina.TrySpendLightAttack())
            {
                LightAttackPressed?.Invoke();
            }
        }

        private void OnBlockOrHeavy(InputAction.CallbackContext context)
        {
            if (_vitals.Stamina.TrySpendHeavyAttack())
            {
                BlockOrHeavyPressed?.Invoke();
            }
        }

        private void OnAetherMode(InputAction.CallbackContext context)
        {
            AetherModePressed?.Invoke();
        }

        private void OnLoot(InputAction.CallbackContext context)
        {
            LootPressed?.Invoke();
        }
    }
}
