namespace Ashenveil.AI
{
    /// <summary>
    /// Current deterministic wildlife behavior state.
    /// Referenced GDD section: Kernsysteme / Wildlife.
    /// </summary>
    public enum WildlifeBrainState
    {
        /// <summary>
        /// Standing still.
        /// </summary>
        Idle,

        /// <summary>
        /// Moving without a direct threat.
        /// </summary>
        Wander,

        /// <summary>
        /// Recently lost or noticed a threat.
        /// </summary>
        Alert,

        /// <summary>
        /// Moving away from a threat.
        /// </summary>
        Flee,

        /// <summary>
        /// Moving toward a threat.
        /// </summary>
        Chase,

        /// <summary>
        /// In range and attempting attacks.
        /// </summary>
        Attack,

        /// <summary>
        /// Dead terminal state.
        /// </summary>
        Dead
    }
}
