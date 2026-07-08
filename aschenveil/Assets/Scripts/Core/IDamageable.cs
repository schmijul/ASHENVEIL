namespace Ashenveil.Core
{
    /// <summary>
    /// Contract for anything that can receive combat or aether damage.
    /// Referenced GDD section: Kernsysteme / Combat.
    /// </summary>
    public interface IDamageable
    {
        /// <summary>
        /// Indicates whether this target can still act and receive meaningful damage.
        /// </summary>
        bool IsAlive { get; }

        /// <summary>
        /// Applies a typed damage payload to this target.
        /// </summary>
        void TakeDamage(DamageInfo damageInfo);
    }
}
