using UnityEngine;

namespace Ashenveil.AI
{
    /// <summary>
    /// Pure wildlife behavior state machine for roaming, fleeing, chasing, and attacking.
    /// </summary>
    public sealed class WildlifeRuntimeState
    {
        private readonly WildlifeProfile _profile;
        private readonly IWildlifeRandomSource _randomSource;

        private float _time;
        private float _stateEndsAt;
        private float _attackCooldownEndsAt;
        private bool _hasTriggeredHowl;
        private bool _hasRequestedDespawn;
        private WildlifeAttackDefinition _currentAttack;
        private WildlifeState _currentState;
        private float _attackSpeedMultiplier = 1f;

        public WildlifeRuntimeState(WildlifeProfile profile, IWildlifeRandomSource randomSource = null)
        {
            _profile = profile;
            _randomSource = randomSource ?? new UnityWildlifeRandomSource();
            Reset();
        }

        public WildlifeState CurrentState => _currentState;

        public WildlifeAttackDefinition CurrentAttack => _currentAttack;

        public float AttackSpeedMultiplier => _attackSpeedMultiplier;

        public float StateRemainingTime => Mathf.Max(0f, _stateEndsAt - _time);

        public bool HasTriggeredHowl => _hasTriggeredHowl;

        public bool HasRequestedDespawn => _hasRequestedDespawn;

        public bool IsDead => _currentState == WildlifeState.Dead;

        public bool IsAttacking => _currentState == WildlifeState.Attack;

        public bool IsFleeing => _currentState == WildlifeState.Flee;

        public bool CanTakeDamage => _currentState != WildlifeState.Dead;

        public void Reset()
        {
            _time = 0f;
            _stateEndsAt = 0f;
            _attackCooldownEndsAt = 0f;
            _hasTriggeredHowl = false;
            _hasRequestedDespawn = false;
            _currentAttack = null;
            _currentState = WildlifeState.Idle;
            _attackSpeedMultiplier = 1f;
            EnterIdle();
        }

        public void Tick(WildlifePerception perception, float deltaTime)
        {
            if (deltaTime <= 0f)
            {
                return;
            }

            _time += deltaTime;

            if (perception.HealthRatio <= 0f)
            {
                EnterDead();
                return;
            }

            if (_currentState == WildlifeState.Dead)
            {
                return;
            }

            if (TryTriggerHowl(perception))
            {
                return;
            }

            if (_currentState == WildlifeState.Flee)
            {
                UpdateFlee(perception);
                return;
            }

            if (_currentState == WildlifeState.Attack)
            {
                UpdateAttack(perception);
                return;
            }

            if (_currentState == WildlifeState.Chase)
            {
                UpdateChase(perception);
                return;
            }

            if (_currentState == WildlifeState.Alert)
            {
                UpdateAlert(perception);
                return;
            }

            if (perception.PlayerDetected && perception.PlayerDistance <= Mathf.Max(0f, _profile.AlertRadius))
            {
                EnterAlert();
                return;
            }

            UpdateRoam(perception);
        }

        public void ForceDead()
        {
            EnterDead();
        }

        private void UpdateRoam(WildlifePerception perception)
        {
            if (_time < _stateEndsAt)
            {
                return;
            }

            if (_profile != null && _profile.CanPatrol && _profile.CanGraze)
            {
                if (_randomSource.Value() < 0.5f)
                {
                    EnterPatrol();
                }
                else
                {
                    EnterGraze();
                }

                return;
            }

            if (_profile != null && _profile.CanPatrol)
            {
                EnterPatrol();
                return;
            }

            if (_profile != null && _profile.CanGraze)
            {
                EnterGraze();
                return;
            }

            EnterIdle();
        }

        private void UpdateAlert(WildlifePerception perception)
        {
            if (_time < _stateEndsAt)
            {
                return;
            }

            if (ShouldFlee(perception))
            {
                EnterFlee();
                return;
            }

            if (ShouldChase(perception))
            {
                EnterChase();
                return;
            }

            EnterIdle();
        }

        private void UpdateFlee(WildlifePerception perception)
        {
            if (perception.PlayerDetected && perception.PlayerDistance >= Mathf.Max(0f, _profile.FleeDespawnDistance))
            {
                _hasRequestedDespawn = true;
                return;
            }

            if (_time >= _stateEndsAt)
            {
                _hasRequestedDespawn = true;
            }
        }

        private void UpdateAttack(WildlifePerception perception)
        {
            if (_time < _stateEndsAt)
            {
                return;
            }

            WildlifeAttackDefinition finishedAttack = _currentAttack;
            _currentAttack = null;
            _attackCooldownEndsAt = _time + Mathf.Max(0f, finishedAttack != null ? finishedAttack.Cooldown : 0f);

            if (ShouldChase(perception))
            {
                EnterChase();
                return;
            }

            EnterIdle();
        }

        private void UpdateChase(WildlifePerception perception)
        {
            if (!perception.PlayerDetected)
            {
                EnterIdle();
                return;
            }

            if (_time < _attackCooldownEndsAt)
            {
                return;
            }

            WildlifeAttackDefinition attack = SelectAttack(perception);
            if (attack != null)
            {
                EnterAttack(attack);
            }
        }

        private bool TryTriggerHowl(WildlifePerception perception)
        {
            if (_profile == null || _hasTriggeredHowl || perception.HealthRatio > 0.5f)
            {
                return false;
            }

            WildlifeAttackDefinition howlAttack = FindAttack(WildlifeAttackKind.Howl);
            if (howlAttack == null)
            {
                return false;
            }

            _hasTriggeredHowl = true;
            _attackSpeedMultiplier = Mathf.Max(_attackSpeedMultiplier, Mathf.Max(1f, howlAttack.SpeedMultiplier));
            EnterAttack(howlAttack);
            return true;
        }

        private void EnterIdle()
        {
            _currentState = WildlifeState.Idle;
            _currentAttack = null;
            _stateEndsAt = _time + RandomDuration(_profile != null ? _profile.IdleMinDuration : 10f, _profile != null ? _profile.IdleMaxDuration : 30f);
        }

        private void EnterPatrol()
        {
            _currentState = WildlifeState.Patrol;
            _currentAttack = null;
            _stateEndsAt = _time + RandomDuration(_profile != null ? _profile.PatrolMinDuration : 10f, _profile != null ? _profile.PatrolMaxDuration : 30f);
        }

        private void EnterGraze()
        {
            _currentState = WildlifeState.Graze;
            _currentAttack = null;
            _stateEndsAt = _time + RandomDuration(_profile != null ? _profile.GrazeMinDuration : 5f, _profile != null ? _profile.GrazeMaxDuration : 15f);
        }

        private void EnterAlert()
        {
            _currentState = WildlifeState.Alert;
            _currentAttack = null;
            _stateEndsAt = _time + RandomDuration(_profile != null ? _profile.AlertMinDuration : 1f, _profile != null ? _profile.AlertMaxDuration : 2f);
        }

        private void EnterFlee()
        {
            _currentState = WildlifeState.Flee;
            _currentAttack = null;
            _stateEndsAt = _time + Mathf.Max(0f, _profile != null ? _profile.FleeDuration : 8f);
        }

        private void EnterChase()
        {
            _currentState = WildlifeState.Chase;
            _currentAttack = null;
            _stateEndsAt = float.PositiveInfinity;
        }

        private void EnterAttack(WildlifeAttackDefinition attack)
        {
            _currentState = WildlifeState.Attack;
            _currentAttack = attack != null ? attack.Clone() : null;
            float attackDuration = attack != null ? Mathf.Max(0f, attack.WindUp + attack.Recovery) : 0.5f;
            _stateEndsAt = _time + attackDuration;
        }

        private void EnterDead()
        {
            _currentState = WildlifeState.Dead;
            _currentAttack = null;
            _stateEndsAt = _time + Mathf.Max(0f, _profile != null ? _profile.DeathDisableDelay : 3f);
        }

        private bool ShouldFlee(WildlifePerception perception)
        {
            if (_profile == null)
            {
                return false;
            }

            return _profile.FleeOnAlert || _profile.Species == WildlifeSpecies.Deer;
        }

        private bool ShouldChase(WildlifePerception perception)
        {
            if (_profile == null || !perception.PlayerDetected)
            {
                return false;
            }

            if (_profile.AggroWhenInsideHomeRadius && perception.IsInsideHomeRadius)
            {
                return true;
            }

            if (_profile.AggressiveWhenClose && perception.PlayerDistance <= Mathf.Max(0f, _profile.AggroRadius))
            {
                return true;
            }

            return _profile.Species == WildlifeSpecies.Wolf || _profile.Species == WildlifeSpecies.MutatedWolf;
        }

        private WildlifeAttackDefinition FindAttack(WildlifeAttackKind attackKind)
        {
            if (_profile == null || _profile.Attacks == null)
            {
                return null;
            }

            for (int index = 0; index < _profile.Attacks.Count; index++)
            {
                WildlifeAttackDefinition attack = _profile.Attacks[index];
                if (attack != null && attack.AttackKind == attackKind)
                {
                    return attack;
                }
            }

            return null;
        }

        private WildlifeAttackDefinition SelectAttack(WildlifePerception perception)
        {
            if (_profile == null || _profile.Attacks == null || _profile.Attacks.Count == 0)
            {
                return null;
            }

            WildlifeAttackDefinition selectedAttack = null;
            float selectedRange = float.MaxValue;
            for (int index = 0; index < _profile.Attacks.Count; index++)
            {
                WildlifeAttackDefinition attack = _profile.Attacks[index];
                if (!IsAttackEligible(attack, perception))
                {
                    continue;
                }

                float attackRange = Mathf.Max(0f, attack.Range);
                if (selectedAttack == null || attackRange < selectedRange)
                {
                    selectedAttack = attack;
                    selectedRange = attackRange;
                }
            }

            return selectedAttack;
        }

        private bool IsAttackEligible(WildlifeAttackDefinition attack, WildlifePerception perception)
        {
            if (attack == null)
            {
                return false;
            }

            if (attack.AttackKind == WildlifeAttackKind.Howl)
            {
                return false;
            }

            if (_time < _attackCooldownEndsAt)
            {
                return false;
            }

            if (attack.RequiresRearApproach && !perception.IsPlayerBehindTarget)
            {
                return false;
            }

            if (perception.PlayerDistance > Mathf.Max(0f, attack.Range))
            {
                return false;
            }

            return true;
        }

        private float RandomDuration(float minDuration, float maxDuration)
        {
            float clampedMin = Mathf.Max(0f, minDuration);
            float clampedMax = Mathf.Max(clampedMin, maxDuration);
            if (Mathf.Approximately(clampedMin, clampedMax))
            {
                return clampedMin;
            }

            return _randomSource.Range(clampedMin, clampedMax);
        }
    }
}
