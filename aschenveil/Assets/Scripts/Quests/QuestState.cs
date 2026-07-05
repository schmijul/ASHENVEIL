namespace Ashenveil.Quests
{
    /// <summary>
    /// Runtime lifecycle for a quest entry.
    /// Referenced GDD section: Kernsysteme / Dialog &amp; Quests.
    /// </summary>
    public enum QuestState
    {
        Inactive,
        Active,
        ObjectivesComplete,
        Completed
    }
}
