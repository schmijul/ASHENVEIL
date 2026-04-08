using System;
using UnityEngine;
using Ashenveil.Combat;

namespace Ashenveil.AI
{
    /// <summary>
    /// Serializable attack data for wildlife combat behaviors.
    /// </summary>
    [Serializable]
    public sealed class WildlifeAttackDefinition
    {
        [Header("Identity")]
        public string AnimationTrigger = string.Empty;
        public WildlifeAttackKind AttackKind = WildlifeAttackKind.None;
        public DamageType DamageType = DamageType.Physical;

        [Header("Combat")]
        [Min(0f)] public float Damage = 0f;
        [Min(0f)] public float Range = 2f;
        [Min(0f)] public float WindUp = 0.2f;
        [Min(0f)] public float Recovery = 0.4f;
        [Min(0f)] public float Cooldown = 0f;
        [Min(0f)] public float AreaRadius = 0f;
        [Min(0f)] public float SpeedMultiplier = 1f;
        [Min(0f)] public float HealthThreshold = 0f;
        public bool RequiresRearApproach;
        public bool OnlyOncePerFight;

        public WildlifeAttackDefinition Clone()
        {
            return new WildlifeAttackDefinition
            {
                AnimationTrigger = AnimationTrigger,
                AttackKind = AttackKind,
                DamageType = DamageType,
                Damage = Damage,
                Range = Range,
                WindUp = WindUp,
                Recovery = Recovery,
                Cooldown = Cooldown,
                AreaRadius = AreaRadius,
                SpeedMultiplier = SpeedMultiplier,
                HealthThreshold = HealthThreshold,
                RequiresRearApproach = RequiresRearApproach,
                OnlyOncePerFight = OnlyOncePerFight
            };
        }
    }
}
