using System.Collections;
using System.Collections.Generic;
using Ashenveil.AI.Boss;
using Ashenveil.Aether;
using Ashenveil.Core;
using Ashenveil.Flow;
using Ashenveil.Player;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Ashenveil.Tests.PlayMode
{
    /// <summary>
    /// Boots the real Grauwald scene in Play mode, runs it for a few frames, and asserts
    /// no runtime exceptions plus that the core loop is live (player falls to ground,
    /// aether flows to the boss gate). This is the end-to-end "does it actually run" check.
    /// </summary>
    public sealed class GrauwaldSmokeTest
    {
        private readonly List<string> _errors = new List<string>();

        [SetUp]
        public void SetUp()
        {
            Application.logMessageReceived += OnLog;
        }

        [TearDown]
        public void TearDown()
        {
            Application.logMessageReceived -= OnLog;
        }

        private void OnLog(string condition, string stackTrace, LogType type)
        {
            if (type == LogType.Exception || type == LogType.Error)
            {
                _errors.Add(condition);
            }
        }

        [UnityTest]
        public IEnumerator Scene_Boots_And_Runs_Without_Errors()
        {
            SceneManager.LoadScene("Grauwald");
            yield return null; // let Awake/Start run
            var menu = Object.FindFirstObjectByType<Ashenveil.UI.MainMenuController>();
            if (menu != null) menu.Hide();
            Time.timeScale = 1f;

            // Run ~2 seconds of gameplay.
            for (int i = 0; i < 120; i++)
            {
                yield return null;
            }

            var player = Object.FindFirstObjectByType<PlayerMovementController>();
            Assert.IsNotNull(player, "Player not present in scene.");

            var director = Object.FindFirstObjectByType<DemoDirector>();
            Assert.IsNotNull(director, "DemoDirector missing.");
            Assert.IsNotNull(director.Flow, "DemoFlow not initialized.");

            // Exercise the aether→boss gate directly to prove the systems interoperate.
            var pool = Object.FindFirstObjectByType<AetherPool>();
            Assert.IsNotNull(pool, "AetherPool missing.");
            pool.Model.Absorb(50f);
            Assert.Greater(pool.Model.Charge, 0f, "Aether did not absorb.");

            var boss = Object.FindFirstObjectByType<MutatedWolfBossController>();
            Assert.IsNotNull(boss, "Boss missing.");
            float before = boss.IsAlive ? 1f : 0f;
            boss.TakeDamage(new DamageInfo(20f, DamageType.Physical, Vector3.zero, 0f));
            boss.TakeDamage(new DamageInfo(20f, DamageType.Aether, Vector3.zero, 0f));
            Assert.AreEqual(1f, before, "Boss should have started alive.");

            CollectionAssert.IsEmpty(_errors, "Runtime errors during play:\n" + string.Join("\n", _errors));
        }
    }
}
