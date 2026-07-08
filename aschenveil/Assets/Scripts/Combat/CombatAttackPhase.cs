namespace Ashenveil.Combat
{
    /// <summary>
    /// Current phase of a melee attack.
    /// Referenced GDD section: Kernsysteme / Combat.
    /// </summary>
    public enum CombatAttackPhase
    {
        /// <summary>
        /// No attack is running.
        /// </summary>
        Idle,

        /// <summary>
        /// Startup before the weapon can hit.
        /// </summary>
        Windup,

        /// <summary>
        /// Active hit frames where the weapon hitbox can apply damage.
        /// </summary>
        Active,

        /// <summary>
        /// Ending frames before another unqueued action can start.
        /// </summary>
        Recovery
    }
}
