using System.Collections.Generic;
using Ashenveil.Dialog;
using Ashenveil.Inventory;
using Ashenveil.NPC;
using UnityEngine;

namespace Ashenveil.Player
{
    /// <summary>
    /// Resolves nearby pickups and NPCs, then drives a simple conversation loop from the interact input.
    /// Referenced GDD sections: 5.10, 5.11
    /// </summary>
    public class PlayerInteractionController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerInputHandler _inputHandler;
        [SerializeField] private PlayerInventoryRuntime _inventory;

        [Header("Interaction")]
        [SerializeField, Min(0.5f)] private float _interactionRadius = 2.5f;
        [SerializeField, Min(0.01f)] private float _scanInterval = 0.1f;
        [SerializeField] private LayerMask _interactionMask = ~0;

        private readonly DialogConversation _conversation = new DialogConversation();
        private readonly List<string> _currentChoiceTexts = new List<string>();

        private DialogContext _dialogContext;
        private InventoryPickup _nearestPickup;
        private NpcController _nearestNpc;
        private NpcController _activeNpc;
        private float _nextScanAt;
        private string _currentPrompt = string.Empty;
        private string _currentSpeaker = string.Empty;
        private string _currentDialogText = string.Empty;

        public string CurrentPrompt => _currentPrompt;
        public string CurrentSpeaker => _currentSpeaker;
        public string CurrentDialogText => _currentDialogText;
        public IReadOnlyList<string> CurrentChoiceTexts => _currentChoiceTexts;
        public bool IsConversationActive => !_conversation.IsComplete && _conversation.CurrentNode != null;
        public NpcController ActiveNpc => _activeNpc;

        private void Awake()
        {
            ResolveReferences();
            SubscribeConversation();
        }

        private void OnEnable()
        {
            ResolveReferences();
            if (_inputHandler != null)
            {
                _inputHandler.InteractPressed += HandleInteractPressed;
            }
        }

        private void Update()
        {
            if (Time.time >= _nextScanAt)
            {
                RefreshTargets();
                _nextScanAt = Time.time + _scanInterval;
            }
        }

        private void OnDisable()
        {
            if (_inputHandler != null)
            {
                _inputHandler.InteractPressed -= HandleInteractPressed;
            }
        }

        private void OnValidate()
        {
            ResolveReferences();
        }

        public bool TryInteract()
        {
            if (IsConversationActive)
            {
                return ContinueConversation();
            }

            RefreshTargets();
            if (_nearestPickup != null && _inventory != null && _inventory.TryCollect(_nearestPickup))
            {
                RefreshTargets();
                return true;
            }

            if (_nearestNpc != null)
            {
                return BeginConversation(_nearestNpc);
            }

            return false;
        }

        public void ConfigureInteractionRadius(float interactionRadius)
        {
            _interactionRadius = Mathf.Max(0.5f, interactionRadius);
        }

        private void ResolveReferences()
        {
            if (_inputHandler == null)
            {
                TryGetComponent(out _inputHandler);
            }

            if (_inventory == null)
            {
                TryGetComponent(out _inventory);
            }
        }

        private void SubscribeConversation()
        {
            _conversation.NodeChanged += HandleDialogNodeChanged;
            _conversation.ChoicesChanged += HandleChoicesChanged;
            _conversation.ConversationEnded += HandleConversationEnded;
            _conversation.QuestTriggerRaised += HandleQuestTriggerRaised;
        }

        private void RefreshTargets()
        {
            if (IsConversationActive)
            {
                _currentPrompt = _currentChoiceTexts.Count > 0 ? "F: Choose" : "F: Continue";
                return;
            }

            _nearestPickup = null;
            _nearestNpc = null;
            float nearestDistance = float.PositiveInfinity;

            Collider[] colliders = Physics.OverlapSphere(transform.position, _interactionRadius, _interactionMask, QueryTriggerInteraction.Collide);
            for (int index = 0; index < colliders.Length; index++)
            {
                Collider collider = colliders[index];
                if (collider == null)
                {
                    continue;
                }

                InventoryPickup pickup = collider.GetComponentInParent<InventoryPickup>();
                if (pickup != null)
                {
                    float sqrDistance = CalculateSqrDistance(collider, pickup.transform.position);
                    if (sqrDistance < nearestDistance)
                    {
                        nearestDistance = sqrDistance;
                        _nearestPickup = pickup;
                        _nearestNpc = null;
                    }

                    continue;
                }

                NpcController npc = collider.GetComponentInParent<NpcController>();
                if (npc != null)
                {
                    float sqrDistance = CalculateSqrDistance(collider, npc.transform.position);
                    if (sqrDistance < nearestDistance)
                    {
                        nearestDistance = sqrDistance;
                        _nearestNpc = npc;
                        _nearestPickup = null;
                    }
                }
            }

            if (_nearestPickup != null)
            {
                _currentPrompt = _nearestPickup.PickupPrompt;
                return;
            }

            if (_nearestNpc != null)
            {
                _currentPrompt = _nearestNpc.InteractionPrompt;
                return;
            }

            _currentPrompt = string.Empty;
        }

        private float CalculateSqrDistance(Collider collider, Vector3 fallbackPosition)
        {
            if (collider == null)
            {
                return (fallbackPosition - transform.position).sqrMagnitude;
            }

            switch (collider)
            {
                case BoxCollider _:
                case SphereCollider _:
                case CapsuleCollider _:
                    return (collider.ClosestPoint(transform.position) - transform.position).sqrMagnitude;
                case MeshCollider meshCollider when meshCollider.convex:
                    return (collider.ClosestPoint(transform.position) - transform.position).sqrMagnitude;
                default:
                    return (collider.bounds.ClosestPoint(transform.position) - transform.position).sqrMagnitude;
            }
        }

        private bool BeginConversation(NpcController npc)
        {
            if (npc == null || npc.DialogTree == null)
            {
                return false;
            }

            _dialogContext = new DialogContext(_inventory != null ? _inventory.Gold : 0);
            npc.RequestConversation();
            if (!_conversation.Begin(npc.DialogTree, 0, _dialogContext))
            {
                return false;
            }

            _activeNpc = npc;
            return true;
        }

        private bool ContinueConversation()
        {
            if (_currentChoiceTexts.Count > 0)
            {
                return _conversation.SelectChoice(0);
            }

            bool advanced = _conversation.Advance();
            if (!advanced && _conversation.IsComplete)
            {
                HandleConversationEnded();
            }

            return advanced;
        }

        private void HandleInteractPressed()
        {
            TryInteract();
        }

        private void HandleDialogNodeChanged(DialogNode node)
        {
            if (node == null || !node.HasChoices)
            {
                _currentChoiceTexts.Clear();
            }

            _currentSpeaker = node != null ? node.SpeakerName : string.Empty;
            _currentDialogText = node != null ? node.Text : string.Empty;
            _currentPrompt = _currentChoiceTexts.Count > 0 ? "F: Choose" : "F: Continue";
        }

        private void HandleChoicesChanged(IReadOnlyList<DialogChoice> choices)
        {
            _currentChoiceTexts.Clear();
            if (choices == null)
            {
                return;
            }

            for (int index = 0; index < choices.Count; index++)
            {
                DialogChoice choice = choices[index];
                if (choice != null && !string.IsNullOrWhiteSpace(choice.ChoiceText))
                {
                    _currentChoiceTexts.Add(choice.ChoiceText);
                }
            }
        }

        private void HandleConversationEnded()
        {
            _activeNpc = null;
            _currentSpeaker = string.Empty;
            _currentDialogText = string.Empty;
            _currentChoiceTexts.Clear();
            RefreshTargets();
        }

        private void HandleQuestTriggerRaised(QuestTrigger questTrigger)
        {
            if (questTrigger == null || _dialogContext == null)
            {
                return;
            }

            switch (questTrigger.TriggerType)
            {
                case QuestTriggerType.SetFlag:
                    _dialogContext.SetFlag(questTrigger.FlagName);
                    break;
                case QuestTriggerType.GrantGold:
                    _dialogContext.AddGold(questTrigger.GoldAmount);
                    _inventory?.AddGold(questTrigger.GoldAmount);
                    break;
                case QuestTriggerType.StartQuest:
                    _dialogContext.SetQuestState(questTrigger.QuestId, "Started");
                    break;
                case QuestTriggerType.CompleteQuest:
                    _dialogContext.SetQuestState(questTrigger.QuestId, "Completed");
                    break;
            }
        }
    }
}
