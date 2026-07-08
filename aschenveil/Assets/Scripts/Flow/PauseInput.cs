using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Ashenveil.Player
{
    /// <summary>
    /// Small New Input System reader for the Escape key. The scene builder wires
    /// <see cref="EscapePressed"/> to the pause menu.
    /// Referenced GDD section: Kernsysteme / UI.
    /// </summary>
    public sealed class PauseInput : MonoBehaviour
    {
        /// <summary>
        /// Raised when Escape is pressed this frame.
        /// </summary>
        public event Action EscapePressed;

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
            {
                EscapePressed?.Invoke();
            }
        }
    }
}
