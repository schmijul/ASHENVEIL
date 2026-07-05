using UnityEngine;

namespace Ashenveil.Dialog
{
    /// <summary>
    /// Player-selectable dialog branch.
    /// Referenced GDD section: Kernsysteme / Dialog &amp; Quests.
    /// </summary>
    [System.Serializable]
    public sealed class DialogChoice
    {
        [Header("German Text")]
        [SerializeField] private string _text = "Antwort";

        [Header("Navigation")]
        [SerializeField] private string _targetNodeId = "";

        [Header("Quest")]
        [SerializeField] private DialogQuestAction _questAction = new DialogQuestAction();
        [SerializeField] private DialogChoiceCondition _condition = new DialogChoiceCondition();

        public DialogChoice()
        {
        }

        public DialogChoice(
            string text,
            string targetNodeId,
            DialogQuestAction questAction = null,
            DialogChoiceCondition condition = null)
        {
            _text = text;
            _targetNodeId = targetNodeId;
            _questAction = questAction ?? new DialogQuestAction();
            _condition = condition ?? new DialogChoiceCondition();
        }

        public string Text => _text;

        public string TargetNodeId => _targetNodeId;

        public DialogQuestAction QuestAction => _questAction;

        public DialogChoiceCondition Condition => _condition;
    }
}
