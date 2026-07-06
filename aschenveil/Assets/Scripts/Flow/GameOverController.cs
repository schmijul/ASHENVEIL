using Ashenveil.Player;
using Ashenveil.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Ashenveil.Flow
{
    /// <summary>
    /// Shows the death screen when the player dies and reloads the current scene on retry.
    /// Referenced GDD section: Kernsysteme / UI.
    /// </summary>
    public sealed class GameOverController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerVitals _vitals;
        [SerializeField] private DeathScreenController _deathScreen;

        private void Awake()
        {
            if (_deathScreen != null)
            {
                _deathScreen.Bind(Retry);
            }
        }

        private void OnEnable()
        {
            if (_vitals != null)
            {
                _vitals.Died += OnDied;
            }
        }

        private void OnDisable()
        {
            if (_vitals != null)
            {
                _vitals.Died -= OnDied;
            }
        }

        private void OnDied()
        {
            _deathScreen?.Show();
        }

        private void Retry()
        {
            Time.timeScale = 1f;
            Scene activeScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(activeScene.buildIndex);
        }
    }
}
