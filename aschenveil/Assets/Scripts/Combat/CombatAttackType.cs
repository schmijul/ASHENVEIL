namespace Ashenveil.Combat
{
    /// <summary>
    /// Kind of melee attack currently running or requested.
    /// Referenced GDD section: Kernsysteme / Combat.
    /// </summary>
    public enum CombatAttackType
    {
        /// <summary>
        /// No attack.
        /// </summary>
        None,

        /// <summary>
        /// Fast, low-cost attack.
        /// </summary>
        Light,

        /// <summary>
        /// Slower, higher-cost attack.
        /// </summary>
        Heavy
    }
}
