using Ashenveil.Dialog;
using UnityEngine;

namespace Ashenveil.Quests
{
    /// <summary>
    /// Runtime-callable factories for demo quest and dialog content.
    /// Referenced GDD section: Demo-Ablauf / NPC-Dialoge.
    /// </summary>
    public static class QuestContentLibrary
    {
        public const string HerbalistQuestId = "quest_kraeuter_heilerin";
        public const string HerbalistObjectiveId = "collect_blutmoos";
        public const string LostToolQuestId = "quest_verlorenes_werkzeug";
        public const string LostToolObjectiveId = "find_schmiedehammer";

        public static void CreateHerbalistQuestContent(out QuestDefinition quest, out DialogGraph dialog)
        {
            quest = ScriptableObject.CreateInstance<QuestDefinition>();
            quest.name = "Quest_KraeuterFuerDieHeilerin";
            quest.Configure(
                HerbalistQuestId,
                "Kräuter für die Heilerin",
                "Die Heilerin braucht Blutmoos aus dem Wald, um Wunden im Dorf zu versorgen.",
                new[]
                {
                    new QuestObjectiveDefinition(HerbalistObjectiveId, "Sammle 5 Blutmoos.", 5)
                },
                new QuestRewardDefinition(25, new QuestRewardItem[0]));

            dialog = ScriptableObject.CreateInstance<DialogGraph>();
            dialog.name = "Dialog_Heilerin_Kraeuter";
            dialog.Configure(
                "dialog_heilerin_kraeuter",
                "entry",
                new[]
                {
                    new DialogNode(
                        "entry",
                        "Heilerin",
                        "Du siehst aus, als könntest du einen klaren Auftrag gebrauchen. Ich brauche Blutmoos aus dem Wald.",
                        new[]
                        {
                            new DialogChoice(
                                "Ich sammle das Blutmoos.",
                                "accepted",
                                new DialogQuestAction(QuestActionType.StartQuest, HerbalistQuestId),
                                new DialogChoiceCondition(HerbalistQuestId, QuestState.Inactive)),
                            new DialogChoice(
                                "Ich habe dein Blutmoos.",
                                "completed",
                                new DialogQuestAction(QuestActionType.CompleteQuest, HerbalistQuestId),
                                new DialogChoiceCondition(HerbalistQuestId, QuestState.ObjectivesComplete)),
                            new DialogChoice(
                                "Ich suche noch weiter.",
                                "active",
                                null,
                                new DialogChoiceCondition(HerbalistQuestId, QuestState.Active)),
                            new DialogChoice("Leb wohl.", "end")
                        }),
                    new DialogNode(
                        "accepted",
                        "Heilerin",
                        "Gut. Bring mir fünf Büschel Blutmoos. Es wächst dort, wo der Boden dunkel und feucht ist.",
                        new DialogChoice[0]),
                    new DialogNode(
                        "active",
                        "Heilerin",
                        "Ohne Blutmoos kann ich kaum jemanden behandeln. Bitte beeil dich.",
                        new DialogChoice[0]),
                    new DialogNode(
                        "completed",
                        "Heilerin",
                        "Das ist genug. Du hast dem Dorf heute geholfen.",
                        new DialogChoice[0]),
                    new DialogNode(
                        "end",
                        "Heilerin",
                        "Dann geh vorsichtig.",
                        new DialogChoice[0])
                });
        }

        public static void CreateLostToolQuestContent(out QuestDefinition quest, out DialogGraph dialog)
        {
            quest = ScriptableObject.CreateInstance<QuestDefinition>();
            quest.name = "Quest_DasVerloreneWerkzeug";
            quest.Configure(
                LostToolQuestId,
                "Das verlorene Werkzeug",
                "Der Schmied hat seinen Hammer am Fluss verloren und kann ohne ihn keine Waffen richten.",
                new[]
                {
                    new QuestObjectiveDefinition(LostToolObjectiveId, "Finde den Schmiedehammer nahe beim Fluss.", 1)
                },
                new QuestRewardDefinition(35, new QuestRewardItem[0]));

            dialog = ScriptableObject.CreateInstance<DialogGraph>();
            dialog.name = "Dialog_Schmied_Werkzeug";
            dialog.Configure(
                "dialog_schmied_werkzeug",
                "entry",
                new[]
                {
                    new DialogNode(
                        "entry",
                        "Schmied",
                        "Verdammter Fluss. Mein Hammer liegt irgendwo unten am Ufer.",
                        new[]
                        {
                            new DialogChoice(
                                "Ich sehe nach deinem Hammer.",
                                "accepted",
                                new DialogQuestAction(QuestActionType.StartQuest, LostToolQuestId),
                                new DialogChoiceCondition(LostToolQuestId, QuestState.Inactive)),
                            new DialogChoice(
                                "Ich habe den Schmiedehammer gefunden.",
                                "completed",
                                new DialogQuestAction(QuestActionType.CompleteQuest, LostToolQuestId),
                                new DialogChoiceCondition(LostToolQuestId, QuestState.ObjectivesComplete)),
                            new DialogChoice(
                                "Ich suche noch am Fluss.",
                                "active",
                                null,
                                new DialogChoiceCondition(LostToolQuestId, QuestState.Active)),
                            new DialogChoice("Später.", "end")
                        }),
                    new DialogNode(
                        "accepted",
                        "Schmied",
                        "Such bei den flachen Steinen. Wenn du ihn findest, mache ich es dir nicht umsonst.",
                        new DialogChoice[0]),
                    new DialogNode(
                        "active",
                        "Schmied",
                        "Ohne den Hammer bleibt die Esse kalt. Er muss noch am Ufer liegen.",
                        new DialogChoice[0]),
                    new DialogNode(
                        "completed",
                        "Schmied",
                        "Da ist er. Gut gemacht. Jetzt kann ich wieder arbeiten.",
                        new DialogChoice[0]),
                    new DialogNode(
                        "end",
                        "Schmied",
                        "Dann steh mir nicht im Licht.",
                        new DialogChoice[0])
                });
        }
    }
}
