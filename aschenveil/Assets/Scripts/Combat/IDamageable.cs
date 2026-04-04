namespace Ashenveil.Combat
{
    /// <summary>
    /// Contract for anything that can receive combat damage and stagger.
    /// </summary>
    public interface IDamageable
    {
        float CurrentHealth { get; }

        float MaxHealth { get; }

        float Armor { get; }

        bool IsDead { get; }

        void TakeDamage(float amount, DamageType damageType);

        void ApplyStagger(float duration);
    }
}
