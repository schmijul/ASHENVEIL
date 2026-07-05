namespace Ashenveil.AI.Boss
{
    /// <summary>
    /// Behavioural phases of the mutated wolf boss.
    /// </summary>
    public enum BossPhase
    {
        /// <summary>Circling and stalking the player.</summary>
        Stalk,

        /// <summary>Committed to closing distance and lunging.</summary>
        Lunge,

        /// <summary>Low-health frenzy: faster, more aggressive.</summary>
        Enrage,

        /// <summary>Defeated.</summary>
        Dead
    }

    /// <summary>
    /// Discrete actions the boss can take on a given tick.
    /// </summary>
    public enum BossAction
    {
        /// <summary>No action this tick.</summary>
        None,

        /// <summary>Dashing bite attack.</summary>
        Lunge,

        /// <summary>Close-range claw swipe.</summary>
        Swipe
    }
}
