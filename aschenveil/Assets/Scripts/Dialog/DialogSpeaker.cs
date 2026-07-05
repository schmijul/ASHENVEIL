using Ashenveil.Core;
using UnityEngine;

namespace Ashenveil.Dialog
{
    /// <summary>
    /// World NPC adapter that exposes dialog through the interactable contract.
    /// Referenced GDD section: Kernsysteme / Dialog &amp; Quests.
    /// </summary>
    public sealed class DialogSpeaker : MonoBehaviour, IInteractable
    {
        [Header("Dialog")]
        [SerializeField] private DialogGraph _dialogGraph;
        [SerializeField] private string _npcDisplayName = "Dorfbewohner";

        public string InteractionPrompt => "Reden (E)";

        public DialogGraph DialogGraph => _dialogGraph;

        public string NpcDisplayName => _npcDisplayName;

        public void Interact(PlayerContext playerContext)
        {
            if (_dialogGraph == null)
            {
                Debug.LogWarning($"DialogSpeaker '{name}' has no dialog graph.", this);
            }
        }
    }
}
