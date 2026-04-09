using System.Collections;
using Ashenveil.Data;
using Ashenveil.Inventory;
using Ashenveil.Player;
using UnityEngine;

namespace Ashenveil.UI
{
    /// <summary>
    /// Lightweight debug HUD for the integrated opening loop until the real HUD system lands.
    /// Referenced GDD sections: 5.10, 5.11
    /// </summary>
    public class OpeningLoopDebugHud : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerInteractionController _interactionController;
        [SerializeField] private PlayerInventoryRuntime _inventory;

        [Header("Toast")]
        [SerializeField, Min(0.25f)] private float _toastDuration = 2f;

        private string _toastMessage = string.Empty;
        private Coroutine _toastRoutine;

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnEnable()
        {
            ResolveReferences();
            if (_inventory != null)
            {
                _inventory.ItemAdded += HandleItemAdded;
            }
        }

        private void OnDisable()
        {
            if (_inventory != null)
            {
                _inventory.ItemAdded -= HandleItemAdded;
            }
        }

        private void OnValidate()
        {
            ResolveReferences();
        }

        private void OnGUI()
        {
            if (_interactionController == null || _inventory == null)
            {
                return;
            }

            GUI.Box(new Rect(16f, 16f, 540f, 150f), string.Empty);
            GUILayout.BeginArea(new Rect(28f, 28f, 516f, 126f));
            GUILayout.Label($"Gold: {_inventory.Gold}");

            if (!string.IsNullOrWhiteSpace(_interactionController.CurrentPrompt))
            {
                GUILayout.Label(_interactionController.CurrentPrompt);
            }

            if (_interactionController.IsConversationActive)
            {
                GUILayout.Space(6f);
                GUILayout.Label($"{_interactionController.CurrentSpeaker}: {_interactionController.CurrentDialogText}");
                if (_interactionController.CurrentChoiceTexts.Count > 0)
                {
                    GUILayout.Label($"Auto choice on interact: {_interactionController.CurrentChoiceTexts[0]}");
                }
            }

            if (!string.IsNullOrWhiteSpace(_toastMessage))
            {
                GUILayout.Space(6f);
                GUILayout.Label(_toastMessage);
            }

            GUILayout.EndArea();
        }

        private void ResolveReferences()
        {
            if (_interactionController == null)
            {
                TryGetComponent(out _interactionController);
            }

            if (_inventory == null)
            {
                TryGetComponent(out _inventory);
            }
        }

        private void HandleItemAdded(ItemData item, int quantity)
        {
            string displayName = item != null ? item.DisplayName : "Item";
            _toastMessage = $"Picked up {quantity}x {displayName}";

            if (_toastRoutine != null)
            {
                StopCoroutine(_toastRoutine);
            }

            _toastRoutine = StartCoroutine(ClearToastAfterDelay());
        }

        private IEnumerator ClearToastAfterDelay()
        {
            yield return new WaitForSeconds(_toastDuration);
            _toastMessage = string.Empty;
            _toastRoutine = null;
        }
    }
}
