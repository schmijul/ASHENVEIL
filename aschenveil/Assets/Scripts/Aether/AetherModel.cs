using System;
using UnityEngine;

namespace Ashenveil.Aether
{
    /// <summary>
    /// The player's aether reserve: charge gained from crystals, spent to empower attacks.
    /// Every point spent adds permanent corruption via the coupled <see cref="CorruptionModel"/>,
    /// encoding the risk/reward mechanic. Charge passively decays; corruption never does.
    /// Referenced GDD section: Kernsysteme / Äther.
    /// </summary>
    public sealed class AetherModel
    {
        private readonly float _maxCharge;
        private readonly float _passiveDecayPerSecond;
        private readonly float _corruptionPerChargeSpent;
        private readonly CorruptionModel _corruption;
        private float _charge;

        /// <summary>
        /// Raised when the current charge changes. Args: charge, maxCharge.
        /// </summary>
        public event Action<float, float> ChargeChanged;

        /// <summary>
        /// Creates an aether model.
        /// </summary>
        public AetherModel(
            float maxCharge,
            float passiveDecayPerSecond,
            float corruptionPerChargeSpent,
            CorruptionModel corruption)
        {
            if (maxCharge <= 0f)
            {
                throw new ArgumentException("maxCharge must be positive.", nameof(maxCharge));
            }

            _maxCharge = maxCharge;
            _passiveDecayPerSecond = Mathf.Max(0f, passiveDecayPerSecond);
            _corruptionPerChargeSpent = Mathf.Max(0f, corruptionPerChargeSpent);
            _corruption = corruption ?? throw new ArgumentNullException(nameof(corruption));
        }

        /// <summary>
        /// Current aether charge (0..max).
        /// </summary>
        public float Charge => _charge;

        /// <summary>
        /// Maximum aether charge.
        /// </summary>
        public float MaxCharge => _maxCharge;

        /// <summary>
        /// Normalized charge for UI bars (0..1).
        /// </summary>
        public float ChargeFraction => _charge / _maxCharge;

        /// <summary>
        /// Coupled corruption model.
        /// </summary>
        public CorruptionModel Corruption => _corruption;

        /// <summary>
        /// Absorbs charge from a crystal, clamped to the maximum.
        /// </summary>
        public void Absorb(float amount)
        {
            if (amount <= 0f)
            {
                return;
            }

            SetCharge(_charge + amount);
        }

        /// <summary>
        /// Returns whether an empowered action costing <paramref name="cost"/> can be paid.
        /// </summary>
        public bool CanSpend(float cost)
        {
            return cost > 0f && _charge >= cost;
        }

        /// <summary>
        /// Spends charge for an empowered attack and adds proportional corruption.
        /// </summary>
        /// <returns>True if the cost was paid; false if there was insufficient charge.</returns>
        public bool SpendForEmpoweredAttack(float cost)
        {
            if (!CanSpend(cost))
            {
                return false;
            }

            SetCharge(_charge - cost);
            _corruption.Add(cost * _corruptionPerChargeSpent);
            return true;
        }

        /// <summary>
        /// Advances passive charge decay by the elapsed time.
        /// </summary>
        public void Tick(float deltaTime)
        {
            if (deltaTime <= 0f || _passiveDecayPerSecond <= 0f || _charge <= 0f)
            {
                return;
            }

            SetCharge(_charge - _passiveDecayPerSecond * deltaTime);
        }

        private void SetCharge(float value)
        {
            float clamped = Mathf.Clamp(value, 0f, _maxCharge);
            if (Mathf.Approximately(clamped, _charge))
            {
                _charge = clamped;
                return;
            }

            _charge = clamped;
            ChargeChanged?.Invoke(_charge, _maxCharge);
        }
    }
}
