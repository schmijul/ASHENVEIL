namespace Ashenveil.Quests
{
    /// <summary>
    /// Read-only quest state lookup used by dialog conditions.
    /// Referenced GDD section: Kernsysteme / Dialog &amp; Quests.
    /// </summary>
    public interface IQuestStateProvider
    {
        QuestState GetQuestState(string questId);
    }
}
