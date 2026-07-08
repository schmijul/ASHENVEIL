using Ashenveil.Quests;
using UnityEngine;

namespace Ashenveil.Dialog
{
    /// <summary>
    /// Optional quest-state requirement for a dialog choice.
    /// Referenced GDD section: Kernsysteme / Dialog &amp; Quests.
    /// </summary>
    [System.Serializable]
    public sealed class DialogChoiceCondition
    {
        [Header("Condition")]
        [SerializeField] private bool _enabled;
        [SerializeField] private string _questId = "quest_id";
        [SerializeField] private QuestState _requiredState = QuestState.Active;

        public DialogChoiceCondition()
        {
        }

        public DialogChoiceCondition(string questId, QuestState requiredState)
        {
            _enabled = true;
            _questId = questId;
            _requiredState = requiredState;
        }

        public bool Enabled => _enabled;

        public string QuestId => _questId;

        public QuestState RequiredState => _requiredState;
    }
}
