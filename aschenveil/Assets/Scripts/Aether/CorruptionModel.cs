using System;
using UnityEngine;

namespace Ashenveil.Aether
{
    /// <summary>
    /// Tracks permanent aether corruption. Corruption only ever rises: touching and
    /// spending aether taints the body. Crossing a stage threshold applies escalating
    /// penalties (data-driven, not hard-wired) and is the risk half of the aether
    /// risk/reward mechanic. Referenced GDD section: Lore — Der Ätherfluss; Kernsysteme / Äther.
    /// </summary>
    public sealed class CorruptionModel
    {
        private readonly float[] _thresholds;
        private readonly float[] _maxHealthPenaltyPerStage;
        private float _corruption;
        private int _stage;

        /// <summary>
        /// Raised when the corruption stage advances. Args: new stage index, max-health penalty fraction.
        /// </summary>
        public event Action<int, float> StageChanged;

        /// <summary>
        /// Creates a corruption model.
        /// </summary>
        /// <param name="thresholds">Ascending corruption points at which each stage begins (e.g. 25/50/75/100).</param>
        /// <param name="maxHealthPenaltyPerStage">Max-health penalty fraction 0..1 applied at each corresponding stage.</param>
        public CorruptionModel(float[] thresholds, float[] maxHealthPenaltyPerStage)
        {
            if (thresholds == null || thresholds.Length == 0)
            {
                throw new ArgumentException("At least one threshold is required.", nameof(thresholds));
            }

            if (maxHealthPenaltyPerStage == null || maxHealthPenaltyPerStage.Length != thresholds.Length)
            {
                throw new ArgumentException("Penalty array must match thresholds length.", nameof(maxHealthPenaltyPerStage));
            }

            _thresholds = (float[])thresholds.Clone();
            _maxHealthPenaltyPerStage = (float[])maxHealthPenaltyPerStage.Clone();
        }

        /// <summary>
        /// Accumulated corruption points (never decreases).
        /// </summary>
        public float Corruption => _corruption;

        /// <summary>
        /// Current corruption stage (0 = pristine, up to thresholds.Length).
        /// </summary>
        public int Stage => _stage;

        /// <summary>
        /// Current max-health penalty fraction from corruption (0..1).
        /// </summary>
        public float MaxHealthPenalty => _stage == 0 ? 0f : _maxHealthPenaltyPerStage[_stage - 1];

        /// <summary>
        /// Adds corruption points and raises <see cref="StageChanged"/> once per crossed stage.
        /// </summary>
        public void Add(float points)
        {
            if (points <= 0f)
            {
                return;
            }

            _corruption += points;

            while (_stage < _thresholds.Length && _corruption >= _thresholds[_stage])
            {
                _stage++;
                StageChanged?.Invoke(_stage, MaxHealthPenalty);
            }
        }
    }
}
