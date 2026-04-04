using UnityEngine;

namespace Ashenveil.Combat
{
    /// <summary>
    /// Pure damage math for melee combat interactions.
    /// </summary>
    public static class DamageResolver
    {
        public static float CalculateFinalDamage(float baseWeaponDamage, float attackMultiplier, float targetArmor)
        {
            float rawDamage = (Mathf.Max(0f, baseWeaponDamage) * Mathf.Max(0f, attackMultiplier)) - Mathf.Max(0f, targetArmor);
            return Mathf.Max(rawDamage, 1f);
        }

        public static float CalculateFinalDamage(WeaponData weaponData, AttackStepDefinition attackStep, float targetArmor)
        {
            if (weaponData == null || attackStep == null)
            {
                return 1f;
            }

            return CalculateFinalDamage(weaponData.BaseDamage, attackStep.DamageMultiplier, targetArmor);
        }
    }
}
