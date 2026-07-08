using System.Collections.Generic;
using UnityEngine;

namespace Ashenveil.AI
{
    /// <summary>
    /// ScriptableObject containing movement, perception, health, combat, and loot data for wildlife.
    /// Referenced GDD section: Kernsysteme / Wildlife.
    /// </summary>
    [CreateAssetMenu(fileName = "NewWildlifeSpecies", menuName = "Ashenveil/AI/Wildlife Species")]
    public sealed class WildlifeSpecies : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private WildlifeSpeciesId _speciesId = WildlifeSpeciesId.Deer;

        [Header("Movement")]
        [SerializeField] private float _walkSpeed = 1.6f;
        [SerializeField] private float _runSpeed = 4.8f;
        [SerializeField] private float _gravity = -18f;

        [Header("Perception")]
        [SerializeField] private float _perceptionRadius = 9f;
        [SerializeField] private float _fieldOfView = 110f;

        [Header("Vitals")]
        [SerializeField] private float _health = 35f;
        [SerializeField] private WildlifeTemperament _temperament = WildlifeTemperament.Flee;

        [Header("Attack")]
        [SerializeField] private WildlifeAttackDefinition _attack = WildlifeAttackDefinition.Default;

        [Header("Loot")]
        [SerializeField] private List<WildlifeLootEntry> _lootTable = new List<WildlifeLootEntry>();

        /// <summary>
        /// Species identifier.
        /// </summary>
        public WildlifeSpeciesId SpeciesId => _speciesId;

        /// <summary>
        /// Default wandering speed.
        /// </summary>
        public float WalkSpeed => Mathf.Max(0f, _walkSpeed);

        /// <summary>
        /// Flee or chase speed.
        /// </summary>
        public float RunSpeed => Mathf.Max(WalkSpeed, _runSpeed);

        /// <summary>
        /// Downward gravity applied by wildlife agents.
        /// </summary>
        public float Gravity => _gravity <= 0f ? _gravity : -_gravity;

        /// <summary>
        /// Threat perception radius in meters.
        /// </summary>
        public float PerceptionRadius => Mathf.Max(0.1f, _perceptionRadius);

        /// <summary>
        /// Threat field of view in degrees.
        /// </summary>
        public float FieldOfView => Mathf.Clamp(_fieldOfView, 1f, 360f);

        /// <summary>
        /// Maximum health for this species.
        /// </summary>
        public float Health => Mathf.Max(1f, _health);

        /// <summary>
        /// Threat response temperament.
        /// </summary>
        public WildlifeTemperament Temperament => _temperament;

        /// <summary>
        /// Attack definition used by aggressive species.
        /// </summary>
        public WildlifeAttackDefinition Attack => _attack;

        /// <summary>
        /// Serialized loot entries.
        /// </summary>
        public IReadOnlyList<WildlifeLootEntry> LootTable => _lootTable;

        /// <summary>
        /// Creates deterministic settings for the pure wildlife brain model.
        /// </summary>
        public WildlifeBrainModel.Settings ToBrainSettings()
        {
            return new WildlifeBrainModel.Settings
            {
                Temperament = Temperament,
                PerceptionRadius = PerceptionRadius,
                AttackRange = Attack.Range,
                AttackCooldown = Attack.Cooldown,
                LowHealthFleeThreshold = 0.3f,
                MinIdleDuration = 1.2f,
                MaxIdleDuration = 2.8f,
                MinWanderDuration = 1.5f,
                MaxWanderDuration = 3.5f,
                AlertDuration = 1f
            };
        }
    }
}
