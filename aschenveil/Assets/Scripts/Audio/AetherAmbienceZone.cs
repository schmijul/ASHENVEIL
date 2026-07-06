using UnityEngine;

namespace Ashenveil.Audio
{
    public sealed class AetherAmbienceZone : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private float _loopSeconds = 6f;
        [Range(0f, 1f)]
        [SerializeField] private float _volume = 0.42f;
        [SerializeField] private float _minDistance = 2.5f;
        [SerializeField] private float _maxDistance = 18f;

        private AudioSource _source;
        private AudioClip _clip;

        private void Awake()
        {
            _clip = ProceduralAudio.AetherHum(_loopSeconds);

            _source = gameObject.AddComponent<AudioSource>();
            _source.playOnAwake = false;
            _source.loop = true;
            _source.clip = _clip;
            _source.volume = _volume;
            _source.spatialBlend = 1f;
            _source.rolloffMode = AudioRolloffMode.Linear;
            _source.minDistance = _minDistance;
            _source.maxDistance = Mathf.Max(_minDistance + 0.1f, _maxDistance);
            _source.dopplerLevel = 0f;
            _source.Play();
        }
    }
}
