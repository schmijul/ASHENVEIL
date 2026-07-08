using System.Collections.Generic;
using UnityEngine;

namespace Ashenveil.Dialog
{
    /// <summary>
    /// A single NPC line and its available player choices.
    /// Referenced GDD section: Kernsysteme / Dialog &amp; Quests.
    /// </summary>
    [System.Serializable]
    public sealed class DialogNode
    {
        [Header("Identity")]
        [SerializeField] private string _id = "node_id";

        [Header("German Text")]
        [SerializeField] private string _speakerName = "NPC";
        [TextArea]
        [SerializeField] private string _text = "Dialog";

        [Header("Choices")]
        [SerializeField] private List<DialogChoice> _choices = new List<DialogChoice>();

        public DialogNode()
        {
        }

        public DialogNode(string id, string speakerName, string text, IEnumerable<DialogChoice> choices)
        {
            _id = id;
            _speakerName = speakerName;
            _text = text;
            _choices = new List<DialogChoice>(choices ?? new DialogChoice[0]);
        }

        public string Id => _id;

        public string SpeakerName => _speakerName;

        public string Text => _text;

        public IReadOnlyList<DialogChoice> Choices => _choices;
    }
}
