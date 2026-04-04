namespace Ashenveil.Combat
{
    /// <summary>
    /// High-level combat state used by the runtime controller.
    /// </summary>
    public enum CombatActionKind
    {
        Idle = 0,
        LightAttack = 1,
        HeavyAttack = 2,
        Blocking = 3,
        Dodging = 4,
        GuardBroken = 5
    }
}
