using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Ashenveil.Combat;

namespace Ashenveil.AI
{
    /// <summary>
    /// NavMesh-driven wildlife controller that bridges runtime state, animations, and combat damage.
    /// </summary>
    [RequireComponent(typeof(WildlifeHealth))]
    public class WildlifeController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private WildlifeProfile _profile;
        [SerializeField] private WildlifeHealth _health;
        [SerializeField] private NavMeshAgent _navMeshAgent;
        [SerializeField] private Animator _animator;
        [SerializeField] private Transform _playerTarget;
        [SerializeField] private Transform _homeAnchor;
        [SerializeField] private Transform[] _patrolPoints;

        [Header("Animation Triggers")]
        [SerializeField] private string _idleTrigger = "Idle";
        [SerializeField] private string _patrolTrigger = "Patrol";
        [SerializeField] private string _grazeTrigger = "Graze";
        [SerializeField] private string _alertTrigger = "Alert";
        [SerializeField] private string _fleeTrigger = "Flee";
        [SerializeField] private string _chaseTrigger = "Chase";
        [SerializeField] private string _attackTrigger = "Attack";
        [SerializeField] private string _deathTrigger = "Death";

        private WildlifeRuntimeState _runtimeState;
        private WildlifeState _lastState = WildlifeState.Idle;
        private WildlifeAttackDefinition _lastAttack;
        private Vector3 _spawnPosition;
        private bool _lootRequested;
        private float _disableAt = -1f;

        public event Action<WildlifeState> StateChanged;
        public event Action<WildlifeAttackDefinition> AttackStarted;
        public event Action<WildlifeAttackDefinition> AttackFinished;
        public event Action<IReadOnlyList<WildlifeLootResult>, Vector3> LootDropped;

        public WildlifeState CurrentState => _runtimeState != null ? _runtimeState.CurrentState : WildlifeState.Idle;

        public bool IsDead => _health != null && _health.IsDead;

        public float HealthRatio => _health != null ? _health.HealthRatio : 1f;

        private void Awake()
        {
            ResolveReferences();
            _spawnPosition = transform.position;
        }

        private void OnEnable()
        {
            ResolveReferences();
            SubscribeToHealth();
            if (_runtimeState == null)
            {
                InitializeRuntime();
            }
        }

        private void Start()
        {
            InitializeRuntime();
        }

        private void Update()
        {
            if (_runtimeState == null || _health == null)
            {
                return;
            }

            if (_health.IsDead)
            {
                _runtimeState.ForceDead();
            }

            WildlifePerception perception = BuildPerception();
            _runtimeState.Tick(perception, Time.deltaTime);

            HandleStateTransitions(perception);
            HandleNavigation(perception);
            HandleDeathCleanup();
        }

        private void OnDisable()
        {
            UnsubscribeFromHealth();

            if (_navMeshAgent != null)
            {
                _navMeshAgent.isStopped = true;
            }
        }

        private void OnValidate()
        {
            ResolveReferences();
        }

        public void SetPlayerTarget(Transform playerTarget)
        {
            _playerTarget = playerTarget;
        }

        private void ResolveReferences()
        {
            if (_health == null)
            {
                TryGetComponent(out _health);
            }

            if (_navMeshAgent == null)
            {
                TryGetComponent(out _navMeshAgent);
            }

            if (_animator == null)
            {
                TryGetComponent(out _animator);
            }

            if (_profile == null && _health != null)
            {
                _profile = _health.Profile;
            }
        }

        private void InitializeRuntime()
        {
            if (_profile == null && _health != null)
            {
                _profile = _health.Profile;
            }

            if (_profile == null)
            {
                _profile = WildlifeProfile.CreateRuntimeDefaults(WildlifeSpecies.Boar);
            }

            _runtimeState = new WildlifeRuntimeState(_profile);
            _lastState = _runtimeState.CurrentState;
            _lastAttack = null;
            _lootRequested = false;
            _disableAt = -1f;
            _spawnPosition = transform.position;
        }

        private void SubscribeToHealth()
        {
            if (_health == null)
            {
                return;
            }

            _health.Died -= HandleHealthDied;
            _health.Died += HandleHealthDied;
        }

        private void UnsubscribeFromHealth()
        {
            if (_health == null)
            {
                return;
            }

            _health.Died -= HandleHealthDied;
        }

        private WildlifePerception BuildPerception()
        {
            WildlifePerception perception = WildlifePerception.CreateDefault();
            perception.HealthRatio = HealthRatio;
            perception.IsInsideHomeRadius = IsInsideHomeRadius();

            if (_playerTarget == null)
            {
                perception.PlayerDetected = false;
                perception.PlayerDistance = float.PositiveInfinity;
                perception.IsPlayerBehindTarget = false;
                return perception;
            }

            Vector3 targetPosition = _playerTarget.position;
            float playerDistance = Vector3.Distance(transform.position, targetPosition);
            perception.PlayerDistance = playerDistance;
            perception.HasLineOfSight = true;
            perception.PlayerDetected = playerDistance <= Mathf.Max(0f, _profile.AlertRadius);

            Vector3 targetToWildlife = (transform.position - targetPosition).normalized;
            perception.IsPlayerBehindTarget = Vector3.Dot(_playerTarget.forward, targetToWildlife) < -0.25f;
            return perception;
        }

        private bool IsInsideHomeRadius()
        {
            Vector3 anchorPosition = _homeAnchor != null ? _homeAnchor.position : _spawnPosition;
            float homeRadius = _profile != null ? _profile.HomeRadius : 0f;
            return Vector3.Distance(anchorPosition, transform.position) <= homeRadius;
        }

        private void HandleStateTransitions(WildlifePerception perception)
        {
            if (_runtimeState == null)
            {
                return;
            }

            WildlifeState currentState = _runtimeState.CurrentState;
            if (currentState == _lastState)
            {
                return;
            }

            WildlifeAttackDefinition currentAttack = _runtimeState.CurrentAttack;
            if (_lastState == WildlifeState.Attack)
            {
                AttackFinished?.Invoke(_lastAttack);
                _lastAttack = null;
            }

            _lastState = currentState;
            StateChanged?.Invoke(currentState);

            if (_animator != null)
            {
                _animator.speed = _runtimeState.HasTriggeredHowl ? _runtimeState.AttackSpeedMultiplier : 1f;
                TriggerAnimatorForState(currentState, currentAttack);
            }

            if (currentState == WildlifeState.Attack && currentAttack != null)
            {
                _lastAttack = currentAttack;
                AttackStarted?.Invoke(currentAttack);
            }

            if (currentState == WildlifeState.Dead && !_lootRequested)
            {
                RequestLootDrop();
            }

            ApplyImmediateNavigationForState(currentState);
        }

        private void HandleNavigation(WildlifePerception perception)
        {
            if (_navMeshAgent == null || !_navMeshAgent.enabled)
            {
                return;
            }

            switch (_runtimeState.CurrentState)
            {
                case WildlifeState.Patrol:
                    _navMeshAgent.isStopped = false;
                    _navMeshAgent.speed = _profile.WalkSpeed;
                    break;
                case WildlifeState.Chase:
                    _navMeshAgent.isStopped = false;
                    _navMeshAgent.speed = _profile.RunSpeed * _runtimeState.AttackSpeedMultiplier;
                    SetChaseDestination();
                    break;
                case WildlifeState.Flee:
                    _navMeshAgent.isStopped = false;
                    _navMeshAgent.speed = _profile.RunSpeed * _runtimeState.AttackSpeedMultiplier;
                    SetFleeDestination();
                    break;
                case WildlifeState.Idle:
                case WildlifeState.Graze:
                case WildlifeState.Alert:
                case WildlifeState.Attack:
                case WildlifeState.Dead:
                    _navMeshAgent.isStopped = true;
                    break;
            }
        }

        private void HandleDeathCleanup()
        {
            if (!_runtimeState.IsDead)
            {
                return;
            }

            if (_disableAt < 0f)
            {
                _disableAt = Time.time + Mathf.Max(0f, _profile.DeathDisableDelay);
            }

            if (Time.time >= _disableAt && gameObject.activeSelf)
            {
                gameObject.SetActive(false);
            }
        }

        private void HandleHealthDied()
        {
            if (_runtimeState != null)
            {
                _runtimeState.ForceDead();
            }
        }

        private void TriggerAnimatorForState(WildlifeState state, WildlifeAttackDefinition attack)
        {
            if (_animator == null)
            {
                return;
            }

            switch (state)
            {
                case WildlifeState.Idle:
                    _animator.SetTrigger(_idleTrigger);
                    break;
                case WildlifeState.Patrol:
                    _animator.SetTrigger(_patrolTrigger);
                    break;
                case WildlifeState.Graze:
                    _animator.SetTrigger(_grazeTrigger);
                    break;
                case WildlifeState.Alert:
                    _animator.SetTrigger(_alertTrigger);
                    break;
                case WildlifeState.Flee:
                    _animator.SetTrigger(_fleeTrigger);
                    break;
                case WildlifeState.Chase:
                    _animator.SetTrigger(_chaseTrigger);
                    break;
                case WildlifeState.Attack:
                    _animator.SetTrigger(attack != null && !string.IsNullOrWhiteSpace(attack.AnimationTrigger) ? attack.AnimationTrigger : _attackTrigger);
                    break;
                case WildlifeState.Dead:
                    _animator.SetTrigger(_deathTrigger);
                    break;
            }
        }

        private void SetPatrolDestination()
        {
            Vector3 anchorPosition = _homeAnchor != null ? _homeAnchor.position : _spawnPosition;
            Vector3 randomOffset = UnityEngine.Random.insideUnitSphere * Mathf.Max(1f, _profile.HomeRadius);
            randomOffset.y = 0f;
            Vector3 candidate = anchorPosition + randomOffset;

            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, Mathf.Max(1f, _profile.HomeRadius), NavMesh.AllAreas))
            {
                _navMeshAgent.SetDestination(hit.position);
            }
        }

        private void SetChaseDestination()
        {
            if (_playerTarget == null)
            {
                return;
            }

            _navMeshAgent.SetDestination(_playerTarget.position);
        }

        private void SetFleeDestination()
        {
            if (_playerTarget == null)
            {
                return;
            }

            Vector3 awayDirection = (transform.position - _playerTarget.position).normalized;
            Vector3 candidate = transform.position + (awayDirection * Mathf.Max(1f, _profile.FleeDespawnDistance));
            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, Mathf.Max(1f, _profile.FleeDespawnDistance), NavMesh.AllAreas))
            {
                _navMeshAgent.SetDestination(hit.position);
            }
        }

        private void RequestLootDrop()
        {
            if (_lootRequested || _profile == null)
            {
                return;
            }

            _lootRequested = true;
            List<WildlifeLootResult> lootResults = _profile.ResolveLoot();
            LootDropped?.Invoke(lootResults, transform.position);
        }

        private void ApplyImmediateNavigationForState(WildlifeState state)
        {
            if (_navMeshAgent == null || !_navMeshAgent.enabled)
            {
                return;
            }

            switch (state)
            {
                case WildlifeState.Patrol:
                    SetPatrolDestination();
                    break;
                case WildlifeState.Chase:
                    SetChaseDestination();
                    break;
                case WildlifeState.Flee:
                    SetFleeDestination();
                    break;
            }
        }
    }
}
