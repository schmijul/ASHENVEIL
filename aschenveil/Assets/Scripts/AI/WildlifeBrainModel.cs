using UnityEngine;

namespace Ashenveil.AI
{
    /// <summary>
    /// Deterministic wildlife behavior state machine.
    /// Referenced GDD section: Kernsysteme / Wildlife.
    /// </summary>
    public sealed class WildlifeBrainModel
    {
        private readonly Settings _settings;
        private readonly IRandomSource _randomSource;
        private WildlifeBrainState _state;
        private float _stateTimeRemaining;
        private float _attackCooldownRemaining;

        /// <summary>
        /// Creates a wildlife brain with explicit settings and random source.
        /// </summary>
        public WildlifeBrainModel(Settings settings, IRandomSource randomSource)
        {
            _settings = settings.Normalized();
            _randomSource = randomSource;
            _state = WildlifeBrainState.Idle;
            _stateTimeRemaining = RandomRange(_settings.MinIdleDuration, _settings.MaxIdleDuration);
        }

        /// <summary>
        /// Current wildlife state.
        /// </summary>
        public WildlifeBrainState State => _state;

        /// <summary>
        /// Advances the wildlife brain by an explicit delta time.
        /// </summary>
        public Output Tick(Input input, float deltaTime)
        {
            deltaTime = Mathf.Max(0f, deltaTime);
            if (_state == WildlifeBrainState.Dead)
            {
                return CreateOutput(false);
            }

            if (input.HealthFraction <= 0f)
            {
                ChangeState(WildlifeBrainState.Dead);
                return CreateOutput(false);
            }

            _attackCooldownRemaining = Mathf.Max(0f, _attackCooldownRemaining - deltaTime);
            bool seesThreat = input.LineOfSight && input.DistanceToThreat <= _settings.PerceptionRadius;

            if (_settings.Temperament == WildlifeTemperament.Flee)
            {
                TickFleeTemperament(input, seesThreat, deltaTime);
                return CreateOutput(false);
            }

            bool shouldAttack = TickAggressiveTemperament(input, seesThreat, deltaTime);
            return CreateOutput(shouldAttack);
        }

        private void TickFleeTemperament(Input input, bool seesThreat, float deltaTime)
        {
            if (seesThreat || input.HealthFraction <= _settings.LowHealthFleeThreshold)
            {
                ChangeState(WildlifeBrainState.Flee);
                return;
            }

            TickCalmState(deltaTime);
        }

        private bool TickAggressiveTemperament(Input input, bool seesThreat, float deltaTime)
        {
            if (seesThreat && input.DistanceToThreat <= _settings.AttackRange)
            {
                ChangeState(WildlifeBrainState.Attack);
                if (_attackCooldownRemaining <= 0f)
                {
                    _attackCooldownRemaining = _settings.AttackCooldown;
                    return true;
                }

                return false;
            }

            if (seesThreat)
            {
                ChangeState(WildlifeBrainState.Chase);
                return false;
            }

            if (_state == WildlifeBrainState.Chase || _state == WildlifeBrainState.Attack)
            {
                ChangeState(WildlifeBrainState.Alert);
            }

            if (_state == WildlifeBrainState.Alert)
            {
                _stateTimeRemaining -= deltaTime;
                if (_stateTimeRemaining <= 0f)
                {
                    ChangeState(WildlifeBrainState.Idle);
                }

                return false;
            }

            TickCalmState(deltaTime);
            return false;
        }

        private void TickCalmState(float deltaTime)
        {
            _stateTimeRemaining -= deltaTime;
            if (_stateTimeRemaining > 0f)
            {
                return;
            }

            if (_state == WildlifeBrainState.Wander)
            {
                ChangeState(WildlifeBrainState.Idle);
            }
            else
            {
                ChangeState(WildlifeBrainState.Wander);
            }
        }

        private void ChangeState(WildlifeBrainState nextState)
        {
            if (_state == WildlifeBrainState.Dead || _state == nextState)
            {
                return;
            }

            _state = nextState;
            if (nextState == WildlifeBrainState.Idle)
            {
                _stateTimeRemaining = RandomRange(_settings.MinIdleDuration, _settings.MaxIdleDuration);
            }
            else if (nextState == WildlifeBrainState.Wander)
            {
                _stateTimeRemaining = RandomRange(_settings.MinWanderDuration, _settings.MaxWanderDuration);
            }
            else if (nextState == WildlifeBrainState.Alert)
            {
                _stateTimeRemaining = _settings.AlertDuration;
            }
        }

        private float RandomRange(float min, float max)
        {
            if (_randomSource == null)
            {
                return min;
            }

            return Mathf.Lerp(min, max, _randomSource.Next01());
        }

        private Output CreateOutput(bool shouldAttack)
        {
            return new Output
            {
                State = _state,
                ShouldAttack = shouldAttack,
                ShouldFlee = _state == WildlifeBrainState.Flee,
                ShouldChase = _state == WildlifeBrainState.Chase,
                ShouldWander = _state == WildlifeBrainState.Wander
            };
        }

        /// <summary>
        /// Tunables used by the wildlife brain.
        /// </summary>
        public struct Settings
        {
            /// <summary>
            /// Flee or aggressive response.
            /// </summary>
            public WildlifeTemperament Temperament { get; set; }

            /// <summary>
            /// Maximum distance for threat perception.
            /// </summary>
            public float PerceptionRadius { get; set; }

            /// <summary>
            /// Distance where aggressive wildlife can attack.
            /// </summary>
            public float AttackRange { get; set; }

            /// <summary>
            /// Seconds between attacks.
            /// </summary>
            public float AttackCooldown { get; set; }

            /// <summary>
            /// Health fraction where flee wildlife panics.
            /// </summary>
            public float LowHealthFleeThreshold { get; set; }

            /// <summary>
            /// Minimum idle state duration.
            /// </summary>
            public float MinIdleDuration { get; set; }

            /// <summary>
            /// Maximum idle state duration.
            /// </summary>
            public float MaxIdleDuration { get; set; }

            /// <summary>
            /// Minimum wander state duration.
            /// </summary>
            public float MinWanderDuration { get; set; }

            /// <summary>
            /// Maximum wander state duration.
            /// </summary>
            public float MaxWanderDuration { get; set; }

            /// <summary>
            /// Alert state duration after losing threat.
            /// </summary>
            public float AlertDuration { get; set; }

            /// <summary>
            /// Sensible default wildlife brain settings.
            /// </summary>
            public static Settings Default => new Settings
            {
                Temperament = WildlifeTemperament.Flee,
                PerceptionRadius = 9f,
                AttackRange = 1.3f,
                AttackCooldown = 1.4f,
                LowHealthFleeThreshold = 0.3f,
                MinIdleDuration = 1.2f,
                MaxIdleDuration = 2.8f,
                MinWanderDuration = 1.5f,
                MaxWanderDuration = 3.5f,
                AlertDuration = 1f
            };

            /// <summary>
            /// Returns settings clamped to usable values.
            /// </summary>
            public Settings Normalized()
            {
                Settings settings = this;
                settings.PerceptionRadius = Mathf.Max(0.1f, PerceptionRadius);
                settings.AttackRange = Mathf.Max(0.1f, AttackRange);
                settings.AttackCooldown = Mathf.Max(0.01f, AttackCooldown);
                settings.LowHealthFleeThreshold = Mathf.Clamp01(LowHealthFleeThreshold);
                settings.MinIdleDuration = Mathf.Max(0.01f, MinIdleDuration);
                settings.MaxIdleDuration = Mathf.Max(settings.MinIdleDuration, MaxIdleDuration);
                settings.MinWanderDuration = Mathf.Max(0.01f, MinWanderDuration);
                settings.MaxWanderDuration = Mathf.Max(settings.MinWanderDuration, MaxWanderDuration);
                settings.AlertDuration = Mathf.Max(0.01f, AlertDuration);
                return settings;
            }
        }

        /// <summary>
        /// Input sample consumed by one wildlife brain tick.
        /// </summary>
        public struct Input
        {
            /// <summary>
            /// Distance to the current threat in meters.
            /// </summary>
            public float DistanceToThreat { get; set; }

            /// <summary>
            /// Whether the threat is visible and inside field of view.
            /// </summary>
            public bool LineOfSight { get; set; }

            /// <summary>
            /// Current health fraction from 0..1.
            /// </summary>
            public float HealthFraction { get; set; }
        }

        /// <summary>
        /// Wildlife behavior output produced by one tick.
        /// </summary>
        public struct Output
        {
            /// <summary>
            /// Current state.
            /// </summary>
            public WildlifeBrainState State { get; set; }

            /// <summary>
            /// Whether an attack should be applied this tick.
            /// </summary>
            public bool ShouldAttack { get; set; }

            /// <summary>
            /// Whether the agent should move away from the threat.
            /// </summary>
            public bool ShouldFlee { get; set; }

            /// <summary>
            /// Whether the agent should move toward the threat.
            /// </summary>
            public bool ShouldChase { get; set; }

            /// <summary>
            /// Whether the agent should wander.
            /// </summary>
            public bool ShouldWander { get; set; }
        }
    }
}
