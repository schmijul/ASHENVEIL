using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Ashenveil.Dialog
{
    /// <summary>
    /// Condition types used by dialog choices and branches.
    /// </summary>
    public enum DialogConditionType
    {
        None = 0,
        HasItem = 1,
        QuestState = 2,
        Flag = 3,
        GoldAtLeast = 4
    }

    /// <summary>
    /// Quest trigger types emitted by dialog nodes.
    /// </summary>
    public enum QuestTriggerType
    {
        None = 0,
        StartQuest = 1,
        CompleteQuest = 2,
        SetFlag = 3,
        GrantGold = 4
    }

    /// <summary>
    /// Runtime dialog context used to evaluate branching conditions.
    /// </summary>
    public sealed class DialogContext
    {
        private readonly Dictionary<string, int> _items = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, string> _questStates = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _flags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public DialogContext(int gold = 0)
        {
            Gold = Mathf.Max(0, gold);
        }

        public int Gold { get; private set; }

        public void AddItem(string itemId, int amount = 1)
        {
            if (string.IsNullOrWhiteSpace(itemId) || amount <= 0)
            {
                return;
            }

            if (_items.TryGetValue(itemId, out int currentAmount))
            {
                _items[itemId] = currentAmount + amount;
                return;
            }

            _items[itemId] = amount;
        }

        public bool HasItem(string itemId, int amount = 1)
        {
            if (string.IsNullOrWhiteSpace(itemId) || amount <= 0)
            {
                return false;
            }

            return _items.TryGetValue(itemId, out int currentAmount) && currentAmount >= amount;
        }

        public void SetQuestState(string questId, string state)
        {
            if (string.IsNullOrWhiteSpace(questId))
            {
                return;
            }

            _questStates[questId] = state ?? string.Empty;
        }

        public bool HasQuestState(string questId, string requiredState)
        {
            if (string.IsNullOrWhiteSpace(questId))
            {
                return false;
            }

            if (!_questStates.TryGetValue(questId, out string currentState))
            {
                return false;
            }

            return string.Equals(currentState, requiredState ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        }

        public void SetFlag(string flag)
        {
            if (string.IsNullOrWhiteSpace(flag))
            {
                return;
            }

            _flags.Add(flag);
        }

        public bool HasFlag(string flag)
        {
            return !string.IsNullOrWhiteSpace(flag) && _flags.Contains(flag);
        }

        public void AddGold(int amount)
        {
            Gold = Mathf.Max(0, Gold + amount);
        }
    }

    /// <summary>
    /// Serializable branching condition for dialog choices.
    /// </summary>
    [Serializable]
    public sealed class DialogCondition
    {
        [SerializeField] private DialogConditionType _conditionType = DialogConditionType.None;
        [SerializeField] private string _targetId;
        [SerializeField] private string _requiredState = "Completed";
        [SerializeField, Min(1)] private int _requiredAmount = 1;

        public DialogConditionType ConditionType => _conditionType;

        public string TargetId => _targetId;

        public string RequiredState => _requiredState;

        public int RequiredAmount => _requiredAmount;

        public static DialogCondition Create(
            DialogConditionType conditionType,
            string targetId = "",
            int requiredAmount = 1,
            string requiredState = "Completed")
        {
            return new DialogCondition
            {
                _conditionType = conditionType,
                _targetId = targetId,
                _requiredAmount = Mathf.Max(1, requiredAmount),
                _requiredState = requiredState ?? string.Empty
            };
        }

        public bool Evaluate(DialogContext context)
        {
            if (_conditionType == DialogConditionType.None)
            {
                return true;
            }

            if (context == null)
            {
                return false;
            }

            switch (_conditionType)
            {
                case DialogConditionType.HasItem:
                    return context.HasItem(_targetId, _requiredAmount);
                case DialogConditionType.QuestState:
                    return context.HasQuestState(_targetId, _requiredState);
                case DialogConditionType.Flag:
                    return context.HasFlag(_targetId);
                case DialogConditionType.GoldAtLeast:
                    return context.Gold >= _requiredAmount;
                default:
                    return true;
            }
        }
    }

    /// <summary>
    /// Dialog branch choice that can be gated by a condition.
    /// </summary>
    [Serializable]
    public sealed class DialogChoice
    {
        [SerializeField, TextArea(1, 2)] private string _choiceText;
        [SerializeField] private int _targetNode = -1;
        [SerializeField] private DialogCondition _condition;

        public string ChoiceText => _choiceText;

        public int TargetNode => _targetNode;

        public DialogCondition Condition => _condition;

        public static DialogChoice Create(string choiceText, int targetNode, DialogCondition condition = null)
        {
            return new DialogChoice
            {
                _choiceText = choiceText ?? string.Empty,
                _targetNode = targetNode,
                _condition = condition
            };
        }

        public bool IsAvailable(DialogContext context)
        {
            return _condition == null || _condition.Evaluate(context);
        }
    }

    /// <summary>
    /// Quest trigger emitted when a dialog node is entered.
    /// </summary>
    [Serializable]
    public sealed class QuestTrigger
    {
        [SerializeField] private QuestTriggerType _triggerType = QuestTriggerType.None;
        [SerializeField] private string _questId;
        [SerializeField] private string _flagName;
        [SerializeField, Min(0)] private int _goldAmount;

        public QuestTriggerType TriggerType => _triggerType;

        public string QuestId => _questId;

        public string FlagName => _flagName;

        public int GoldAmount => _goldAmount;

        public static QuestTrigger Create(
            QuestTriggerType triggerType,
            string questId = "",
            string flagName = "",
            int goldAmount = 0)
        {
            return new QuestTrigger
            {
                _triggerType = triggerType,
                _questId = questId ?? string.Empty,
                _flagName = flagName ?? string.Empty,
                _goldAmount = Mathf.Max(0, goldAmount)
            };
        }

        public bool IsActive => _triggerType != QuestTriggerType.None;
    }

    /// <summary>
    /// One dialog node in a branching dialog tree.
    /// </summary>
    [Serializable]
    public sealed class DialogNode
    {
        [SerializeField] private int _nodeId;
        [SerializeField] private string _speakerName;
        [SerializeField, TextArea(3, 8)] private string _text;
        [SerializeField] private DialogChoice[] _choices = Array.Empty<DialogChoice>();
        [SerializeField] private int _nextNode = -1;
        [SerializeField] private QuestTrigger _questTrigger;
        [SerializeField] private bool _oneTimeOnly;

        public int NodeId => _nodeId;

        public string SpeakerName => _speakerName;

        public string Text => _text;

        public IReadOnlyList<DialogChoice> Choices => _choices ?? Array.Empty<DialogChoice>();

        public int NextNode => _nextNode;

        public QuestTrigger QuestTrigger => _questTrigger;

        public bool OneTimeOnly => _oneTimeOnly;

        public bool HasChoices => _choices != null && _choices.Length > 0;

        public static DialogNode Create(
            int nodeId,
            string speakerName,
            string text,
            int nextNode = -1,
            bool oneTimeOnly = false,
            QuestTrigger questTrigger = null,
            params DialogChoice[] choices)
        {
            return new DialogNode
            {
                _nodeId = nodeId,
                _speakerName = speakerName ?? string.Empty,
                _text = text ?? string.Empty,
                _nextNode = nextNode,
                _oneTimeOnly = oneTimeOnly,
                _questTrigger = questTrigger,
                _choices = choices ?? Array.Empty<DialogChoice>()
            };
        }
    }

    /// <summary>
    /// ScriptableObject dialog tree used by NPCs.
    /// </summary>
    [CreateAssetMenu(fileName = "DialogTree", menuName = "Ashenveil/Dialogs/Dialog Tree")]
    public sealed class DialogTree : ScriptableObject
    {
        [Header("Base Properties")]
        [SerializeField] private string _dialogId = "RuntimeDialogTree";
        [SerializeField] private DialogNode[] _nodes = Array.Empty<DialogNode>();
        [SerializeField] private int _fallbackNodeId = -1;

        public string DialogId => _dialogId;

        public IReadOnlyList<DialogNode> Nodes => _nodes ?? Array.Empty<DialogNode>();

        public int FallbackNodeId => _fallbackNodeId;

        public static DialogTree CreateRuntimeDefaults()
        {
            DialogTree tree = CreateInstance<DialogTree>();
            tree.hideFlags = HideFlags.HideAndDontSave;
            tree._dialogId = $"RuntimeDialogTree_{Guid.NewGuid():N}";
            tree._nodes = Array.Empty<DialogNode>();
            tree._fallbackNodeId = -1;
            return tree;
        }

        public void SetNodes(IEnumerable<DialogNode> nodes, int fallbackNodeId = -1)
        {
            _nodes = nodes?.ToArray() ?? Array.Empty<DialogNode>();
            _fallbackNodeId = fallbackNodeId;
        }

        public DialogNode GetNode(int nodeId)
        {
            if (_nodes == null || _nodes.Length == 0)
            {
                return null;
            }

            for (int index = 0; index < _nodes.Length; index++)
            {
                DialogNode node = _nodes[index];
                if (node != null && node.NodeId == nodeId)
                {
                    return node;
                }
            }

            return null;
        }

        public DialogNode GetFallbackNode()
        {
            return _fallbackNodeId >= 0 ? GetNode(_fallbackNodeId) : null;
        }
    }
}
