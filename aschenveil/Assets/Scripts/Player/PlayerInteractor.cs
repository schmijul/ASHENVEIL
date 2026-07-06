using Ashenveil.Core;
using Ashenveil.UI;
using UnityEngine;

namespace Ashenveil.Player
{
    /// <summary>
    /// Casts from the camera each frame to find the nearest <see cref="IInteractable"/>,
    /// shows its German prompt on the HUD, and activates it on the interact input.
    /// Keeps interaction free of scene lookups by using serialized references.
    /// Referenced GDD section: Kernsysteme / UI.
    /// </summary>
    public sealed class PlayerInteractor : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Camera _camera;
        [SerializeField] private PlayerMovementController _movementController;
        [SerializeField] private HudController _hud;

        [Header("Ray")]
        [SerializeField] private float _interactDistance = 3.5f;
        [SerializeField] private float _sphereRadius = 0.4f;
        [SerializeField] private LayerMask _interactMask = ~0;

        private IInteractable _current;

        private void Awake()
        {
            if (_camera == null)
            {
                _camera = Camera.main;
            }
        }

        private void OnEnable()
        {
            if (_movementController != null)
            {
                _movementController.InteractPressed += OnInteractPressed;
                _movementController.LootPressed += OnLootPressed;
            }
        }

        private void OnDisable()
        {
            if (_movementController != null)
            {
                _movementController.InteractPressed -= OnInteractPressed;
                _movementController.LootPressed -= OnLootPressed;
            }
        }

        private void Update()
        {
            _current = FindInteractable();
            if (_hud != null)
            {
                _hud.SetPrompt(_current != null ? _current.InteractionPrompt : string.Empty);
            }
        }

        private IInteractable FindInteractable()
        {
            if (_camera == null)
            {
                return null;
            }

            Ray ray = new Ray(_camera.transform.position, _camera.transform.forward);
            if (Physics.SphereCast(ray, _sphereRadius, out RaycastHit hit, _interactDistance, _interactMask, QueryTriggerInteraction.Collide))
            {
                return hit.collider.GetComponentInParent<IInteractable>();
            }

            return null;
        }

        private void OnInteractPressed(PlayerContext context)
        {
            _current?.Interact(context);
        }

        private void OnLootPressed()
        {
            // F is the dedicated loot key; it activates the focused container/pickup.
            if (_current != null && _movementController != null)
            {
                _current.Interact(_movementController.Context);
            }
        }
    }
}
