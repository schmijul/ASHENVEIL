using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Ashenveil.UI
{
    /// <summary>
    /// Pause overlay that freezes gameplay through <see cref="IGamePauser"/>.
    /// Referenced GDD section: Kernsysteme / UI.
    /// </summary>
    public sealed class PauseMenuController : MonoBehaviour
    {
        private GameObject _root;
        private Action _onMainMenu;
        private IGamePauser _pauser;
        private bool _isPaused;

        private void Awake()
        {
            _pauser = new TimeScalePauser();
            BuildUi();
            Hide();
        }

        private void OnDisable()
        {
            if (_isPaused)
            {
                _pauser?.Resume();
                _isPaused = false;
            }
        }

        /// <summary>
        /// Binds the main-menu callback.
        /// </summary>
        public void Bind(Action onMainMenu)
        {
            _onMainMenu = onMainMenu;
        }

        /// <summary>
        /// Overrides the pauser for tests or shared pause handling.
        /// </summary>
        public void SetPauser(IGamePauser pauser)
        {
            if (pauser != null)
            {
                _pauser = pauser;
            }
        }

        /// <summary>
        /// Opens or closes the pause menu.
        /// </summary>
        public void Toggle()
        {
            if (_isPaused)
            {
                Hide();
            }
            else
            {
                Show();
            }
        }

        /// <summary>
        /// Shows the pause menu and pauses gameplay.
        /// </summary>
        public void Show()
        {
            SetVisible(true);
            _pauser?.Pause();
            _isPaused = true;
        }

        /// <summary>
        /// Hides the pause menu and resumes gameplay.
        /// </summary>
        public void Hide()
        {
            SetVisible(false);
            if (_isPaused)
            {
                _pauser?.Resume();
                _isPaused = false;
            }
        }

        private void GoToMainMenu()
        {
            Hide();
            _onMainMenu?.Invoke();
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
            Canvas canvas = UIFactory.CreateCanvas("PauseMenu_Canvas", sortOrder: 105);
            canvas.transform.SetParent(transform, false);

            Image background = UIFactory.CreatePanel("PauseMenuBackground", canvas.transform, new Color(0f, 0f, 0f, 0.72f));
            UIFactory.Stretch(background.rectTransform);
            _root = background.gameObject;

            Image panel = UIFactory.CreatePanel("PausePanel", background.transform, UITheme.PanelBackground);
            UIFactory.Place(panel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(560f, 430f));

            TextMeshProUGUI title = UIFactory.CreateLabel(
                "Title", panel.transform, "Pausiert", UITheme.FontSizeTitle, UITheme.TextPrimary, TextAlignmentOptions.Center);
            UIFactory.Place(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -55f), new Vector2(500f, 70f));

            Button resume = UIFactory.CreateButton("ResumeButton", panel.transform, "Weiter", Hide);
            UIFactory.Place(resume.image.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 170f), new Vector2(340f, 58f));

            Button mainMenu = UIFactory.CreateButton("MainMenuButton", panel.transform, "Zum Hauptmenü", GoToMainMenu);
            UIFactory.Place(mainMenu.image.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 90f), new Vector2(340f, 58f));
        }
    }
}
