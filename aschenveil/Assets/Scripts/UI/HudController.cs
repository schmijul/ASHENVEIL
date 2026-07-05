using Ashenveil.Aether;
using Ashenveil.Core;
using Ashenveil.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Ashenveil.UI
{
    /// <summary>
    /// Always-on heads-up display: health, stamina, aether charge and corruption bars,
    /// the interaction prompt, and the tutorial toast line. Builds itself programmatically
    /// via <see cref="UIFactory"/>. Referenced GDD section: Kernsysteme / UI.
    /// </summary>
    public sealed class HudController : MonoBehaviour
    {
        [SerializeField] private PlayerVitals _vitals;
        [SerializeField] private AetherPool _aetherPool;
        [SerializeField] private float _tutorialDuration = 5f;

        private Image _healthFill;
        private Image _staminaFill;
        private Image _aetherFill;
        private Image _corruptionFill;
        private TextMeshProUGUI _promptLabel;
        private TextMeshProUGUI _toastLabel;
        private TutorialToastModel _toasts;

        /// <summary>
        /// The tutorial toast queue driving the hint line.
        /// </summary>
        public TutorialToastModel Toasts => _toasts;

        private void Awake()
        {
            _toasts = new TutorialToastModel(_tutorialDuration);
            BuildUi();
            _toasts.ToastShown += text => SetToast(text);
            _toasts.ToastHidden += () => SetToast(string.Empty);
        }

        private void Update()
        {
            _toasts.Tick(Time.unscaledDeltaTime);

            if (_vitals != null)
            {
                _healthFill.fillAmount = Mathf.Clamp01(_vitals.CurrentHealth / _vitals.MaxHealth);
                _staminaFill.fillAmount = _vitals.Stamina != null ? _vitals.Stamina.Normalized : 0f;
            }

            if (_aetherPool != null)
            {
                _aetherFill.fillAmount = _aetherPool.Model.ChargeFraction;
                float corr = _aetherPool.Corruption.Corruption;
                _corruptionFill.fillAmount = Mathf.Clamp01(corr / 100f);
            }
        }

        /// <summary>
        /// Sets the interaction prompt line (empty string hides it).
        /// </summary>
        public void SetPrompt(string prompt)
        {
            if (_promptLabel != null)
            {
                _promptLabel.text = prompt ?? string.Empty;
            }
        }

        private void SetToast(string text)
        {
            if (_toastLabel != null)
            {
                _toastLabel.text = text ?? string.Empty;
            }
        }

        private void BuildUi()
        {
            Canvas canvas = UIFactory.CreateCanvas("HUD_Canvas", sortOrder: 10);
            canvas.transform.SetParent(transform, false);

            // Bottom-left vitals stack.
            RectTransform bars = UIFactory.CreateRect("Bars", canvas.transform);
            UIFactory.Place(bars, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(40f, 40f), new Vector2(360f, 120f));

            _healthFill = MakeBar(bars, "Health", UITheme.HealthFill, 0f);
            _staminaFill = MakeBar(bars, "Stamina", UITheme.StaminaFill, 32f);
            _aetherFill = MakeBar(bars, "Aether", UITheme.AetherFill, 64f);
            _corruptionFill = MakeBar(bars, "Corruption", UITheme.CorruptionFill, 96f);
            _corruptionFill.fillAmount = 0f;

            // Center-bottom interaction prompt.
            _promptLabel = UIFactory.CreateLabel(
                "Prompt", canvas.transform, string.Empty, UITheme.FontSizeBody, UITheme.TextPrimary, TextAlignmentOptions.Center);
            UIFactory.Place(_promptLabel.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 180f), new Vector2(700f, 40f));

            // Center-top tutorial toast.
            _toastLabel = UIFactory.CreateLabel(
                "Toast", canvas.transform, string.Empty, UITheme.FontSizeHeading, UITheme.TextPrimary, TextAlignmentOptions.Center);
            UIFactory.Place(_toastLabel.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -80f), new Vector2(1000f, 80f));
        }

        private Image MakeBar(Transform parent, string name, Color color, float yOffset)
        {
            Image fill = UIFactory.CreateBar(name, parent, color);
            RectTransform track = fill.rectTransform.parent as RectTransform;
            UIFactory.Place(track, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, -yOffset), new Vector2(340f, 24f));
            return fill;
        }
    }
}
