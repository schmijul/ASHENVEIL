using System;
using UnityEngine;

namespace Ashenveil.Combat
{
    /// <summary>
    /// Serializable attack timing and stamina data for a single combo step.
    /// </summary>
    [Serializable]
    public sealed class AttackStepDefinition
    {
        [Header("Animation")]
        public string AnimationTrigger = string.Empty;

        [Header("Combat")]
        [Min(0f)] public float DamageMultiplier = 1f;
        [Min(0f)] public float StaminaCost = 10f;
        [Min(0f)] public float WindUp = 0.1f;
        [Min(0f)] public float Recovery = 0.3f;
        [Min(0f)] public float ComboWindow = 0.6f;
        public bool CanChain = true;

        public static AttackStepDefinition CreateLightAttackStep(int comboIndex)
        {
            switch (Mathf.Clamp(comboIndex, 0, 2))
            {
                case 0:
                    return new AttackStepDefinition
                    {
                        AnimationTrigger = "LightAttack1",
                        DamageMultiplier = 1f,
                        StaminaCost = 10f,
                        WindUp = 0.1f,
                        Recovery = 0.3f,
                        ComboWindow = 0.6f,
                        CanChain = true
                    };
                case 1:
                    return new AttackStepDefinition
                    {
                        AnimationTrigger = "LightAttack2",
                        DamageMultiplier = 1.2f,
                        StaminaCost = 10f,
                        WindUp = 0.15f,
                        Recovery = 0.35f,
                        ComboWindow = 0.6f,
                        CanChain = true
                    };
                default:
                    return new AttackStepDefinition
                    {
                        AnimationTrigger = "LightAttack3",
                        DamageMultiplier = 1.5f,
                        StaminaCost = 15f,
                        WindUp = 0.2f,
                        Recovery = 0.5f,
                        ComboWindow = 0.6f,
                        CanChain = true
                    };
            }
        }

        public static AttackStepDefinition CreateHeavyAttackStep()
        {
            return new AttackStepDefinition
            {
                AnimationTrigger = "HeavyAttack",
                DamageMultiplier = 2f,
                StaminaCost = 25f,
                WindUp = 0.8f,
                Recovery = 0.6f,
                ComboWindow = 0f,
                CanChain = false
            };
        }

        public AttackStepDefinition Clone()
        {
            return new AttackStepDefinition
            {
                AnimationTrigger = AnimationTrigger,
                DamageMultiplier = DamageMultiplier,
                StaminaCost = StaminaCost,
                WindUp = WindUp,
                Recovery = Recovery,
                ComboWindow = ComboWindow,
                CanChain = CanChain
            };
        }
    }
}
