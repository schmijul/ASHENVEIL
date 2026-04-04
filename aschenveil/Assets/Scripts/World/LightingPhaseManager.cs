using System;
using System.Collections;
using UnityEngine;

namespace Ashenveil.World
{
    /// <summary>
    /// Applies and blends world lighting phases for the demo.
    /// Referenced GDD section: 3.4
    /// </summary>
    public class LightingPhaseManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private LightingPhaseProfile _profile;
        [SerializeField] private Light _directionalLight;

        [Header("Runtime")]
        [SerializeField, Min(0)] private int _startingPhaseIndex;

        private Coroutine _transitionRoutine;

        public event Action<int, LightingPhaseProfile.LightingPhaseDefinition> PhaseChanged;

        public int CurrentPhaseIndex { get; private set; }

        private void Awake()
        {
            if (_directionalLight == null && !TryGetComponent(out _directionalLight))
            {
                Debug.LogError($"{nameof(LightingPhaseManager)} on {name} requires a directional Light reference.");
                enabled = false;
                return;
            }
        }

        private void Start()
        {
            if (_profile != null && _profile.HasMinimumPhaseCount(1))
            {
                ApplyPhaseInstant(_startingPhaseIndex);
            }
        }

        public void ApplyPhaseInstant(int phaseIndex)
        {
            if (_profile == null || !_profile.HasMinimumPhaseCount(1))
            {
                return;
            }

            if (_transitionRoutine != null)
            {
                StopCoroutine(_transitionRoutine);
                _transitionRoutine = null;
            }

            CurrentPhaseIndex = ClampPhaseIndex(phaseIndex);
            LightingPhaseProfile.LightingPhaseDefinition phase = _profile.GetPhase(CurrentPhaseIndex);
            ApplyPhaseState(phase);
            PhaseChanged?.Invoke(CurrentPhaseIndex, phase);
        }

        public void TransitionToPhase(int phaseIndex, float durationSeconds = 2f)
        {
            if (_profile == null || !_profile.HasMinimumPhaseCount(1))
            {
                return;
            }

            int targetIndex = ClampPhaseIndex(phaseIndex);
            if (_transitionRoutine != null)
            {
                StopCoroutine(_transitionRoutine);
            }

            _transitionRoutine = StartCoroutine(TransitionRoutine(CurrentPhaseIndex, targetIndex, durationSeconds));
        }

        private IEnumerator TransitionRoutine(int fromIndex, int toIndex, float durationSeconds)
        {
            LightingPhaseProfile.LightingPhaseDefinition from = _profile.GetPhase(fromIndex);
            LightingPhaseProfile.LightingPhaseDefinition to = _profile.GetPhase(toIndex);
            float elapsed = 0f;
            float duration = Mathf.Max(0.01f, durationSeconds);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                ApplyPhaseState(LightingPhaseMath.Lerp(from, to, t));
                yield return null;
            }

            CurrentPhaseIndex = toIndex;
            ApplyPhaseState(to);
            PhaseChanged?.Invoke(CurrentPhaseIndex, to);
            _transitionRoutine = null;
        }

        private void ApplyPhaseState(LightingPhaseProfile.LightingPhaseDefinition phase)
        {
            if (_directionalLight != null)
            {
                _directionalLight.enabled = phase.directionalLightEnabled;
                _directionalLight.color = phase.directionalColor;
                _directionalLight.intensity = phase.directionalIntensity;
                _directionalLight.transform.rotation = Quaternion.Euler(phase.directionalAngle, 0f, 0f);
            }

            RenderSettings.ambientLight = phase.ambientColor;
            RenderSettings.fogColor = phase.fogColor;
        }

        private int ClampPhaseIndex(int phaseIndex)
        {
            return Mathf.Clamp(phaseIndex, 0, _profile.PhaseCount - 1);
        }
    }
}
