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
    /// Drives the real Grauwald scene through all eight demo phases end-to-end, asserting
    /// each transition fires. This is the headless "playthrough": it exercises the actual
    /// runtime wiring (director, signals, crystal, boss, escape) rather than models in
    /// isolation, so a broken integration point shows up as a failing step.
    /// </summary>
    public sealed class GrauwaldPlaythroughTest
    {
        private readonly List<string> _errors = new List<string>();

        [SetUp]
        public void SetUp() => Application.logMessageReceived += OnLog;

        [TearDown]
        public void TearDown() => Application.logMessageReceived -= OnLog;

        private void OnLog(string condition, string stack, LogType type)
        {
            if (type == LogType.Exception || type == LogType.Error)
            {
                _errors.Add(condition);
            }
        }

        private static T Find<T>() where T : Object => Object.FindFirstObjectByType<T>();

        [UnityTest]
        public IEnumerator Full_Demo_Loop_Reaches_Demo_Ende()
        {
            SceneManager.LoadScene("Grauwald");
            yield return null;
            // Simulate pressing "Spiel starten": hide the main menu and unpause.
            var menu = Find<Ashenveil.UI.MainMenuController>();
            if (menu != null) menu.Hide();
            Time.timeScale = 1f;
            for (int i = 0; i < 30; i++) yield return null; // settle

            var director = Find<DemoDirector>();
            Assert.IsNotNull(director, "DemoDirector missing.");
            DemoFlowModel flow = director.Flow;

            // --- Phase 1: wake in forest ---
            Assert.AreEqual(DemoPhase.WakeInForest, flow.Phase, "Should start in WakeInForest.");

            // Player should fall to the ground under gravity within a second.
            var player = Find<PlayerMovementController>();
            Assert.IsNotNull(player, "Player missing.");
            var cc = player.GetComponent<CharacterController>();
            for (int i = 0; i < 120 && !cc.isGrounded; i++) yield return null;
            Assert.IsTrue(cc.isGrounded, "Player never became grounded (fell through / no floor).");

            // --- Phase 2: hunt (first kill starts it) ---
            GameSignals.RaiseAnimalKilled("deer", player.transform.position);
            yield return null;
            Assert.AreEqual(DemoPhase.Hunt, flow.Phase, "AnimalKilled should advance to Hunt.");

            // --- Phase 3: enter village ---
            director.NotifyEnteredVillage();
            yield return null;
            Assert.AreEqual(DemoPhase.EnterVillage, flow.Phase, "Entering village should advance to EnterVillage.");

            // --- Phase 4: village quests ---
            director.NotifyVillageQuestsStarted();
            yield return null;
            Assert.AreEqual(DemoPhase.VillageQuests, flow.Phase, "Starting quests should advance to VillageQuests.");

            // --- Phase 5: deep forest + crystal ---
            director.NotifyEnteredDeepForest();
            yield return null;
            Assert.AreEqual(DemoPhase.DeepForestCrystal, flow.Phase, "Entering deep forest should advance to DeepForestCrystal.");

            // Touch the crystal for real (grants charge + fires the story beat).
            var crystal = Find<AetherCrystal>();
            var pool = Find<AetherPool>();
            Assert.IsNotNull(crystal, "AetherCrystal missing.");
            Assert.IsNotNull(pool, "AetherPool missing.");
            crystal.Interact(player.Context);
            yield return null;
            Assert.Greater(pool.Model.Charge, 0f, "Touching the crystal should grant aether charge.");
            Assert.AreEqual(DemoPhase.BossFight, flow.Phase, "Aether touch should advance to BossFight.");

            // Corruption should now bite: spending enough aether shrinks max HP.
            var vitals = player.GetComponent<Ashenveil.Player.PlayerVitals>();
            float maxBefore = vitals.MaxHealth;
            for (int i = 0; i < 12; i++)
            {
                pool.Model.Absorb(30f);
                pool.Model.SpendForEmpoweredAttack(30f); // adds corruption
            }
            yield return null;
            Assert.Less(vitals.MaxHealth, maxBefore, "Corruption should reduce the player's max health.");

            // --- Phase 6: boss (aether-only) ---
            var boss = Find<MutatedWolfBossController>();
            Assert.IsNotNull(boss, "Boss missing.");
            // Physical damage should be nearly useless; aether should kill.
            for (int i = 0; i < 60 && boss.IsAlive; i++)
            {
                boss.TakeDamage(new DamageInfo(40f, DamageType.Aether, Vector3.zero, 0f));
                yield return null;
            }
            Assert.IsFalse(boss.IsAlive, "Boss should be defeatable with aether damage.");

            // --- Phase 7 → 8: burning village then escape (director drives a fade) ---
            float timeout = 6f;
            while (flow.Phase != DemoPhase.EscapeChoice && timeout > 0f)
            {
                timeout -= Time.unscaledDeltaTime;
                yield return null;
            }
            Assert.AreEqual(DemoPhase.EscapeChoice, flow.Phase, "Boss death should lead through BurningVillage to EscapeChoice.");

            // --- Choose an escape direction: demo ends ---
            bool ended = false;
            flow.DemoEnded += _ => ended = true;
            flow.ChooseEscape(EscapeDirection.Hohensang);
            yield return null;
            Assert.IsTrue(ended, "Choosing a direction should end the demo.");
            Assert.IsTrue(flow.IsFinished, "Flow should be finished after escape.");

            CollectionAssert.IsEmpty(_errors, "Runtime errors during playthrough:\n" + string.Join("\n", _errors));
        }
    }
}
