using UnityEngine;

namespace Ashenveil.Quests
{
    /// <summary>
    /// Authorable quest objective data.
    /// Referenced GDD section: Kernsysteme / Dialog &amp; Quests.
    /// </summary>
    [System.Serializable]
    public sealed class QuestObjectiveDefinition
    {
        [Header("Identity")]
        [SerializeField] private string _id = "objective_id";

        [Header("Text")]
        [SerializeField] private string _text = "Ziel";

        [Header("Progress")]
        [SerializeField] private int _requiredCount = 1;

        public QuestObjectiveDefinition()
        {
        }

        public QuestObjectiveDefinition(string id, string text, int requiredCount)
        {
            _id = id;
            _text = text;
            _requiredCount = requiredCount;
        }

        public string Id => _id;

        public string Text => _text;

        public int RequiredCount => Mathf.Max(1, _requiredCount);
    }
}
