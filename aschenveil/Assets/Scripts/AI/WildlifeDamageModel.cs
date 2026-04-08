using UnityEngine;
using Ashenveil.Combat;

namespace Ashenveil.AI
{
    /// <summary>
    /// Pure resistance math for wildlife damage intake.
    /// </summary>
    public sealed class WildlifeDamageModel
    {
        private readonly WildlifeDamageResistance[] _resistances;

        public WildlifeDamageModel(WildlifeDamageResistance[] resistances)
        {
            if (resistances == null || resistances.Length == 0)
            {
                _resistances = new WildlifeDamageResistance[0];
            }
            else
            {
                _resistances = new WildlifeDamageResistance[resistances.Length];
                for (int index = 0; index < resistances.Length; index++)
                {
                    _resistances[index] = resistances[index] != null ? resistances[index].Clone() : null;
                }
            }
        }

        public float GetMultiplier(DamageType damageType)
        {
            if (_resistances == null)
            {
                return 1f;
            }

            for (int index = 0; index < _resistances.Length; index++)
            {
                WildlifeDamageResistance resistance = _resistances[index];
                if (resistance != null && resistance.DamageType == damageType)
                {
                    return Mathf.Max(0f, resistance.Multiplier);
                }
            }

            return 1f;
        }

        public float CalculateDamage(float amount, DamageType damageType)
        {
            float sanitizedAmount = Mathf.Max(0f, amount);
            if (damageType == DamageType.True)
            {
                return sanitizedAmount;
            }

            float multipliedAmount = sanitizedAmount * GetMultiplier(damageType);
            return Mathf.Max(multipliedAmount, sanitizedAmount > 0f ? 1f : 0f);
        }
    }
}
