using System.Collections;
using Ashenveil.AI;
using Ashenveil.Core;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Ashenveil.Tests.PlayMode
{
    /// <summary>
    /// Verifies the hunt loop is live: the spawners actually populate the world with
    /// wildlife agents, and killing one raises <see cref="GameSignals.AnimalKilled"/> and
    /// drops a lootable container. Complements the phase playthrough, which fakes the kill.
    /// </summary>
    public sealed class GrauwaldWildlifeTest
    {
        [UnityTest]
        public IEnumerator Wildlife_Spawns_And_Kills_Drop_Loot()
        {
            SceneManager.LoadScene("Grauwald");
            yield return null;
            var menu = Object.FindFirstObjectByType<Ashenveil.UI.MainMenuController>();
            if (menu != null) menu.Hide();
            Time.timeScale = 1f;

            // Spawners populate on Update — give them a few frames.
            WildlifeAgent[] agents = null;
            for (int i = 0; i < 60; i++)
            {
                agents = Object.FindObjectsByType<WildlifeAgent>(FindObjectsSortMode.None);
                if (agents.Length > 0) break;
                yield return null;
            }

            Assert.IsNotNull(agents);
            Assert.Greater(agents.Length, 0, "No wildlife spawned from the spawners.");

            // Kill one and confirm the signal fires + loot appears.
            var target = agents[0];
            var health = target.GetComponent<WildlifeHealth>();
            Assert.IsNotNull(health, "Wildlife agent has no health.");

            bool killed = false;
            void OnKill(string id, Vector3 pos) => killed = true;
            GameSignals.AnimalKilled += OnKill;

            int lootBefore = Object.FindObjectsByType<LootContainer>(FindObjectsSortMode.None).Length;

            for (int i = 0; i < 20 && health.IsAlive; i++)
            {
                health.TakeDamage(new DamageInfo(50f, DamageType.Physical, Vector3.zero, 0f));
                yield return null;
            }

            yield return null;
            GameSignals.AnimalKilled -= OnKill;

            Assert.IsFalse(health.IsAlive, "Animal did not die from damage.");
            Assert.IsTrue(killed, "Killing an animal should raise AnimalKilled.");

            int lootAfter = Object.FindObjectsByType<LootContainer>(FindObjectsSortMode.None).Length;
            Assert.Greater(lootAfter, lootBefore, "A kill should drop a lootable container.");
        }
    }
}
