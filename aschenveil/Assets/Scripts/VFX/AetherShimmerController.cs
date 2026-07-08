using UnityEngine;

namespace Ashenveil.VFX
{
    /// <summary>
    /// Runtime adapter that creates a local aether shimmer and exposes charge-driven intensity.
    /// Referenced GDD section: Demo-Ablauf Phase 5.
    /// </summary>
        public sealed class AetherShimmerController : MonoBehaviour
        {
            [Header("Aether shimmer")]
        [SerializeField] private Color _color = default;
        [SerializeField] private float _baseEmissionRate = ParticleBudget.AetherShimmerRate;

        private ParticleSystem _particles;

        private void Awake()
        {
            if (_color.a <= ParticleBudget.InvisibleAlpha)
            {
                _color = ParticleBudget.AetherDefaultColor;
            }

            GameObject shimmer = VfxFactory.BuildAetherShimmer(transform, _color);
            if (shimmer.TryGetComponent(out ParticleSystem particles))
            {
                _particles = particles;
            }
        }

        /// <summary>
        /// Scales shimmer emission by a charge or activity multiplier.
        /// </summary>
        /// <param name="intensity">Emission multiplier, clamped to the supported particle budget.</param>
        public void SetIntensity(float intensity)
        {
            if (_particles == null)
            {
                return;
            }

            float multiplier = Mathf.Clamp(
                intensity,
                ParticleBudget.MinIntensityMultiplier,
                ParticleBudget.MaxIntensityMultiplier);

            ParticleSystem.EmissionModule emission = _particles.emission;
            emission.rateOverTime = _baseEmissionRate * multiplier;
        }
    }
}
