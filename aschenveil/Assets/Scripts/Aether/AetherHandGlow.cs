using Ashenveil.Core;
using UnityEngine;

namespace Ashenveil.Aether
{
    /// <summary>
    /// Presentation-only adapter: awakens the player's hand glow on first aether touch,
    /// then lerps a Light (and optional ParticleSystem emission) toward the intensity
    /// produced by the pure <see cref="AetherGlowModel"/> as charge changes.
    /// Referenced GDD section: Demo-Ablauf Phase 5.
    /// </summary>
    public sealed class AetherHandGlow : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private AetherPool _aetherPool;
        [SerializeField] private Light _handLight;
        [SerializeField] private ParticleSystem _handParticles;

        [Header("Glow")]
        [SerializeField] private float _baseIntensity = 0.4f;
        [SerializeField] private float _maxIntensity = 2.5f;
        [SerializeField] private float _lerpSpeed = 6f;

        private AetherGlowModel _glow;
        private float _current;

        private void Awake()
        {
            _glow = new AetherGlowModel(_baseIntensity, _maxIntensity);
            if (_handLight != null)
            {
                _handLight.intensity = 0f;
            }
        }

        private void OnEnable()
        {
            GameSignals.AetherTouched += OnAetherTouched;
        }

        private void OnDisable()
        {
            GameSignals.AetherTouched -= OnAetherTouched;
        }

        private void OnAetherTouched(Vector3 worldPosition, float amount)
        {
            _glow.Awaken();
            if (_handParticles != null && !_handParticles.isPlaying)
            {
                _handParticles.Play();
            }
        }

        private void Update()
        {
            if (_glow == null || _aetherPool == null)
            {
                return;
            }

            float target = _glow.IntensityForChargeFraction(_aetherPool.Model.ChargeFraction);
            _current = Mathf.Lerp(_current, target, 1f - Mathf.Exp(-_lerpSpeed * Time.deltaTime));

            if (_handLight != null)
            {
                _handLight.intensity = _current;
            }
        }
    }
}
