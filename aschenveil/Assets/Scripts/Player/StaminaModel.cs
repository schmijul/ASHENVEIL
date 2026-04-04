using UnityEngine;

namespace Ashenveil.Player
{
    /// <summary>
    /// Pure stamina logic for sprinting and future combat costs.
    /// Referenced GDD sections: 5.2
    /// </summary>
    public sealed class StaminaModel
    {
        private readonly float _maxStamina;
        private readonly float _regenRate;
        private readonly float _regenDelay;
        private readonly float _emptySprintThreshold;
        private float _timeSinceConsumption;

        public StaminaModel(float maxStamina, float startingStamina, float regenRate, float regenDelay, float emptySprintThreshold)
        {
            _maxStamina = Mathf.Max(0f, maxStamina);
            CurrentStamina = Mathf.Clamp(startingStamina < 0f ? _maxStamina : startingStamina, 0f, _maxStamina);
            _regenRate = Mathf.Max(0f, regenRate);
            _regenDelay = Mathf.Max(0f, regenDelay);
            _emptySprintThreshold = Mathf.Max(0f, emptySprintThreshold);
            _timeSinceConsumption = CurrentStamina >= _maxStamina ? _regenDelay : 0f;
        }

        public float CurrentStamina { get; private set; }

        public float MaxStamina => _maxStamina;

        public bool CanSprint => CurrentStamina >= _emptySprintThreshold;

        public bool TryConsume(float amount)
        {
            if (amount <= 0f)
            {
                return true;
            }

            if (CurrentStamina < amount)
            {
                return false;
            }

            CurrentStamina -= amount;
            _timeSinceConsumption = 0f;
            return true;
        }

        public void Restore(float amount)
        {
            if (amount <= 0f)
            {
                return;
            }

            CurrentStamina = Mathf.Min(_maxStamina, CurrentStamina + amount);
            _timeSinceConsumption = CurrentStamina >= _maxStamina ? _regenDelay : 0f;
        }

        public void Tick(float deltaTime)
        {
            if (deltaTime <= 0f || CurrentStamina >= _maxStamina)
            {
                if (CurrentStamina >= _maxStamina)
                {
                    _timeSinceConsumption = _regenDelay;
                }

                return;
            }

            _timeSinceConsumption += deltaTime;
            if (_timeSinceConsumption <= _regenDelay)
            {
                return;
            }

            float regenTime = _timeSinceConsumption - _regenDelay;
            CurrentStamina = Mathf.Min(_maxStamina, CurrentStamina + (_regenRate * regenTime));
            _timeSinceConsumption = CurrentStamina >= _maxStamina ? _regenDelay : _regenDelay;
        }
    }
}
