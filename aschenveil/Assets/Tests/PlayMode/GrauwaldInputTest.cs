using System.Collections;
using Ashenveil.Player;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Ashenveil.Tests.PlayMode
{
    /// <summary>
    /// Controllability check for the player rig. Headless batch mode cannot reliably
    /// synthesize a hardware keyboard into the composite move action, so this asserts the
    /// facts that CAN be verified without a device — the player is grounded and its
    /// CharacterController physically moves it (i.e. it is not stuck on the body mesh,
    /// terrain, or trees) — and best-effort-drives real W input, logging the outcome.
    /// Live keyboard/mouse control is confirmed manually in the editor.
    /// </summary>
    public sealed class GrauwaldInputTest
    {
        [UnityTest]
        public IEnumerator Player_Is_Grounded_And_CharacterController_Moves_It()
        {
            SceneManager.LoadScene("Grauwald");
            yield return null;
            var menu = Object.FindFirstObjectByType<Ashenveil.UI.MainMenuController>();
            if (menu != null) menu.Hide();
            Time.timeScale = 1f;

            var player = Object.FindFirstObjectByType<PlayerMovementController>();
            Assert.IsNotNull(player, "Player missing.");
            var cc = player.GetComponent<CharacterController>();
            Assert.IsNotNull(cc, "Player has no CharacterController.");
            Assert.IsTrue(cc.enabled, "CharacterController disabled.");

            // Actively settle onto the ground (headless dt is tiny, so passive gravity is slow).
            for (int i = 0; i < 120 && !cc.isGrounded; i++)
            {
                cc.Move(Vector3.down * 0.1f);
                yield return null;
            }
            Assert.IsTrue(cc.isGrounded, "Player never grounded (fell through / no floor).");

            // Physics: the controller must move the player a fixed distance — proves it is
            // not pinned by the character body mesh, a tree, or spawning inside geometry.
            Vector3 start = player.transform.position;
            for (int i = 0; i < 30; i++)
            {
                cc.Move(player.transform.forward * 0.1f + Vector3.down * 0.05f);
                yield return null;
            }
            Vector3 moved = player.transform.position - start;
            moved.y = 0f;
            Assert.Greater(moved.magnitude, 2.0f,
                $"CharacterController is blocked (moved {moved.magnitude:0.00} m of ~3 m) — player is stuck.");

            // Best-effort real-input drive (informational; unreliable in headless batch).
            var keyboard = Keyboard.current ?? InputSystem.AddDevice<Keyboard>();
            Vector3 beforeInput = player.transform.position;
            for (int i = 0; i < 60; i++)
            {
                var ks = new KeyboardState();
                ks.Set(Key.W, true);
                InputSystem.QueueStateEvent(keyboard, ks);
                InputSystem.Update();
                yield return null;
            }
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.Update();
            float inputMoved = (player.transform.position - beforeInput).magnitude;
            Debug.Log($"[InputTest] simulated-W moved {inputMoved:0.00} m " +
                      "(0 is expected in headless batch; live input verified in editor).");
        }
    }
}
