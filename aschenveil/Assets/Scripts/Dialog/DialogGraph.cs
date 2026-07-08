using System.Collections.Generic;
using UnityEngine;

namespace Ashenveil.Dialog
{
    /// <summary>
    /// Authorable branching dialog graph.
    /// Referenced GDD section: Kernsysteme / Dialog &amp; Quests.
    /// </summary>
    [CreateAssetMenu(fileName = "NewDialogGraph", menuName = "Ashenveil/Dialog/Dialog Graph")]
    public sealed class DialogGraph : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string _dialogId = "dialog_id";

        [Header("Entry")]
        [SerializeField] private string _entryNodeId = "entry";

        [Header("Nodes")]
        [SerializeField] private List<DialogNode> _nodes = new List<DialogNode>();

        public string DialogId => _dialogId;

        public string EntryNodeId => _entryNodeId;

        public IReadOnlyList<DialogNode> Nodes => _nodes;

        public void Configure(string dialogId, string entryNodeId, IEnumerable<DialogNode> nodes)
        {
            _dialogId = dialogId;
            _entryNodeId = entryNodeId;
            _nodes = new List<DialogNode>(nodes ?? new DialogNode[0]);
        }
    }
}
