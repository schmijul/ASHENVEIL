using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Ashenveil.UI
{
    /// <summary>
    /// Full-screen gothic main menu for starting or exiting the demo.
    /// Referenced GDD section: Kernsysteme / UI.
    /// </summary>
    public sealed class MainMenuController : MonoBehaviour
    {
        private GameObject _root;
        private Action _onStart;

        private void Awake()
        {
            BuildUi();
            Show();
        }

        /// <summary>
        /// Whether the main menu is currently visible.
        /// </summary>
        public bool IsShown => _root != null && _root.activeSelf;

        /// <summary>
        /// Binds the start-game callback.
        /// </summary>
        public void Bind(Action onStart)
        {
            _onStart = onStart;
        }

        /// <summary>
        /// Shows the main menu.
        /// </summary>
        public void Show()
        {
            SetVisible(true);
        }

        /// <summary>
        /// Hides the main menu.
        /// </summary>
        public void Hide()
        {
            SetVisible(false);
        }

        private void StartGame()
        {
            _onStart?.Invoke();
        }

        private void QuitGame()
        {
#if !UNITY_EDITOR
            Application.Quit();
#endif
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
            Canvas canvas = UIFactory.CreateCanvas("MainMenu_Canvas", sortOrder: 100);
            canvas.transform.SetParent(transform, false);

            Image background = UIFactory.CreatePanel("MainMenuBackground", canvas.transform, new Color(0.02f, 0.018f, 0.024f, 0.98f));
            UIFactory.Stretch(background.rectTransform);
            _root = background.gameObject;

            Image frame = UIFactory.CreatePanel("MainMenuFrame", background.transform, new Color(0.08f, 0.075f, 0.09f, 0.70f));
            UIFactory.Place(frame.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(760f, 560f));

            TextMeshProUGUI title = UIFactory.CreateLabel(
                "Title", frame.transform, "ASHENVEIL", 72f, UITheme.TextPrimary, TextAlignmentOptions.Center);
            UIFactory.Place(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -90f), new Vector2(700f, 100f));

            TextMeshProUGUI subtitle = UIFactory.CreateLabel(
                "Subtitle", frame.transform, "Der Ätherfluss", UITheme.FontSizeHeading, UITheme.AetherFill, TextAlignmentOptions.Center);
            UIFactory.Place(subtitle.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -190f), new Vector2(620f, 50f));

            Image divider = UIFactory.CreatePanel("Divider", frame.transform, UITheme.PanelBorder);
            UIFactory.Place(divider.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f), new Vector2(0f, -260f), new Vector2(520f, 2f));

            Button start = UIFactory.CreateButton("StartButton", frame.transform, "Spiel starten", StartGame);
            UIFactory.Place(start.image.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 170f), new Vector2(360f, 58f));

            Button quit = UIFactory.CreateButton("QuitButton", frame.transform, "Beenden", QuitGame);
            UIFactory.Place(quit.image.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 90f), new Vector2(360f, 58f));
        }
    }
}
