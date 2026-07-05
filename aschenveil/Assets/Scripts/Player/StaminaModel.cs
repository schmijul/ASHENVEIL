using UnityEngine;

namespace Ashenveil.Player
{
    /// <summary>
    /// Deterministic stamina model for sprinting, dodging, attacks, and delayed regeneration.
    /// Referenced GDD section: Kernsysteme / Movement/Camera and Combat.
    /// </summary>
    public sealed class StaminaModel
    {
        private readonly Settings _settings;
        private float _regenDelayRemaining;

        /// <summary>
        /// Creates a stamina model with full stamina.
        /// </summary>
        public StaminaModel(Settings settings)
        {
            _settings = settings.Normalized();
            CurrentStamina = _settings.MaxStamina;
        }

        /// <summary>
        /// Current stamina amount.
        /// </summary>
        public float CurrentStamina { get; private set; }

        /// <summary>
        /// Maximum stamina amount.
        /// </summary>
        public float MaxStamina => _settings.MaxStamina;

        /// <summary>
        /// Current stamina normalized to 0..1.
        /// </summary>
        public float Normalized => MaxStamina <= 0f ? 0f : CurrentStamina / MaxStamina;

        /// <summary>
        /// Indicates whether sprinting may start or continue.
        /// </summary>
        public bool CanSprint => CurrentStamina > _settings.MinimumSprintStamina;

        /// <summary>
        /// Advances sprint drain or regeneration.
        /// </summary>
        public State Update(bool sprintRequested, float deltaTime)
        {
            deltaTime = Mathf.Max(0f, deltaTime);
            bool isSprinting = sprintRequested && CanSprint;

            if (isSprinting)
            {
                DrainSprint(_settings.SprintDrainPerSecond * deltaTime);
                isSprinting = CanSprint;
            }
            else
            {
                Regenerate(deltaTime);
            }

            return new State
            {
                CurrentStamina = CurrentStamina,
                Normalized = Normalized,
                IsSprinting = isSprinting,
                CanSprint = CanSprint
            };
        }

        /// <summary>
        /// Attempts to spend stamina for a dodge.
        /// </summary>
        public bool TrySpendDodge()
        {
            return Spend(_settings.DodgeCost);
        }

        /// <summary>
        /// Attempts to spend stamina for a light attack.
        /// </summary>
        public bool TrySpendLightAttack()
        {
            return Spend(_settings.LightAttackCost);
        }

        /// <summary>
        /// Attempts to spend stamina for a heavy attack.
        /// </summary>
        public bool TrySpendHeavyAttack()
        {
            return Spend(_settings.HeavyAttackCost);
        }

        /// <summary>
        /// Attempts to spend an explicit stamina amount.
        /// </summary>
        public bool Spend(float amount)
        {
            amount = Mathf.Max(0f, amount);
            if (amount <= 0f)
            {
                return true;
            }

            if (CurrentStamina < amount)
            {
                return false;
            }

            CurrentStamina = Mathf.Max(0f, CurrentStamina - amount);
            _regenDelayRemaining = _settings.RegenDelay;
            return true;
        }

        private void Regenerate(float deltaTime)
        {
            if (_regenDelayRemaining > 0f)
            {
                _regenDelayRemaining = Mathf.Max(0f, _regenDelayRemaining - deltaTime);
                return;
            }

            CurrentStamina = Mathf.Min(_settings.MaxStamina, CurrentStamina + _settings.RegenPerSecond * deltaTime);
        }

        private void DrainSprint(float amount)
        {
            amount = Mathf.Max(0f, amount);
            if (amount <= 0f)
            {
                return;
            }

            CurrentStamina = Mathf.Max(0f, CurrentStamina - amount);
            _regenDelayRemaining = _settings.RegenDelay;
        }

        /// <summary>
        /// Tunables used by the stamina model.
        /// </summary>
        public struct Settings
        {
            /// <summary>
            /// Maximum stamina amount.
            /// </summary>
            public float MaxStamina { get; set; }

            /// <summary>
            /// Stamina drained while sprinting.
            /// </summary>
            public float SprintDrainPerSecond { get; set; }

            /// <summary>
            /// Stamina cost for one dodge.
            /// </summary>
            public float DodgeCost { get; set; }

            /// <summary>
            /// Stamina cost for one light attack.
            /// </summary>
            public float LightAttackCost { get; set; }

            /// <summary>
            /// Stamina cost for one heavy attack.
            /// </summary>
            public float HeavyAttackCost { get; set; }

            /// <summary>
            /// Stamina restored per second after the delay.
            /// </summary>
            public float RegenPerSecond { get; set; }

            /// <summary>
            /// Delay before regeneration begins after spending stamina.
            /// </summary>
            public float RegenDelay { get; set; }

            /// <summary>
            /// Minimum stamina required to sprint.
            /// </summary>
            public float MinimumSprintStamina { get; set; }

            /// <summary>
            /// Sensible default stamina settings for the demo player.
            /// </summary>
            public static Settings Default => new Settings
            {
                MaxStamina = 100f,
                SprintDrainPerSecond = 18f,
                DodgeCost = 25f,
                LightAttackCost = 12f,
                HeavyAttackCost = 28f,
                RegenPerSecond = 22f,
                RegenDelay = 0.8f,
                MinimumSprintStamina = 0f
            };

            /// <summary>
            /// Returns settings clamped to usable values.
            /// </summary>
            public Settings Normalized()
            {
                Settings settings = this;
                settings.MaxStamina = Mathf.Max(1f, MaxStamina);
                settings.SprintDrainPerSecond = Mathf.Max(0f, SprintDrainPerSecond);
                settings.DodgeCost = Mathf.Max(0f, DodgeCost);
                settings.LightAttackCost = Mathf.Max(0f, LightAttackCost);
                settings.HeavyAttackCost = Mathf.Max(0f, HeavyAttackCost);
                settings.RegenPerSecond = Mathf.Max(0f, RegenPerSecond);
                settings.RegenDelay = Mathf.Max(0f, RegenDelay);
                settings.MinimumSprintStamina = Mathf.Clamp(MinimumSprintStamina, 0f, settings.MaxStamina);
                return settings;
            }
        }

        /// <summary>
        /// Stamina output produced by one simulation tick.
        /// </summary>
        public struct State
        {
            /// <summary>
            /// Current stamina amount.
            /// </summary>
            public float CurrentStamina { get; set; }

            /// <summary>
            /// Current stamina normalized to 0..1.
            /// </summary>
            public float Normalized { get; set; }

            /// <summary>
            /// Whether sprinting is active for this tick.
            /// </summary>
            public bool IsSprinting { get; set; }

            /// <summary>
            /// Whether sprinting can start or continue.
            /// </summary>
            public bool CanSprint { get; set; }
        }
    }
}
