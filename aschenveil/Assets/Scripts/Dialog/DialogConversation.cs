using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ashenveil.Dialog
{
    /// <summary>
    /// Runtime dialog session that advances through a dialog tree and emits quest triggers.
    /// </summary>
    public sealed class DialogConversation
    {
        private readonly HashSet<int> _seenOneTimeNodes = new HashSet<int>();
        private readonly List<DialogChoice> _availableChoices = new List<DialogChoice>();

        private DialogTree _tree;
        private string _activeTreeId;
        private DialogContext _context;
        private DialogNode _currentNode;
        private bool _isComplete;

        public event Action<DialogNode> NodeChanged;
        public event Action<IReadOnlyList<DialogChoice>> ChoicesChanged;
        public event Action<QuestTrigger> QuestTriggerRaised;
        public event Action ConversationEnded;

        public DialogNode CurrentNode => _currentNode;

        public IReadOnlyList<DialogChoice> CurrentChoices => _availableChoices;

        public bool IsComplete => _isComplete;

        public bool Begin(DialogTree tree, int startNodeId = 0, DialogContext context = null)
        {
            if (tree == null)
            {
                return false;
            }

            if (!string.Equals(_activeTreeId, tree.DialogId, StringComparison.Ordinal))
            {
                _seenOneTimeNodes.Clear();
                _activeTreeId = tree.DialogId;
            }

            _tree = tree;
            _context = context ?? new DialogContext();
            _isComplete = false;
            return TryEnterNode(startNodeId);
        }

        public bool SelectChoice(int choiceIndex)
        {
            if (_isComplete || _currentNode == null || _availableChoices.Count == 0)
            {
                return false;
            }

            if (choiceIndex < 0 || choiceIndex >= _availableChoices.Count)
            {
                return false;
            }

            DialogChoice choice = _availableChoices[choiceIndex];
            if (choice == null || !choice.IsAvailable(_context))
            {
                return false;
            }

            return TryEnterNode(choice.TargetNode);
        }

        public bool Advance()
        {
            if (_isComplete || _currentNode == null)
            {
                return false;
            }

            if (_availableChoices.Count > 0)
            {
                return false;
            }

            int nextNode = _currentNode.NextNode;
            if (nextNode < 0)
            {
                EndConversation();
                return true;
            }

            return TryEnterNode(nextNode);
        }

        public void Reset()
        {
            _tree = null;
            _context = null;
            _currentNode = null;
            _availableChoices.Clear();
            _isComplete = false;
        }

        private bool TryEnterNode(int nodeId)
        {
            if (_tree == null)
            {
                return false;
            }

            DialogNode node = _tree.GetNode(nodeId);
            if (node == null)
            {
                DialogNode fallback = _tree.GetFallbackNode();
                if (fallback == null)
                {
                    EndConversation();
                    return false;
                }

                node = fallback;
            }

            if (node.OneTimeOnly && _seenOneTimeNodes.Contains(node.NodeId))
            {
                DialogNode fallbackNode = _tree.GetFallbackNode();
                if (fallbackNode == null || fallbackNode.NodeId == node.NodeId)
                {
                    EndConversation();
                    return false;
                }

                node = fallbackNode;
            }

            _currentNode = node;
            _availableChoices.Clear();

            if (_currentNode.OneTimeOnly)
            {
                _seenOneTimeNodes.Add(_currentNode.NodeId);
            }

            if (_currentNode.QuestTrigger != null && _currentNode.QuestTrigger.IsActive)
            {
                QuestTriggerRaised?.Invoke(_currentNode.QuestTrigger);
            }

            if (_currentNode.HasChoices)
            {
                PopulateAvailableChoices();
            }
            else if (_currentNode.NextNode < 0)
            {
                // A terminal node is still exposed to the UI until the caller advances/ends the conversation.
                PublishCurrentState();
                return true;
            }

            PublishCurrentState();
            return true;
        }

        private void PopulateAvailableChoices()
        {
            if (_currentNode == null)
            {
                return;
            }

            IReadOnlyList<DialogChoice> choices = _currentNode.Choices;
            for (int index = 0; index < choices.Count; index++)
            {
                DialogChoice choice = choices[index];
                if (choice != null && choice.IsAvailable(_context))
                {
                    _availableChoices.Add(choice);
                }
            }

            ChoicesChanged?.Invoke(_availableChoices);
        }

        private void PublishCurrentState()
        {
            NodeChanged?.Invoke(_currentNode);
        }

        private void EndConversation()
        {
            _currentNode = null;
            _availableChoices.Clear();
            _isComplete = true;
            ConversationEnded?.Invoke();
        }
    }
}
