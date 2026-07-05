using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Ashenveil.UI
{
    /// <summary>
    /// Fullscreen black overlay for fade-in/out transitions, e.g. the burning-village
    /// cut and the demo-end fade. Referenced GDD section: Demo-Ablauf Phases 7–8.
    /// </summary>
    public sealed class FadeScreenController : MonoBehaviour
    {
        [SerializeField] private float _defaultDuration = 1.5f;

        private Image _overlay;

        private void Awake()
        {
            Canvas canvas = UIFactory.CreateCanvas("Fade_Canvas", sortOrder: 90);
            canvas.transform.SetParent(transform, false);
            _overlay = UIFactory.CreatePanel("FadeOverlay", canvas.transform, new Color(0f, 0f, 0f, 0f));
            UIFactory.Stretch(_overlay.rectTransform);
            _overlay.raycastTarget = false;
        }

        /// <summary>
        /// Fades to black, then invokes <paramref name="onComplete"/>.
        /// </summary>
        public void FadeToBlack(Action onComplete = null, float? duration = null)
        {
            StopAllCoroutines();
            StartCoroutine(FadeRoutine(1f, duration ?? _defaultDuration, onComplete));
        }

        /// <summary>
        /// Fades from black back to clear.
        /// </summary>
        public void FadeFromBlack(Action onComplete = null, float? duration = null)
        {
            StopAllCoroutines();
            StartCoroutine(FadeRoutine(0f, duration ?? _defaultDuration, onComplete));
        }

        private IEnumerator FadeRoutine(float targetAlpha, float duration, Action onComplete)
        {
            _overlay.raycastTarget = targetAlpha > 0.5f;
            float startAlpha = _overlay.color.a;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float a = Mathf.Lerp(startAlpha, targetAlpha, Mathf.Clamp01(elapsed / duration));
                _overlay.color = new Color(0f, 0f, 0f, a);
                yield return null;
            }

            _overlay.color = new Color(0f, 0f, 0f, targetAlpha);
            onComplete?.Invoke();
        }
    }
}
