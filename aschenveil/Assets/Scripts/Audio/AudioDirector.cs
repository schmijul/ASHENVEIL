using Ashenveil.Core;
using UnityEngine;

namespace Ashenveil.Audio
{
    public sealed class AudioDirector : MonoBehaviour
    {
        [Header("Loop Durations")]
        [SerializeField] private float _windLoopSeconds = 8f;

        [Header("Volumes")]
        [Range(0f, 1f)]
        [SerializeField] private float _windVolume = 0.35f;
        [Range(0f, 1f)]
        [SerializeField] private float _swingVolume = 0.7f;
        [Range(0f, 1f)]
        [SerializeField] private float _hitVolume = 0.75f;
        [Range(0f, 1f)]
        [SerializeField] private float _uiVolume = 0.45f;

        private AudioSource _windSource;
        private AudioSource _oneShotSource;
        private AudioClip _windClip;
        private AudioClip _swingClip;
        private AudioClip _hitClip;
        private AudioClip _uiClip;

        private void Awake()
        {
            _windClip = ProceduralAudio.Wind(_windLoopSeconds);
            _swingClip = ProceduralAudio.SwordSwing();
            _hitClip = ProceduralAudio.Hit();
            _uiClip = ProceduralAudio.Ui();

            _windSource = CreateSource("Wind Ambience Source");
            _windSource.loop = true;
            _windSource.clip = _windClip;
            _windSource.volume = _windVolume;
            _windSource.Play();

            _oneShotSource = CreateSource("One Shot Audio Source");
            _oneShotSource.volume = 1f;
        }

        private void OnEnable()
        {
            GameSignals.VendorTransaction += OnVendorTransaction;
            GameSignals.DialogFinished += OnDialogFinished;
            GameSignals.AnimalKilled += OnAnimalKilled;
            GameSignals.BossDefeated += OnBossDefeated;
        }

        private void OnDisable()
        {
            GameSignals.VendorTransaction -= OnVendorTransaction;
            GameSignals.DialogFinished -= OnDialogFinished;
            GameSignals.AnimalKilled -= OnAnimalKilled;
            GameSignals.BossDefeated -= OnBossDefeated;
        }

        public void PlaySwing()
        {
            PlayOneShot(_swingClip, _swingVolume);
        }

        public void PlayUi()
        {
            PlayOneShot(_uiClip, _uiVolume);
        }

        private AudioSource CreateSource(string sourceName)
        {
            GameObject sourceObject = new GameObject(sourceName);
            sourceObject.transform.SetParent(transform, false);

            AudioSource source = sourceObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            source.dopplerLevel = 0f;
            return source;
        }

        private void PlayOneShot(AudioClip clip, float volume)
        {
            if (_oneShotSource == null || clip == null)
            {
                return;
            }

            _oneShotSource.PlayOneShot(clip, volume);
        }

        private void OnVendorTransaction(ItemDefinition item, int amount, int goldDelta, bool wasPurchase)
        {
            PlayUi();
        }

        private void OnDialogFinished(string dialogId)
        {
            PlayUi();
        }

        private void OnAnimalKilled(string animalId, Vector3 worldPosition)
        {
            PlayOneShot(_hitClip, _hitVolume);
        }

        private void OnBossDefeated(string bossId)
        {
            PlayOneShot(_hitClip, _hitVolume);
        }
    }
}
