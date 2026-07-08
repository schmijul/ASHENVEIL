using Ashenveil.Core;
using UnityEngine;

namespace Ashenveil.Quests
{
    /// <summary>
    /// Item stack granted by a quest reward.
    /// Referenced GDD section: Kernsysteme / Inventar &amp; Handel.
    /// </summary>
    [System.Serializable]
    public sealed class QuestRewardItem
    {
        [Header("Reward Item")]
        [SerializeField] private ItemDefinition _item;
        [SerializeField] private int _amount = 1;

        public QuestRewardItem()
        {
        }

        public QuestRewardItem(ItemDefinition item, int amount)
        {
            _item = item;
            _amount = amount;
        }

        public ItemDefinition Item => _item;

        public int Amount => Mathf.Max(1, _amount);
    }
}
