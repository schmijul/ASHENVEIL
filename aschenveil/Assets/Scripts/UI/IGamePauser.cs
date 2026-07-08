namespace Ashenveil.UI
{
    /// <summary>
    /// Abstraction over pausing gameplay for menu-style screens, so screen controllers
    /// don't hard-code Time.timeScale and stay testable/composable.
    /// </summary>
    public interface IGamePauser
    {
        /// <summary>Requests a gameplay pause.</summary>
        void Pause();

        /// <summary>Releases a previously requested pause.</summary>
        void Resume();
    }

    /// <summary>
    /// Default pauser that toggles <see cref="UnityEngine.Time.timeScale"/>.
    /// </summary>
    public sealed class TimeScalePauser : IGamePauser
    {
        /// <inheritdoc />
        public void Pause()
        {
            UnityEngine.Time.timeScale = 0f;
        }

        /// <inheritdoc />
        public void Resume()
        {
            UnityEngine.Time.timeScale = 1f;
        }
    }
}
