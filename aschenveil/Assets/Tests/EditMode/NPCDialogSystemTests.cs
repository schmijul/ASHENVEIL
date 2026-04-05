using Ashenveil.Dialog;
using Ashenveil.NPC;
using NUnit.Framework;

namespace Ashenveil.Tests.EditMode
{
    public class NPCDialogSystemTests
    {
        [Test]
        public void Resolve_ReturnsMatchingScheduleEntry_ForActiveHour()
        {
            NPCScheduleEntry[] schedule =
            {
                NPCScheduleEntry.Create(6f, 10f, NPCAction.Idle),
                NPCScheduleEntry.Create(10f, 18f, NPCAction.Work),
                NPCScheduleEntry.Create(18f, 22f, NPCAction.Walk)
            };

            NPCScheduleEntry entry = NpcScheduleResolver.Resolve(schedule, 11f);

            Assert.That(entry, Is.Not.Null);
            Assert.That(entry.Action, Is.EqualTo(NPCAction.Work));
        }

        [Test]
        public void Resolve_ReturnsNearestPreviousEntry_WhenHourFallsInGap()
        {
            NPCScheduleEntry[] schedule =
            {
                NPCScheduleEntry.Create(6f, 10f, NPCAction.Idle),
                NPCScheduleEntry.Create(12f, 18f, NPCAction.Work),
                NPCScheduleEntry.Create(18f, 22f, NPCAction.Walk)
            };

            NPCScheduleEntry entry = NpcScheduleResolver.Resolve(schedule, 10.5f);

            Assert.That(entry, Is.Not.Null);
            Assert.That(entry.Action, Is.EqualTo(NPCAction.Idle));
        }

        [Test]
        public void SelectChoice_UsesConditionAndAdvancesToTargetNode()
        {
            DialogContext context = new DialogContext();
            context.AddItem("moonpetal", 5);

            DialogTree tree = DialogTree.CreateRuntimeDefaults();
            tree.SetNodes(
                new[]
                {
                    DialogNode.Create(
                        0,
                        "Healer",
                        "Could you gather five moonpetals for me?",
                        choices:
                        new[]
                        {
                            DialogChoice.Create("I have them.", 1, DialogCondition.Create(DialogConditionType.HasItem, "moonpetal", 5)),
                            DialogChoice.Create("Not yet.", 2)
                        }),
                    DialogNode.Create(1, "Healer", "Thank you.", nextNode: -1),
                    DialogNode.Create(2, "Healer", "Come back when you have them.", nextNode: -1)
                },
                fallbackNodeId: 2);

            DialogConversation conversation = new DialogConversation();
            bool begun = conversation.Begin(tree, 0, context);

            Assert.That(begun, Is.True);
            Assert.That(conversation.CurrentNode.NodeId, Is.EqualTo(0));
            Assert.That(conversation.CurrentChoices.Count, Is.EqualTo(2));

            bool selected = conversation.SelectChoice(0);

            Assert.That(selected, Is.True);
            Assert.That(conversation.CurrentNode.NodeId, Is.EqualTo(1));
        }

        [Test]
        public void Advance_RaisesQuestTriggerAndMovesToNextNode()
        {
            QuestTrigger trigger = QuestTrigger.Create(QuestTriggerType.StartQuest, "the-hunt");
            DialogTree tree = DialogTree.CreateRuntimeDefaults();
            int questTriggerCount = 0;

            tree.SetNodes(
                new[]
                {
                    DialogNode.Create(
                        0,
                        "Elder",
                        "The village needs meat.",
                        nextNode: 1,
                        questTrigger: trigger),
                    DialogNode.Create(1, "Elder", "Head north into the Grauwald.", nextNode: -1)
                });

            DialogConversation conversation = new DialogConversation();
            conversation.QuestTriggerRaised += _ => questTriggerCount++;

            bool begun = conversation.Begin(tree);
            bool advanced = conversation.Advance();

            Assert.That(begun, Is.True);
            Assert.That(advanced, Is.True);
            Assert.That(questTriggerCount, Is.EqualTo(1));
            Assert.That(conversation.CurrentNode.NodeId, Is.EqualTo(1));
        }

        [Test]
        public void Begin_OneTimeOnlyNode_ReusesFallbackAfterFirstVisit()
        {
            DialogTree tree = DialogTree.CreateRuntimeDefaults();
            tree.SetNodes(
                new[]
                {
                    DialogNode.Create(0, "Trader", "Welcome!", nextNode: -1, oneTimeOnly: true),
                    DialogNode.Create(1, "Trader", "Come back anytime.", nextNode: -1)
                },
                fallbackNodeId: 1);

            DialogConversation conversation = new DialogConversation();

            bool firstBegin = conversation.Begin(tree, 0);
            conversation.Reset();
            bool secondBegin = conversation.Begin(tree, 0);

            Assert.That(firstBegin, Is.True);
            Assert.That(secondBegin, Is.True);
            Assert.That(conversation.CurrentNode.NodeId, Is.EqualTo(1));
        }
    }
}
