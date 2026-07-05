using System.Collections.Generic;
using Ashenveil.Core;
using Ashenveil.Quests;

namespace Ashenveil.Dialog
{
    /// <summary>
    /// Pure dialog traversal model with quest-state filtering.
    /// Referenced GDD section: Kernsysteme / Dialog &amp; Quests.
    /// </summary>
    public sealed class DialogRunnerModel
    {
        private readonly string _dialogId;
        private readonly Dictionary<string, DialogNode> _nodes = new Dictionary<string, DialogNode>();
        private readonly IQuestStateProvider _questStateProvider;
        private readonly IQuestActionSink _questActionSink;
        private string _currentNodeId;
        private bool _finishedSignalRaised;

        public DialogRunnerModel(
            DialogGraph graph,
            IQuestStateProvider questStateProvider,
            IQuestActionSink questActionSink)
        {
            _dialogId = graph != null ? graph.DialogId : "";
            _currentNodeId = graph != null ? graph.EntryNodeId : "";
            _questStateProvider = questStateProvider;
            _questActionSink = questActionSink;

            if (graph != null)
            {
                foreach (DialogNode node in graph.Nodes)
                {
                    if (node == null || string.IsNullOrWhiteSpace(node.Id) || _nodes.ContainsKey(node.Id))
                    {
                        continue;
                    }

                    _nodes.Add(node.Id, node);
                }
            }

            FinishIfAtEnd();
        }

        public DialogNode CurrentNode => _nodes.TryGetValue(_currentNodeId, out DialogNode node) ? node : null;

        public IReadOnlyList<DialogChoice> AvailableChoices => GetAvailableChoices();

        public bool IsFinished => _finishedSignalRaised;

        public bool Choose(int choiceIndex)
        {
            if (IsFinished)
            {
                return false;
            }

            List<DialogChoice> choices = GetAvailableChoices();
            if (choiceIndex < 0 || choiceIndex >= choices.Count)
            {
                return false;
            }

            DialogChoice choice = choices[choiceIndex];
            ApplyQuestAction(choice.QuestAction);
            _currentNodeId = choice.TargetNodeId;
            FinishIfAtEnd();
            return true;
        }

        private List<DialogChoice> GetAvailableChoices()
        {
            List<DialogChoice> availableChoices = new List<DialogChoice>();
            DialogNode currentNode = CurrentNode;
            if (currentNode == null)
            {
                return availableChoices;
            }

            foreach (DialogChoice choice in currentNode.Choices)
            {
                if (choice != null && ConditionAllows(choice.Condition))
                {
                    availableChoices.Add(choice);
                }
            }

            return availableChoices;
        }

        private bool ConditionAllows(DialogChoiceCondition condition)
        {
            if (condition == null || !condition.Enabled)
            {
                return true;
            }

            if (_questStateProvider == null || string.IsNullOrWhiteSpace(condition.QuestId))
            {
                return false;
            }

            return _questStateProvider.GetQuestState(condition.QuestId) == condition.RequiredState;
        }

        private void ApplyQuestAction(DialogQuestAction questAction)
        {
            if (questAction == null || !questAction.HasAction || _questActionSink == null)
            {
                return;
            }

            if (questAction.ActionType == QuestActionType.StartQuest)
            {
                _questActionSink.StartQuest(questAction.QuestId);
            }
            else if (questAction.ActionType == QuestActionType.AdvanceQuest)
            {
                _questActionSink.AdvanceQuest(questAction.QuestId);
            }
            else if (questAction.ActionType == QuestActionType.CompleteQuest)
            {
                _questActionSink.CompleteQuest(questAction.QuestId);
            }
        }

        private void FinishIfAtEnd()
        {
            if (_finishedSignalRaised)
            {
                return;
            }

            DialogNode currentNode = CurrentNode;
            if (currentNode == null || GetAvailableChoices().Count == 0)
            {
                _finishedSignalRaised = true;
                GameSignals.RaiseDialogFinished(_dialogId);
            }
        }
    }
}
