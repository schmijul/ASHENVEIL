using UnityEngine;

namespace Ashenveil.Aether
{
    /// <summary>
    /// Pure presentation math for the hand-glow beat: maps aether charge (and the
    /// first-touch story flag) to a light/particle intensity. Kept testable so the
    /// MonoBehaviour view only lerps toward the value this produces.
    /// Referenced GDD section: Demo-Ablauf Phase 5.
    /// </summary>
    public sealed class AetherGlowModel
    {
        private readonly float _baseIntensity;
        private readonly float _maxIntensity;
        private bool _awakened;

        /// <summary>
        /// Creates a glow model.
        /// </summary>
        /// <param name="baseIntensity">Idle glow once the hands have awakened.</param>
        /// <param name="maxIntensity">Glow at full charge.</param>
        public AetherGlowModel(float baseIntensity, float maxIntensity)
        {
            _baseIntensity = Mathf.Max(0f, baseIntensity);
            _maxIntensity = Mathf.Max(_baseIntensity, maxIntensity);
        }

        /// <summary>
        /// Whether the hands have been awakened by the first crystal touch.
        /// </summary>
        public bool Awakened => _awakened;

        /// <summary>
        /// Marks the first-touch story beat; before this the hands never glow.
        /// </summary>
        public void Awaken()
        {
            _awakened = true;
        }

        /// <summary>
        /// Target glow intensity for the given normalized charge (0..1).
        /// Monotonic in charge; zero until awakened.
        /// </summary>
        public float IntensityForChargeFraction(float chargeFraction)
        {
            if (!_awakened)
            {
                return 0f;
            }

            float t = Mathf.Clamp01(chargeFraction);
            return Mathf.Lerp(_baseIntensity, _maxIntensity, t);
        }
    }
}
