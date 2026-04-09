using System.Collections;
using Ashenveil.Player;
using UnityEngine;

namespace Ashenveil.Combat
{
    /// <summary>
    /// Connects player input to combat state and simulates swing timing without animation events.
    /// Referenced GDD section: 5.5
    /// </summary>
    [RequireComponent(typeof(CombatController))]
    public class PlayerCombatInputDriver : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerInputHandler _inputHandler;
        [SerializeField] private CombatController _combatController;
        [SerializeField] private CharacterController _characterController;

        [Header("Swing Timing")]
        [SerializeField, Min(0.05f)] private float _minimumSwingWindow = 0.12f;
        [SerializeField, Range(0.1f, 1f)] private float _recoveryToActiveRatio = 0.5f;

        private Coroutine _swingRoutine;
        private Coroutine _dodgeRoutine;

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnEnable()
        {
            ResolveReferences();

            if (_inputHandler != null)
            {
                _inputHandler.MoveInputChanged += HandleMoveInputChanged;
                _inputHandler.LightAttackPressed += HandleLightAttackPressed;
                _inputHandler.HeavyAttackPressed += HandleHeavyAttackPressed;
                _inputHandler.DodgePressed += HandleDodgePressed;
                _inputHandler.BlockChanged += HandleBlockChanged;
            }

            if (_combatController != null)
            {
                _combatController.AttackStarted += HandleAttackStarted;
                _combatController.DodgeStarted += HandleDodgeStarted;
            }
        }

        private void OnDisable()
        {
            if (_inputHandler != null)
            {
                _inputHandler.MoveInputChanged -= HandleMoveInputChanged;
                _inputHandler.LightAttackPressed -= HandleLightAttackPressed;
                _inputHandler.HeavyAttackPressed -= HandleHeavyAttackPressed;
                _inputHandler.DodgePressed -= HandleDodgePressed;
                _inputHandler.BlockChanged -= HandleBlockChanged;
            }

            if (_combatController != null)
            {
                _combatController.AttackStarted -= HandleAttackStarted;
                _combatController.DodgeStarted -= HandleDodgeStarted;
                _combatController.OnWeaponSwingEnd();
                _combatController.OnDodgeEnd();
            }

            StopAllCoroutines();
            _swingRoutine = null;
            _dodgeRoutine = null;
        }

        private void OnValidate()
        {
            ResolveReferences();
        }

        private void ResolveReferences()
        {
            if (_inputHandler == null)
            {
                TryGetComponent(out _inputHandler);
            }

            if (_combatController == null)
            {
                TryGetComponent(out _combatController);
            }

            if (_characterController == null)
            {
                TryGetComponent(out _characterController);
            }
        }

        private void HandleMoveInputChanged(Vector2 movementInput)
        {
            _combatController?.SetMovementInput(movementInput);
        }

        private void HandleLightAttackPressed()
        {
            _combatController?.RequestLightAttack();
        }

        private void HandleHeavyAttackPressed()
        {
            _combatController?.RequestHeavyAttack();
        }

        private void HandleDodgePressed()
        {
            _combatController?.RequestDodge();
        }

        private void HandleBlockChanged(bool active)
        {
            if (_combatController == null)
            {
                return;
            }

            if (active)
            {
                _combatController.BeginBlock();
                return;
            }

            _combatController.EndBlock();
        }

        private void HandleAttackStarted(AttackStepDefinition attackStep)
        {
            if (_combatController == null || attackStep == null)
            {
                return;
            }

            if (_swingRoutine != null)
            {
                StopCoroutine(_swingRoutine);
            }

            _swingRoutine = StartCoroutine(ExecuteSwingRoutine(attackStep));
        }

        private void HandleDodgeStarted(DodgeSettings dodgeSettings, Vector2 direction)
        {
            if (_dodgeRoutine != null)
            {
                StopCoroutine(_dodgeRoutine);
            }

            _dodgeRoutine = StartCoroutine(ExecuteDodgeRoutine(dodgeSettings, direction));
        }

        private IEnumerator ExecuteSwingRoutine(AttackStepDefinition attackStep)
        {
            float windUp = Mathf.Max(0f, attackStep.WindUp);
            if (windUp > 0f)
            {
                yield return new WaitForSeconds(windUp);
            }

            _combatController.OnWeaponSwingBegin();

            float activeWindow = Mathf.Max(_minimumSwingWindow, attackStep.Recovery * _recoveryToActiveRatio);
            yield return new WaitForSeconds(activeWindow);

            _combatController.OnWeaponSwingEnd();
            _swingRoutine = null;
        }

        private IEnumerator ExecuteDodgeRoutine(DodgeSettings dodgeSettings, Vector2 direction)
        {
            if (_combatController == null)
            {
                yield break;
            }

            DodgeSettings settings = dodgeSettings ?? DodgeSettings.CreateDefault();
            Vector3 worldDirection = ResolveWorldDirection(direction);
            float duration = Mathf.Max(0.01f, settings.Duration);
            float speed = settings.Distance / duration;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                float step = Mathf.Min(Time.deltaTime, duration - elapsed);
                if (_characterController != null)
                {
                    _characterController.Move(worldDirection * speed * step);
                }
                else
                {
                    transform.position += worldDirection * speed * step;
                }

                elapsed += step;
                yield return null;
            }

            _combatController.OnDodgeEnd();
            _dodgeRoutine = null;
        }

        private Vector3 ResolveWorldDirection(Vector2 inputDirection)
        {
            Vector3 forward = transform.forward;
            forward.y = 0f;
            forward.Normalize();

            Vector3 right = transform.right;
            right.y = 0f;
            right.Normalize();

            Vector2 normalizedInput = inputDirection.sqrMagnitude > 0.0001f ? inputDirection.normalized : Vector2.up;
            Vector3 worldDirection = (right * normalizedInput.x) + (forward * normalizedInput.y);
            if (worldDirection.sqrMagnitude <= 0.0001f)
            {
                worldDirection = forward.sqrMagnitude > 0.0001f ? forward : Vector3.forward;
            }

            return worldDirection.normalized;
        }
    }
}
