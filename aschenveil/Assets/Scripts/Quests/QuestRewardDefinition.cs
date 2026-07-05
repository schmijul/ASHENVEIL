using System.Collections.Generic;
using UnityEngine;

namespace Ashenveil.Quests
{
    /// <summary>
    /// Gold and item reward data for a quest.
    /// Referenced GDD section: Kernsysteme / Dialog &amp; Quests.
    /// </summary>
    [System.Serializable]
    public sealed class QuestRewardDefinition
    {
        [Header("Gold")]
        [SerializeField] private int _gold;

        [Header("Items")]
        [SerializeField] private List<QuestRewardItem> _items = new List<QuestRewardItem>();

        public QuestRewardDefinition()
        {
        }

        public QuestRewardDefinition(int gold, IEnumerable<QuestRewardItem> items)
        {
            _gold = gold;
            _items = new List<QuestRewardItem>(items ?? new QuestRewardItem[0]);
        }

        public int Gold => Mathf.Max(0, _gold);

        public IReadOnlyList<QuestRewardItem> Items => _items;
    }
}
