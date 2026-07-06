using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Ashenveil.UI
{
    /// <summary>
    /// Full-screen death overlay with a retry action.
    /// Referenced GDD section: Kernsysteme / UI.
    /// </summary>
    public sealed class DeathScreenController : MonoBehaviour
    {
        private GameObject _root;
        private Action _onRetry;

        private void Awake()
        {
            BuildUi();
            SetVisible(false);
        }

        /// <summary>
        /// Binds the retry callback.
        /// </summary>
        public void Bind(Action onRetry)
        {
            _onRetry = onRetry;
        }

        /// <summary>
        /// Shows the death screen.
        /// </summary>
        public void Show()
        {
            SetVisible(true);
        }

        private void Retry()
        {
            _onRetry?.Invoke();
        }

        private void SetVisible(bool visible)
        {
            if (_root != null)
            {
                _root.SetActive(visible);
            }
        }

        private void BuildUi()
        {
            Canvas canvas = UIFactory.CreateCanvas("Death_Canvas", sortOrder: 110);
            canvas.transform.SetParent(transform, false);

            Image background = UIFactory.CreatePanel("DeathBackground", canvas.transform, new Color(0f, 0f, 0f, 0.92f));
            UIFactory.Stretch(background.rectTransform);
            _root = background.gameObject;

            TextMeshProUGUI title = UIFactory.CreateLabel(
                "Title", background.transform, "Du bist gestorben", UITheme.FontSizeTitle, UITheme.TextWarning, TextAlignmentOptions.Center);
            UIFactory.Place(title.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 130f), new Vector2(900f, 80f));

            TextMeshProUGUI line = UIFactory.CreateLabel(
                "Line", background.transform, "Der Wald fordert seinen Preis. Sammle dich und versuche es erneut.", UITheme.FontSizeBody, UITheme.TextMuted, TextAlignmentOptions.Center);
            UIFactory.Place(line.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 55f), new Vector2(900f, 60f));

            Button retry = UIFactory.CreateButton("RetryButton", background.transform, "Erneut versuchen", Retry);
            UIFactory.Place(retry.image.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -55f), new Vector2(340f, 58f));
        }
    }
}
