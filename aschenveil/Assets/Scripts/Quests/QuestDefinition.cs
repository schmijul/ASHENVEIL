using System.Collections.Generic;
using UnityEngine;

namespace Ashenveil.Quests
{
    /// <summary>
    /// Authorable quest data used by the quest log model.
    /// Referenced GDD section: Kernsysteme / Dialog &amp; Quests.
    /// </summary>
    [CreateAssetMenu(fileName = "NewQuestDefinition", menuName = "Ashenveil/Quests/Quest Definition")]
    public sealed class QuestDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string _id = "quest_id";

        [Header("German Text")]
        [SerializeField] private string _title = "Quest";
        [TextArea]
        [SerializeField] private string _description = "Beschreibung";

        [Header("Objectives")]
        [SerializeField] private List<QuestObjectiveDefinition> _objectives = new List<QuestObjectiveDefinition>();

        [Header("Reward")]
        [SerializeField] private QuestRewardDefinition _reward = new QuestRewardDefinition();

        public string Id => _id;

        public string Title => _title;

        public string Description => _description;

        public IReadOnlyList<QuestObjectiveDefinition> Objectives => _objectives;

        public QuestRewardDefinition Reward => _reward;

        public void Configure(
            string id,
            string title,
            string description,
            IEnumerable<QuestObjectiveDefinition> objectives,
            QuestRewardDefinition reward)
        {
            _id = id;
            _title = title;
            _description = description;
            _objectives = new List<QuestObjectiveDefinition>(objectives ?? new QuestObjectiveDefinition[0]);
            _reward = reward ?? new QuestRewardDefinition();
        }
    }
}
