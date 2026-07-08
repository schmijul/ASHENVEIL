using System;
using System.Collections.Generic;
using Ashenveil.Core;

namespace Ashenveil.Quests
{
    /// <summary>
    /// Pure quest lifecycle and objective progress model.
    /// Referenced GDD section: Kernsysteme / Dialog &amp; Quests.
    /// </summary>
    public sealed class QuestLogModel : IQuestStateProvider, IQuestActionSink
    {
        private readonly Dictionary<string, QuestEntry> _entries = new Dictionary<string, QuestEntry>();
        private readonly IRewardSink _rewardSink;

        public QuestLogModel(IEnumerable<QuestDefinition> quests, IRewardSink rewardSink)
        {
            _rewardSink = rewardSink;

            foreach (QuestDefinition quest in quests ?? new QuestDefinition[0])
            {
                if (quest == null || string.IsNullOrWhiteSpace(quest.Id) || _entries.ContainsKey(quest.Id))
                {
                    continue;
                }

                _entries.Add(quest.Id, new QuestEntry(quest));
            }
        }

        public QuestState GetQuestState(string questId)
        {
            return _entries.TryGetValue(questId, out QuestEntry entry)
                ? entry.State
                : QuestState.Inactive;
        }

        public int GetObjectiveProgress(string questId, string objectiveId)
        {
            if (!_entries.TryGetValue(questId, out QuestEntry entry))
            {
                return 0;
            }

            return entry.Progress.TryGetValue(objectiveId, out int count) ? count : 0;
        }

        public bool StartQuest(string questId)
        {
            if (!TryGetEntry(questId, out QuestEntry entry) || entry.State != QuestState.Inactive)
            {
                return false;
            }

            SetState(entry, QuestState.Active);
            CompleteObjectivesIfReady(entry);
            return true;
        }

        public bool AdvanceQuest(string questId)
        {
            if (!TryGetEntry(questId, out QuestEntry entry) || entry.State != QuestState.Active)
            {
                return false;
            }

            SetState(entry, QuestState.ObjectivesComplete);
            return true;
        }

        public bool ReportProgress(string questId, string objectiveId, int amount)
        {
            if (!TryGetEntry(questId, out QuestEntry entry)
                || entry.State != QuestState.Active
                || amount <= 0
                || !entry.RequiredCounts.ContainsKey(objectiveId))
            {
                return false;
            }

            int current = entry.Progress[objectiveId];
            int required = entry.RequiredCounts[objectiveId];
            entry.Progress[objectiveId] = Math.Min(required, current + amount);
            CompleteObjectivesIfReady(entry);
            return true;
        }

        public bool CompleteQuest(string questId)
        {
            if (!TryGetEntry(questId, out QuestEntry entry) || entry.State != QuestState.ObjectivesComplete)
            {
                return false;
            }

            GrantRewards(entry);
            SetState(entry, QuestState.Completed);
            return true;
        }

        private bool TryGetEntry(string questId, out QuestEntry entry)
        {
            if (string.IsNullOrWhiteSpace(questId))
            {
                entry = null;
                return false;
            }

            return _entries.TryGetValue(questId, out entry);
        }

        private void CompleteObjectivesIfReady(QuestEntry entry)
        {
            foreach (KeyValuePair<string, int> requiredCount in entry.RequiredCounts)
            {
                if (entry.Progress[requiredCount.Key] < requiredCount.Value)
                {
                    return;
                }
            }

            SetState(entry, QuestState.ObjectivesComplete);
        }

        private void GrantRewards(QuestEntry entry)
        {
            if (entry.RewardGranted || _rewardSink == null)
            {
                entry.RewardGranted = true;
                return;
            }

            if (entry.Reward.Gold > 0)
            {
                _rewardSink.GrantGold(entry.Reward.Gold);
            }

            foreach (QuestRewardItem rewardItem in entry.Reward.Items)
            {
                if (rewardItem.Item != null)
                {
                    _rewardSink.GrantItem(rewardItem.Item, rewardItem.Amount);
                }
            }

            entry.RewardGranted = true;
        }

        private void SetState(QuestEntry entry, QuestState nextState)
        {
            if (entry.State == nextState)
            {
                return;
            }

            entry.State = nextState;
            GameSignals.RaiseQuestStateChanged(entry.Id, nextState.ToString());
        }

        private sealed class QuestEntry
        {
            public QuestEntry(QuestDefinition definition)
            {
                Id = definition.Id;
                Reward = definition.Reward ?? new QuestRewardDefinition();

                foreach (QuestObjectiveDefinition objective in definition.Objectives)
                {
                    if (objective == null || string.IsNullOrWhiteSpace(objective.Id) || RequiredCounts.ContainsKey(objective.Id))
                    {
                        continue;
                    }

                    RequiredCounts.Add(objective.Id, objective.RequiredCount);
                    Progress.Add(objective.Id, 0);
                }
            }

            public string Id { get; }

            public QuestState State { get; set; }

            public Dictionary<string, int> RequiredCounts { get; } = new Dictionary<string, int>();

            public Dictionary<string, int> Progress { get; } = new Dictionary<string, int>();

            public QuestRewardDefinition Reward { get; }

            public bool RewardGranted { get; set; }
        }
    }
}
