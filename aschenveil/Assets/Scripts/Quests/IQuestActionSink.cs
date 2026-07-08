namespace Ashenveil.Quests
{
    /// <summary>
    /// Quest command surface used by dialog choices.
    /// Referenced GDD section: Kernsysteme / Dialog &amp; Quests.
    /// </summary>
    public interface IQuestActionSink
    {
        bool StartQuest(string questId);

        bool AdvanceQuest(string questId);

        bool CompleteQuest(string questId);
    }
}
