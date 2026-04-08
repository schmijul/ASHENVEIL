namespace Ashenveil.AI
{
    /// <summary>
    /// Core wildlife state machine states.
    /// </summary>
    public enum WildlifeState
    {
        Idle = 0,
        Patrol = 1,
        Graze = 2,
        Alert = 3,
        Flee = 4,
        Chase = 5,
        Attack = 6,
        Dead = 7
    }
}
