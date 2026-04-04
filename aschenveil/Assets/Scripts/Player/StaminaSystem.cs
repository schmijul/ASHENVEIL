using System;
using UnityEngine;

namespace Ashenveil.Player
{
    /// <summary>
    /// Runtime stamina state and regeneration.
    /// Referenced GDD sections: 5.2
    /// </summary>
    public class StaminaSystem : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerStats _playerStats;

        [Header("Runtime")]
        [SerializeField, Min(0f)] private float _startingStamina = -1f;

        private StaminaModel _model;

        public event Action<float, float> StaminaChanged;

        public float CurrentStamina => _model != null ? _model.CurrentStamina : 0f;

        public float MaxStamina => _model != null ? _model.MaxStamina : 0f;

        public bool CanSprint => _model != null && _model.CanSprint;

        private void Awake()
        {
            ResolvePlayerStats();
            InitializeModel();
        }

        private void Update()
        {
            if (_model == null)
            {
                return;
            }

            float before = _model.CurrentStamina;
            _model.Tick(Time.deltaTime);
            if (!Mathf.Approximately(before, _model.CurrentStamina))
            {
                StaminaChanged?.Invoke(_model.CurrentStamina, _model.MaxStamina);
            }
        }

        private void OnValidate()
        {
            ResolvePlayerStats();
        }

        private void ResolvePlayerStats()
        {
            if (_playerStats == null)
            {
                _playerStats = PlayerStats.CreateRuntimeDefaults();
            }
        }

        private void InitializeModel()
        {
            if (_playerStats == null)
            {
                Debug.LogError($"{nameof(StaminaSystem)} on {name} requires a {nameof(PlayerStats)} reference.");
                enabled = false;
                return;
            }

            _model = new StaminaModel(
                _playerStats.MaxStamina,
                _startingStamina,
                _playerStats.StaminaRegenRate,
                _playerStats.StaminaRegenDelay,
                _playerStats.EmptySprintThreshold);

            StaminaChanged?.Invoke(_model.CurrentStamina, _model.MaxStamina);
        }

        public bool TryConsume(float amount)
        {
            if (_model == null)
            {
                return false;
            }

            bool consumed = _model.TryConsume(amount);
            if (consumed)
            {
                StaminaChanged?.Invoke(_model.CurrentStamina, _model.MaxStamina);
            }

            return consumed;
        }

        public bool TryConsumeSprint(float deltaTime)
        {
            if (_playerStats == null)
            {
                return false;
            }

            return TryConsume(_playerStats.SprintCostPerSecond * deltaTime);
        }

        public void Restore(float amount)
        {
            if (_model == null)
            {
                return;
            }

            _model.Restore(amount);
            StaminaChanged?.Invoke(_model.CurrentStamina, _model.MaxStamina);
        }
    }
}
