using System.Collections.Generic;
using Ashenveil.Core;
using Ashenveil.Dialog;
using Ashenveil.Quests;
using NUnit.Framework;
using UnityEngine;

namespace Ashenveil.Tests.EditMode
{
    /// <summary>
    /// EditMode tests for dialog traversal logic.
    /// </summary>
    public sealed class DialogRunnerModelTests
    {
        /// <summary>
        /// Verifies player choices move to their configured target node.
        /// </summary>
        [Test]
        public void Choose_SecondBranch_MovesToTargetNode()
        {
            DialogGraph graph = CreateGraph(
                "branch_dialog",
                new DialogNode(
                    "entry",
                    "Alva",
                    "Wähle einen Weg.",
                    new[]
                    {
                        new DialogChoice("Links.", "left"),
                        new DialogChoice("Rechts.", "right")
                    }),
                new DialogNode("left", "Alva", "Linker Weg.", new DialogChoice[0]),
                new DialogNode("right", "Alva", "Rechter Weg.", new DialogChoice[0]));

            DialogRunnerModel model = new DialogRunnerModel(graph, null, null);

            bool chosen = model.Choose(1);

            Assert.That(chosen, Is.True);
            Assert.That(model.CurrentNode.Id, Is.EqualTo("right"));
            Assert.That(model.CurrentNode.Text, Is.EqualTo("Rechter Weg."));
        }

        /// <summary>
        /// Verifies choices are filtered by quest state conditions.
        /// </summary>
        [Test]
        public void AvailableChoices_QuestConditions_FiltersByRequiredState()
        {
            FakeQuestStateProvider stateProvider = new FakeQuestStateProvider();
            stateProvider.SetState("quest", QuestState.Active);
            DialogGraph graph = CreateGraph(
                "condition_dialog",
                new DialogNode(
                    "entry",
                    "Heilerin",
                    "Wie steht es um die Aufgabe?",
                    new[]
                    {
                        new DialogChoice(
                            "Ich bin dabei.",
                            "active",
                            null,
                            new DialogChoiceCondition("quest", QuestState.Active)),
                        new DialogChoice(
                            "Es ist erledigt.",
                            "complete",
                            null,
                            new DialogChoiceCondition("quest", QuestState.ObjectivesComplete))
                    }),
                new DialogNode("active", "Heilerin", "Beeil dich.", new DialogChoice[0]),
                new DialogNode("complete", "Heilerin", "Gut.", new DialogChoice[0]));

            DialogRunnerModel model = new DialogRunnerModel(graph, stateProvider, null);

            Assert.That(model.AvailableChoices, Has.Count.EqualTo(1));
            Assert.That(model.AvailableChoices[0].Text, Is.EqualTo("Ich bin dabei."));
        }

        /// <summary>
        /// Verifies quest actions are dispatched before advancing the dialog.
        /// </summary>
        [Test]
        public void Choose_QuestAction_DispatchesToSink()
        {
            FakeQuestActionSink actionSink = new FakeQuestActionSink();
            DialogGraph graph = CreateGraph(
                "action_dialog",
                new DialogNode(
                    "entry",
                    "Schmied",
                    "Hilfst du?",
                    new[]
                    {
                        new DialogChoice(
                            "Ja.",
                            "end",
                            new DialogQuestAction(QuestActionType.StartQuest, "quest_tool"))
                    }),
                new DialogNode("end", "Schmied", "Gut.", new DialogChoice[0]));
            DialogRunnerModel model = new DialogRunnerModel(graph, null, actionSink);

            model.Choose(0);

            Assert.That(actionSink.StartedQuestId, Is.EqualTo("quest_tool"));
        }

        /// <summary>
        /// Verifies reaching an end node finishes the dialog and raises the signal.
        /// </summary>
        [Test]
        public void Choose_EndNode_RaisesDialogFinished()
        {
            string finishedDialogId = "";
            void OnDialogFinished(string dialogId)
            {
                finishedDialogId = dialogId;
            }

            DialogGraph graph = CreateGraph(
                "ending_dialog",
                new DialogNode(
                    "entry",
                    "Alva",
                    "Noch etwas?",
                    new[] { new DialogChoice("Nein.", "end") }),
                new DialogNode("end", "Alva", "Dann geh.", new DialogChoice[0]));
            DialogRunnerModel model = new DialogRunnerModel(graph, null, null);

            GameSignals.DialogFinished += OnDialogFinished;
            try
            {
                model.Choose(0);
            }
            finally
            {
                GameSignals.DialogFinished -= OnDialogFinished;
            }

            Assert.That(model.IsFinished, Is.True);
            Assert.That(finishedDialogId, Is.EqualTo("ending_dialog"));
        }

        private static DialogGraph CreateGraph(string dialogId, params DialogNode[] nodes)
        {
            DialogGraph graph = ScriptableObject.CreateInstance<DialogGraph>();
            graph.Configure(dialogId, "entry", nodes);
            return graph;
        }

        private sealed class FakeQuestStateProvider : IQuestStateProvider
        {
            private readonly Dictionary<string, QuestState> _states = new Dictionary<string, QuestState>();

            public void SetState(string questId, QuestState state)
            {
                _states[questId] = state;
            }

            public QuestState GetQuestState(string questId)
            {
                return _states.TryGetValue(questId, out QuestState state) ? state : QuestState.Inactive;
            }
        }

        private sealed class FakeQuestActionSink : IQuestActionSink
        {
            public string StartedQuestId { get; private set; }

            public bool StartQuest(string questId)
            {
                StartedQuestId = questId;
                return true;
            }

            public bool AdvanceQuest(string questId)
            {
                return true;
            }

            public bool CompleteQuest(string questId)
            {
                return true;
            }
        }
    }
}
