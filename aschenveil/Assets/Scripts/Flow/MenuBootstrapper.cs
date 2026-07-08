using Ashenveil.Player;
using Ashenveil.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Ashenveil.Flow
{
    /// <summary>
    /// Wires the runtime callbacks between the menu screens and pause input that cannot be
    /// serialized from the scene builder: the main menu's start button, ESC → pause toggle,
    /// and the pause menu's return-to-menu action. Referenced GDD section: Kernsysteme / UI.
    /// </summary>
    public sealed class MenuBootstrapper : MonoBehaviour
    {
        [SerializeField] private MainMenuController _mainMenu;
        [SerializeField] private PauseMenuController _pauseMenu;
        [SerializeField] private PauseInput _pauseInput;

        private readonly TimeScalePauser _pauser = new TimeScalePauser();

        private void Start()
        {
            if (_mainMenu != null)
            {
                // Pause the world until the player starts; the menu overlays gameplay.
                _pauser.Pause();
                _mainMenu.Bind(OnStart);
                _mainMenu.Show();
            }

            if (_pauseInput != null && _pauseMenu != null)
            {
                _pauseInput.EscapePressed += OnEscape;
            }

            if (_pauseMenu != null)
            {
                _pauseMenu.Bind(ReloadToMenu);
            }
        }

        private void OnDestroy()
        {
            if (_pauseInput != null)
            {
                _pauseInput.EscapePressed -= OnEscape;
            }
        }

        private void OnStart()
        {
            _mainMenu.Hide();
            _pauser.Resume();
        }

        private void OnEscape()
        {
            // Don't allow pausing while the main menu is still up.
            if (_mainMenu == null || !_mainMenu.IsShown)
            {
                _pauseMenu.Toggle();
            }
        }

        private void ReloadToMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
