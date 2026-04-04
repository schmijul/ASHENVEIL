using UnityEngine;

namespace Ashenveil.Player
{
    /// <summary>
    /// Player movement and stamina tuning values for the demo.
    /// Referenced GDD sections: 5.2
    /// </summary>
    [CreateAssetMenu(fileName = "PlayerStats", menuName = "Ashenveil/Player/Player Stats")]
    public class PlayerStats : ScriptableObject
    {
        [Header("Movement")]
        [SerializeField, Min(0f)] private float _walkSpeed = 3f;
        [SerializeField, Min(0f)] private float _sprintSpeed = 6f;
        [SerializeField, Min(0f)] private float _jumpHeight = 1.5f;
        [SerializeField] private float _gravity = -19.62f;
        [SerializeField, Min(0.01f)] private float _characterHeight = 1.8f;
        [SerializeField, Min(0.01f)] private float _characterRadius = 0.3f;
        [SerializeField, Min(0f)] private float _slopeLimit = 45f;
        [SerializeField, Min(0f)] private float _stepOffset = 0.3f;

        [Header("Grounding")]
        [SerializeField, Min(0f)] private float _groundCheckRadius = 0.2f;
        [SerializeField, Min(0f)] private float _groundCheckDistance = 0.1f;
        [SerializeField] private LayerMask _groundLayers = ~0;

        [Header("Stamina")]
        [SerializeField, Min(0f)] private float _maxStamina = 100f;
        [SerializeField, Min(0f)] private float _sprintCostPerSecond = 10f;
        [SerializeField, Min(0f)] private float _dodgeCost = 25f;
        [SerializeField, Min(0f)] private float _lightAttackCost = 10f;
        [SerializeField, Min(0f)] private float _heavyAttackCost = 25f;
        [SerializeField, Min(0f)] private float _blockCostPerHit = 5f;
        [SerializeField, Min(0f)] private float _staminaRegenRate = 15f;
        [SerializeField, Min(0f)] private float _staminaRegenDelay = 1f;
        [SerializeField, Min(0f)] private float _emptySprintThreshold = 5f;

        public float WalkSpeed => _walkSpeed;
        public float SprintSpeed => _sprintSpeed;
        public float JumpHeight => _jumpHeight;
        public float Gravity => _gravity;
        public float CharacterHeight => _characterHeight;
        public float CharacterRadius => _characterRadius;
        public float SlopeLimit => _slopeLimit;
        public float StepOffset => _stepOffset;
        public float GroundCheckRadius => _groundCheckRadius;
        public float GroundCheckDistance => _groundCheckDistance;
        public LayerMask GroundLayers => _groundLayers;
        public float MaxStamina => _maxStamina;
        public float SprintCostPerSecond => _sprintCostPerSecond;
        public float DodgeCost => _dodgeCost;
        public float LightAttackCost => _lightAttackCost;
        public float HeavyAttackCost => _heavyAttackCost;
        public float BlockCostPerHit => _blockCostPerHit;
        public float StaminaRegenRate => _staminaRegenRate;
        public float StaminaRegenDelay => _staminaRegenDelay;
        public float EmptySprintThreshold => _emptySprintThreshold;

        public static PlayerStats CreateRuntimeDefaults()
        {
            return CreateInstance<PlayerStats>();
        }
    }
}
