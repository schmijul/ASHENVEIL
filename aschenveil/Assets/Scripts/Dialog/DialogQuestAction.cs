using UnityEngine;

namespace Ashenveil.Dialog
{
    /// <summary>
    /// Optional quest command attached to a dialog choice.
    /// Referenced GDD section: Kernsysteme / Dialog &amp; Quests.
    /// </summary>
    [System.Serializable]
    public sealed class DialogQuestAction
    {
        [Header("Quest Action")]
        [SerializeField] private QuestActionType _actionType = QuestActionType.None;
        [SerializeField] private string _questId = "quest_id";

        public DialogQuestAction()
        {
        }

        public DialogQuestAction(QuestActionType actionType, string questId)
        {
            _actionType = actionType;
            _questId = questId;
        }

        public QuestActionType ActionType => _actionType;

        public string QuestId => _questId;

        public bool HasAction => _actionType != QuestActionType.None && !string.IsNullOrWhiteSpace(_questId);
    }
}
