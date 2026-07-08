using System.Collections.Generic;
using Ashenveil.Core;
using UnityEngine;

namespace Ashenveil.AI
{
    /// <summary>
    /// CharacterController adapter for wildlife locomotion, attacks, damage, and loot drops.
    /// Referenced GDD section: Kernsysteme / Wildlife.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(WildlifeHealth))]
    public sealed class WildlifeAgent : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private WildlifeSpecies _species;
        [SerializeField] private int _randomSeed = 17;

        [Header("References")]
        [SerializeField] private Transform _threat;
        [SerializeField] private LootContainer _lootContainerPrefab;

        private CharacterController _controller;
        private WildlifeHealth _health;
        private WildlifeBrainModel _brain;
        private SystemRandomSource _randomSource;
        private Vector3 _wanderDirection = Vector3.forward;
        private float _verticalVelocity;
        private bool _lootDropped;

        /// <summary>
        /// Species data currently driving this agent.
        /// </summary>
        public WildlifeSpecies Species => _species;

        /// <summary>
        /// Current health adapter.
        /// </summary>
        public WildlifeHealth Health => _health;

        private void Awake()
        {
            if (!TryGetComponent(out _controller))
            {
                Debug.LogError("WildlifeAgent requires a CharacterController.", this);
                enabled = false;
                return;
            }

            if (!TryGetComponent(out _health))
            {
                Debug.LogError("WildlifeAgent requires a WildlifeHealth.", this);
                enabled = false;
                return;
            }

            _randomSource = new SystemRandomSource(_randomSeed);
            if (_species != null)
            {
                _brain = new WildlifeBrainModel(_species.ToBrainSettings(), _randomSource);
                _health.Initialize(_species);
            }

            _health.Died += OnDied;
            PickWanderDirection();
        }

        private void OnDestroy()
        {
            if (_health != null)
            {
                _health.Died -= OnDied;
            }
        }

        private void Update()
        {
            if (_species == null || _brain == null || _health == null || !_health.IsAlive)
            {
                return;
            }

            float distanceToThreat = _threat != null ? Vector3.Distance(transform.position, _threat.position) : float.MaxValue;
            bool hasLineOfSight = HasThreatInView(distanceToThreat);
            WildlifeBrainModel.Output output = _brain.Tick(new WildlifeBrainModel.Input
            {
                DistanceToThreat = distanceToThreat,
                LineOfSight = hasLineOfSight,
                HealthFraction = _health.HealthFraction
            }, Time.deltaTime);

            Move(output);
            if (output.ShouldAttack)
            {
                AttackThreat();
            }
        }

        /// <summary>
        /// Initializes this agent after spawning.
        /// </summary>
        public void Initialize(WildlifeSpecies species, Transform threat)
        {
            _species = species;
            _threat = threat;
            if (_health != null)
            {
                _health.Initialize(species);
            }

            if (species != null)
            {
                _brain = new WildlifeBrainModel(species.ToBrainSettings(), _randomSource ?? new SystemRandomSource(_randomSeed));
            }
        }

        private bool HasThreatInView(float distanceToThreat)
        {
            if (_threat == null || distanceToThreat > _species.PerceptionRadius)
            {
                return false;
            }

            Vector3 toThreat = _threat.position - transform.position;
            toThreat.y = 0f;
            if (toThreat.sqrMagnitude <= 0.0001f)
            {
                return true;
            }

            float angle = Vector3.Angle(transform.forward, toThreat.normalized);
            return angle <= _species.FieldOfView * 0.5f;
        }

        private void Move(WildlifeBrainModel.Output output)
        {
            Vector3 direction = Vector3.zero;
            float speed = 0f;

            if (_threat != null && output.ShouldFlee)
            {
                direction = transform.position - _threat.position;
                speed = _species.RunSpeed;
            }
            else if (_threat != null && output.ShouldChase)
            {
                direction = _threat.position - transform.position;
                speed = _species.RunSpeed;
            }
            else if (output.ShouldWander)
            {
                direction = _wanderDirection;
                speed = _species.WalkSpeed;
            }

            direction.y = 0f;
            if (direction.sqrMagnitude > 0.0001f)
            {
                direction.Normalize();
                transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
            }

            if (_controller.isGrounded && _verticalVelocity < 0f)
            {
                _verticalVelocity = -1f;
            }

            _verticalVelocity += _species.Gravity * Time.deltaTime;
            Vector3 velocity = direction * speed + Vector3.up * _verticalVelocity;
            _controller.Move(velocity * Time.deltaTime);

            if (!output.ShouldWander)
            {
                PickWanderDirection();
            }
        }

        private void AttackThreat()
        {
            if (_threat == null || !_threat.TryGetComponent(out IDamageable damageable) || !damageable.IsAlive)
            {
                return;
            }

            WildlifeAttackDefinition attack = _species.Attack;
            DamageInfo damageInfo = new DamageInfo(attack.Damage, attack.DamageType, transform.position, attack.Knockback);
            damageable.TakeDamage(damageInfo);
        }

        private void PickWanderDirection()
        {
            float angle = (_randomSource != null ? _randomSource.Next01() : 0f) * 360f;
            _wanderDirection = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
        }

        private void OnDied()
        {
            if (_lootDropped)
            {
                return;
            }

            _lootDropped = true;
            List<LootStack> loot = WildlifeLootRoller.Roll(_species.LootTable, _randomSource);
            LootContainer container = SpawnLootContainer();
            container.Initialize(loot);
        }

        private LootContainer SpawnLootContainer()
        {
            if (_lootContainerPrefab != null)
            {
                return Instantiate(_lootContainerPrefab, transform.position, Quaternion.identity);
            }

            GameObject lootObject = new GameObject("Wildtierbeute");
            lootObject.transform.position = transform.position;
            SphereCollider trigger = lootObject.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.radius = 1f;
            return lootObject.AddComponent<LootContainer>();
        }
    }
}
