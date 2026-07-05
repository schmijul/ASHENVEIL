using System.Collections.Generic;
using Ashenveil.Core;
using Ashenveil.Quests;
using NUnit.Framework;
using UnityEngine;

namespace Ashenveil.Tests.EditMode
{
    /// <summary>
    /// EditMode tests for quest lifecycle logic.
    /// </summary>
    public sealed class QuestLogModelTests
    {
        /// <summary>
        /// Verifies start, accumulated progress, objective completion, and final completion transitions.
        /// </summary>
        [Test]
        public void QuestLifecycle_ProgressAccumulation_CompletesObjectives()
        {
            List<string> stateChanges = new List<string>();
            void OnQuestStateChanged(string questId, string stateId)
            {
                stateChanges.Add($"{questId}:{stateId}");
            }

            QuestDefinition quest = CreateQuest("quest_herbs", "collect", 3, new QuestRewardDefinition());
            QuestLogModel model = new QuestLogModel(new[] { quest }, new FakeRewardSink());

            GameSignals.QuestStateChanged += OnQuestStateChanged;
            try
            {
                Assert.That(model.StartQuest("quest_herbs"), Is.True);
                Assert.That(model.ReportProgress("quest_herbs", "collect", 1), Is.True);
                Assert.That(model.GetObjectiveProgress("quest_herbs", "collect"), Is.EqualTo(1));
                Assert.That(model.ReportProgress("quest_herbs", "collect", 2), Is.True);
            }
            finally
            {
                GameSignals.QuestStateChanged -= OnQuestStateChanged;
            }

            Assert.That(model.GetQuestState("quest_herbs"), Is.EqualTo(QuestState.ObjectivesComplete));
            Assert.That(model.GetObjectiveProgress("quest_herbs", "collect"), Is.EqualTo(3));
            Assert.That(stateChanges, Is.EqualTo(new[]
            {
                "quest_herbs:Active",
                "quest_herbs:ObjectivesComplete"
            }));
        }

        /// <summary>
        /// Verifies quest rewards are granted only once.
        /// </summary>
        [Test]
        public void CompleteQuest_Reward_DispatchesOnce()
        {
            ItemDefinition item = ScriptableObject.CreateInstance<ItemDefinition>();
            QuestRewardDefinition reward = new QuestRewardDefinition(
                20,
                new[] { new QuestRewardItem(item, 2) });
            QuestDefinition quest = CreateQuest("quest_reward", "done", 1, reward);
            FakeRewardSink rewardSink = new FakeRewardSink();
            QuestLogModel model = new QuestLogModel(new[] { quest }, rewardSink);

            model.StartQuest("quest_reward");
            model.ReportProgress("quest_reward", "done", 1);

            Assert.That(model.CompleteQuest("quest_reward"), Is.True);
            Assert.That(model.CompleteQuest("quest_reward"), Is.False);

            Assert.That(rewardSink.Gold, Is.EqualTo(20));
            Assert.That(rewardSink.ItemsGranted, Is.EqualTo(1));
            Assert.That(rewardSink.LastItem, Is.EqualTo(item));
            Assert.That(rewardSink.LastItemAmount, Is.EqualTo(2));
        }

        /// <summary>
        /// Verifies invalid lifecycle calls are rejected.
        /// </summary>
        [Test]
        public void InvalidTransitions_AreRejected()
        {
            QuestDefinition quest = CreateQuest("quest_tool", "find", 1, new QuestRewardDefinition());
            QuestLogModel model = new QuestLogModel(new[] { quest }, new FakeRewardSink());

            Assert.That(model.CompleteQuest("quest_tool"), Is.False);
            Assert.That(model.ReportProgress("quest_tool", "find", 1), Is.False);
            Assert.That(model.StartQuest("missing"), Is.False);
            Assert.That(model.StartQuest("quest_tool"), Is.True);
            Assert.That(model.StartQuest("quest_tool"), Is.False);
            Assert.That(model.ReportProgress("quest_tool", "unknown", 1), Is.False);
        }

        private static QuestDefinition CreateQuest(
            string questId,
            string objectiveId,
            int requiredCount,
            QuestRewardDefinition reward)
        {
            QuestDefinition quest = ScriptableObject.CreateInstance<QuestDefinition>();
            quest.Configure(
                questId,
                "Testquest",
                "Beschreibung",
                new[] { new QuestObjectiveDefinition(objectiveId, "Ziel", requiredCount) },
                reward);
            return quest;
        }

        private sealed class FakeRewardSink : IRewardSink
        {
            public int Gold { get; private set; }

            public int ItemsGranted { get; private set; }

            public ItemDefinition LastItem { get; private set; }

            public int LastItemAmount { get; private set; }

            public void GrantGold(int amount)
            {
                Gold += amount;
            }

            public void GrantItem(ItemDefinition item, int amount)
            {
                ItemsGranted++;
                LastItem = item;
                LastItemAmount = amount;
            }
        }
    }
}
