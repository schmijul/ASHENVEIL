using System;
using System.Collections.Generic;
using UnityEngine;
using Ashenveil.Combat;

namespace Ashenveil.AI
{
    /// <summary>
    /// ScriptableObject configuration for wildlife species.
    /// </summary>
    [CreateAssetMenu(fileName = "WildlifeProfile", menuName = "Ashenveil/AI/Wildlife Profile")]
    public sealed class WildlifeProfile : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private WildlifeSpecies _species = WildlifeSpecies.Boar;
        [SerializeField] private string _displayName = "Boar";
        [SerializeField, TextArea] private string _description;

        [Header("Vitals")]
        [SerializeField, Min(0f)] private float _maxHealth = 30f;
        [SerializeField, Min(0f)] private float _armor = 2f;
        [SerializeField, Min(0f)] private float _damage = 15f;
        [SerializeField, Min(0f)] private float _walkSpeed = 4f;
        [SerializeField, Min(0f)] private float _runSpeed = 6f;

        [Header("Behavior")]
        [SerializeField] private bool _canPatrol = true;
        [SerializeField] private bool _canGraze = true;
        [SerializeField] private bool _fleeOnAlert = false;
        [SerializeField] private bool _aggressiveWhenClose = true;
        [SerializeField] private bool _aggroWhenInsideHomeRadius = false;
        [SerializeField] private bool _stalksFromBehind = false;
        [SerializeField, Min(0f)] private float _alertRadius = 8f;
        [SerializeField, Min(0f)] private float _aggroRadius = 8f;
        [SerializeField, Min(0f)] private float _attackRange = 2f;
        [SerializeField, Min(0f)] private float _homeRadius = 20f;
        [SerializeField, Min(1)] private int _packSizeMin = 1;
        [SerializeField, Min(1)] private int _packSizeMax = 3;
        [SerializeField, Min(0f)] private float _fleeDespawnDistance = 50f;

        [Header("Timing")]
        [SerializeField, Min(0f)] private float _idleMinDuration = 10f;
        [SerializeField, Min(0f)] private float _idleMaxDuration = 30f;
        [SerializeField, Min(0f)] private float _patrolMinDuration = 10f;
        [SerializeField, Min(0f)] private float _patrolMaxDuration = 30f;
        [SerializeField, Min(0f)] private float _grazeMinDuration = 5f;
        [SerializeField, Min(0f)] private float _grazeMaxDuration = 15f;
        [SerializeField, Min(0f)] private float _alertMinDuration = 1f;
        [SerializeField, Min(0f)] private float _alertMaxDuration = 2f;
        [SerializeField, Min(0f)] private float _fleeDuration = 8f;
        [SerializeField, Min(0f)] private float _deathDisableDelay = 3f;

        [Header("Combat")]
        [SerializeField] private WildlifeAttackDefinition[] _attacks = new WildlifeAttackDefinition[0];
        [SerializeField] private WildlifeDamageResistance[] _damageResistances = new WildlifeDamageResistance[0];

        [Header("Loot")]
        [SerializeField] private WildlifeLootDrop[] _lootDrops = new WildlifeLootDrop[0];

        public WildlifeSpecies Species => _species;
        public string DisplayName => _displayName;
        public string Description => _description;
        public float MaxHealth => _maxHealth;
        public float Armor => _armor;
        public float Damage => _damage;
        public float WalkSpeed => _walkSpeed;
        public float RunSpeed => _runSpeed;
        public bool CanPatrol => _canPatrol;
        public bool CanGraze => _canGraze;
        public bool FleeOnAlert => _fleeOnAlert;
        public bool AggressiveWhenClose => _aggressiveWhenClose;
        public bool AggroWhenInsideHomeRadius => _aggroWhenInsideHomeRadius;
        public bool StalksFromBehind => _stalksFromBehind;
        public float AlertRadius => _alertRadius;
        public float AggroRadius => _aggroRadius;
        public float AttackRange => _attackRange;
        public float HomeRadius => _homeRadius;
        public int PackSizeMin => _packSizeMin;
        public int PackSizeMax => _packSizeMax;
        public float FleeDespawnDistance => _fleeDespawnDistance;
        public float IdleMinDuration => _idleMinDuration;
        public float IdleMaxDuration => _idleMaxDuration;
        public float PatrolMinDuration => _patrolMinDuration;
        public float PatrolMaxDuration => _patrolMaxDuration;
        public float GrazeMinDuration => _grazeMinDuration;
        public float GrazeMaxDuration => _grazeMaxDuration;
        public float AlertMinDuration => _alertMinDuration;
        public float AlertMaxDuration => _alertMaxDuration;
        public float FleeDuration => _fleeDuration;
        public float DeathDisableDelay => _deathDisableDelay;
        public IReadOnlyList<WildlifeAttackDefinition> Attacks => _attacks;
        public IReadOnlyList<WildlifeDamageResistance> DamageResistances => _damageResistances;
        public IReadOnlyList<WildlifeLootDrop> LootDrops => _lootDrops;

        private void OnEnable()
        {
            EnsureDefaults();
        }

        private void OnValidate()
        {
            EnsureDefaults();
        }

        public static WildlifeProfile CreateRuntimeDefaults(WildlifeSpecies species)
        {
            WildlifeProfile profile = CreateInstance<WildlifeProfile>();
            profile.hideFlags = HideFlags.HideAndDontSave;
            profile.ApplySpeciesDefaults(species);
            profile.EnsureDefaults();
            return profile;
        }

        public WildlifeDamageModel CreateDamageModel()
        {
            EnsureDefaults();
            return new WildlifeDamageModel(_damageResistances);
        }

        public List<WildlifeLootResult> ResolveLoot(IWildlifeRandomSource randomSource = null)
        {
            EnsureDefaults();
            return WildlifeLootTable.ResolveDrops(_lootDrops, randomSource);
        }

        public WildlifeAttackDefinition GetAttack(WildlifeAttackKind attackKind)
        {
            EnsureDefaults();
            if (_attacks == null)
            {
                return null;
            }

            for (int index = 0; index < _attacks.Length; index++)
            {
                WildlifeAttackDefinition attack = _attacks[index];
                if (attack != null && attack.AttackKind == attackKind)
                {
                    return attack;
                }
            }

            return null;
        }

        private void ApplySpeciesDefaults(WildlifeSpecies species)
        {
            _species = species;
            _damageResistances = new WildlifeDamageResistance[0];
            _lootDrops = new WildlifeLootDrop[0];
            _attacks = new WildlifeAttackDefinition[0];

            switch (species)
            {
                case WildlifeSpecies.Deer:
                    _displayName = "Deer";
                    _description = "Passive forest animal that flees on sight.";
                    _maxHealth = 15f;
                    _armor = 0f;
                    _damage = 0f;
                    _walkSpeed = 5f;
                    _runSpeed = 8f;
                    _canPatrol = true;
                    _canGraze = true;
                    _fleeOnAlert = true;
                    _aggressiveWhenClose = false;
                    _aggroWhenInsideHomeRadius = false;
                    _stalksFromBehind = false;
                    _alertRadius = 15f;
                    _aggroRadius = 0f;
                    _attackRange = 0f;
                    _homeRadius = 30f;
                    _packSizeMin = 1;
                    _packSizeMax = 3;
                    _fleeDespawnDistance = 50f;
                    _idleMinDuration = 10f;
                    _idleMaxDuration = 30f;
                    _patrolMinDuration = 10f;
                    _patrolMaxDuration = 30f;
                    _grazeMinDuration = 5f;
                    _grazeMaxDuration = 15f;
                    _alertMinDuration = 1f;
                    _alertMaxDuration = 2f;
                    _fleeDuration = 8f;
                    _attacks = new WildlifeAttackDefinition[0];
                    _damageResistances = new[]
                    {
                        new WildlifeDamageResistance { DamageType = DamageType.Slash, Multiplier = 1f },
                        new WildlifeDamageResistance { DamageType = DamageType.Pierce, Multiplier = 1f },
                        new WildlifeDamageResistance { DamageType = DamageType.Blunt, Multiplier = 1f },
                        new WildlifeDamageResistance { DamageType = DamageType.Physical, Multiplier = 1f }
                    };
                    _lootDrops = new[]
                    {
                        new WildlifeLootDrop { ItemId = "Deer Pelt", DropChance = 1f, MinAmount = 1, MaxAmount = 1 },
                        new WildlifeLootDrop { ItemId = "Deer Meat", DropChance = 1f, MinAmount = 1, MaxAmount = 1 }
                    };
                    break;
                case WildlifeSpecies.Boar:
                    _displayName = "Boar";
                    _description = "Aggressive forest animal that charges when threatened.";
                    _maxHealth = 30f;
                    _armor = 2f;
                    _damage = 15f;
                    _walkSpeed = 4f;
                    _runSpeed = 6f;
                    _canPatrol = true;
                    _canGraze = true;
                    _fleeOnAlert = false;
                    _aggressiveWhenClose = true;
                    _aggroWhenInsideHomeRadius = false;
                    _stalksFromBehind = false;
                    _alertRadius = 8f;
                    _aggroRadius = 8f;
                    _attackRange = 2f;
                    _homeRadius = 20f;
                    _packSizeMin = 2;
                    _packSizeMax = 3;
                    _fleeDespawnDistance = 50f;
                    _idleMinDuration = 10f;
                    _idleMaxDuration = 30f;
                    _patrolMinDuration = 10f;
                    _patrolMaxDuration = 30f;
                    _grazeMinDuration = 5f;
                    _grazeMaxDuration = 15f;
                    _alertMinDuration = 1f;
                    _alertMaxDuration = 2f;
                    _fleeDuration = 8f;
                    _attacks = new[]
                    {
                        new WildlifeAttackDefinition
                        {
                            AnimationTrigger = "Charge",
                            AttackKind = WildlifeAttackKind.Charge,
                            DamageType = DamageType.Blunt,
                            Damage = 15f,
                            Range = 2f,
                            WindUp = 0.3f,
                            Recovery = 0.7f,
                            Cooldown = 1.5f
                        }
                    };
                    _damageResistances = new[]
                    {
                        new WildlifeDamageResistance { DamageType = DamageType.Slash, Multiplier = 1f },
                        new WildlifeDamageResistance { DamageType = DamageType.Pierce, Multiplier = 1f },
                        new WildlifeDamageResistance { DamageType = DamageType.Blunt, Multiplier = 1f },
                        new WildlifeDamageResistance { DamageType = DamageType.Physical, Multiplier = 1f }
                    };
                    _lootDrops = new[]
                    {
                        new WildlifeLootDrop { ItemId = "Boar Pelt", DropChance = 1f, MinAmount = 1, MaxAmount = 1 },
                        new WildlifeLootDrop { ItemId = "Boar Meat", DropChance = 1f, MinAmount = 1, MaxAmount = 1 },
                        new WildlifeLootDrop { ItemId = "Boar Tusk", DropChance = 0.3f, MinAmount = 1, MaxAmount = 1 }
                    };
                    break;
                case WildlifeSpecies.Wolf:
                    _displayName = "Wolf";
                    _description = "Predatory forest pack hunter.";
                    _maxHealth = 50f;
                    _armor = 0f;
                    _damage = 20f;
                    _walkSpeed = 6f;
                    _runSpeed = 8f;
                    _canPatrol = true;
                    _canGraze = false;
                    _fleeOnAlert = false;
                    _aggressiveWhenClose = true;
                    _aggroWhenInsideHomeRadius = false;
                    _stalksFromBehind = true;
                    _alertRadius = 20f;
                    _aggroRadius = 12f;
                    _attackRange = 2f;
                    _homeRadius = 40f;
                    _packSizeMin = 2;
                    _packSizeMax = 2;
                    _fleeDespawnDistance = 50f;
                    _idleMinDuration = 10f;
                    _idleMaxDuration = 30f;
                    _patrolMinDuration = 10f;
                    _patrolMaxDuration = 30f;
                    _grazeMinDuration = 5f;
                    _grazeMaxDuration = 15f;
                    _alertMinDuration = 1f;
                    _alertMaxDuration = 2f;
                    _fleeDuration = 8f;
                    _attacks = new[]
                    {
                        new WildlifeAttackDefinition
                        {
                            AnimationTrigger = "Bite",
                            AttackKind = WildlifeAttackKind.Bite,
                            DamageType = DamageType.Slash,
                            Damage = 20f,
                            Range = 2f,
                            WindUp = 0.25f,
                            Recovery = 0.55f,
                            Cooldown = 1.25f,
                            RequiresRearApproach = true
                        }
                    };
                    _damageResistances = new[]
                    {
                        new WildlifeDamageResistance { DamageType = DamageType.Slash, Multiplier = 1f },
                        new WildlifeDamageResistance { DamageType = DamageType.Pierce, Multiplier = 1f },
                        new WildlifeDamageResistance { DamageType = DamageType.Blunt, Multiplier = 1f },
                        new WildlifeDamageResistance { DamageType = DamageType.Physical, Multiplier = 1f }
                    };
                    _lootDrops = new[]
                    {
                        new WildlifeLootDrop { ItemId = "Wolf Pelt", DropChance = 1f, MinAmount = 1, MaxAmount = 1 },
                        new WildlifeLootDrop { ItemId = "Wolf Fang", DropChance = 0.5f, MinAmount = 1, MaxAmount = 1 }
                    };
                    break;
                case WildlifeSpecies.MutatedWolf:
                    _displayName = "Mutated Wolf";
                    _description = "Aether-corrupted boss wolf guarding the crystal clearing.";
                    _maxHealth = 120f;
                    _armor = 5f;
                    _damage = 30f;
                    _walkSpeed = 5f;
                    _runSpeed = 7f;
                    _canPatrol = true;
                    _canGraze = false;
                    _fleeOnAlert = false;
                    _aggressiveWhenClose = true;
                    _aggroWhenInsideHomeRadius = true;
                    _stalksFromBehind = true;
                    _alertRadius = 25f;
                    _aggroRadius = 25f;
                    _attackRange = 3f;
                    _homeRadius = 15f;
                    _packSizeMin = 1;
                    _packSizeMax = 1;
                    _fleeDespawnDistance = 50f;
                    _idleMinDuration = 10f;
                    _idleMaxDuration = 30f;
                    _patrolMinDuration = 10f;
                    _patrolMaxDuration = 30f;
                    _grazeMinDuration = 5f;
                    _grazeMaxDuration = 15f;
                    _alertMinDuration = 1f;
                    _alertMaxDuration = 2f;
                    _fleeDuration = 8f;
                    _attacks = new[]
                    {
                        new WildlifeAttackDefinition
                        {
                            AnimationTrigger = "Bite",
                            AttackKind = WildlifeAttackKind.Bite,
                            DamageType = DamageType.Physical,
                            Damage = 30f,
                            Range = 3f,
                            WindUp = 0.25f,
                            Recovery = 0.55f,
                            Cooldown = 1.5f,
                            RequiresRearApproach = true
                        },
                        new WildlifeAttackDefinition
                        {
                            AnimationTrigger = "Leap",
                            AttackKind = WildlifeAttackKind.Leap,
                            DamageType = DamageType.Aether,
                            Damage = 40f,
                            Range = 10f,
                            AreaRadius = 4f,
                            WindUp = 0.55f,
                            Recovery = 0.85f,
                            Cooldown = 8f
                        },
                        new WildlifeAttackDefinition
                        {
                            AnimationTrigger = "Howl",
                            AttackKind = WildlifeAttackKind.Howl,
                            DamageType = DamageType.True,
                            Damage = 0f,
                            Range = 0f,
                            WindUp = 0.35f,
                            Recovery = 0.85f,
                            Cooldown = 0f,
                            SpeedMultiplier = 1.3f,
                            HealthThreshold = 0.5f,
                            OnlyOncePerFight = true
                        }
                    };
                    _damageResistances = new[]
                    {
                        new WildlifeDamageResistance { DamageType = DamageType.Slash, Multiplier = 0.5f },
                        new WildlifeDamageResistance { DamageType = DamageType.Pierce, Multiplier = 0.5f },
                        new WildlifeDamageResistance { DamageType = DamageType.Blunt, Multiplier = 0.5f },
                        new WildlifeDamageResistance { DamageType = DamageType.Physical, Multiplier = 0.5f },
                        new WildlifeDamageResistance { DamageType = DamageType.Aether, Multiplier = 1f }
                    };
                    _lootDrops = new[]
                    {
                        new WildlifeLootDrop { ItemId = "Aether Shard", DropChance = 1f, MinAmount = 1, MaxAmount = 1 },
                        new WildlifeLootDrop { ItemId = "Wolf Pelt (Mutated)", DropChance = 1f, MinAmount = 1, MaxAmount = 1 }
                    };
                    break;
                default:
                    ApplySpeciesDefaults(WildlifeSpecies.Boar);
                    break;
            }
        }

        private void EnsureDefaults()
        {
            if (_packSizeMax < _packSizeMin)
            {
                _packSizeMax = _packSizeMin;
            }

            if (_attackRange < 0f)
            {
                _attackRange = 0f;
            }

            if (_aggroRadius < 0f)
            {
                _aggroRadius = 0f;
            }

            if (_homeRadius < 0f)
            {
                _homeRadius = 0f;
            }

            if (_fleeDespawnDistance < 0f)
            {
                _fleeDespawnDistance = 0f;
            }

            if (_attacks == null)
            {
                _attacks = new WildlifeAttackDefinition[0];
            }

            for (int index = 0; index < _attacks.Length; index++)
            {
                if (_attacks[index] == null)
                {
                    _attacks[index] = new WildlifeAttackDefinition();
                }
            }

            if (_damageResistances == null)
            {
                _damageResistances = new WildlifeDamageResistance[0];
            }

            for (int index = 0; index < _damageResistances.Length; index++)
            {
                if (_damageResistances[index] == null)
                {
                    _damageResistances[index] = new WildlifeDamageResistance();
                }
            }

            if (_lootDrops == null)
            {
                _lootDrops = new WildlifeLootDrop[0];
            }

            for (int index = 0; index < _lootDrops.Length; index++)
            {
                if (_lootDrops[index] == null)
                {
                    _lootDrops[index] = new WildlifeLootDrop();
                }
            }
        }
    }
}
