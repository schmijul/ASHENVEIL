using Ashenveil.Combat;
using Ashenveil.Core;
using NUnit.Framework;
using UnityEngine;

namespace Ashenveil.Tests.EditMode
{
    /// <summary>
    /// EditMode tests for deterministic combat logic.
    /// </summary>
    public sealed class CombatModelTests
    {
        /// <summary>
        /// Verifies light attacks move through windup, active, recovery, and idle.
        /// </summary>
        [Test]
        public void Tick_LightAttack_TransitionsThroughAttackPhases()
        {
            CombatModel.Settings settings = FastSettings();
            CombatModel model = new CombatModel(settings);

            Assert.That(model.TryStartLightAttack(), Is.True);
            Assert.That(model.Phase, Is.EqualTo(CombatAttackPhase.Windup));

            CombatModel.State active = model.Tick(settings.LightWindupDuration);
            CombatModel.State recovery = model.Tick(settings.LightActiveDuration);
            CombatModel.State idle = model.Tick(settings.LightRecoveryDuration);

            Assert.That(active.Phase, Is.EqualTo(CombatAttackPhase.Active));
            Assert.That(active.IsAttackActive, Is.True);
            Assert.That(recovery.Phase, Is.EqualTo(CombatAttackPhase.Recovery));
            Assert.That(idle.Phase, Is.EqualTo(CombatAttackPhase.Idle));
        }

        /// <summary>
        /// Verifies attacks fail when standalone combat stamina is insufficient.
        /// </summary>
        [Test]
        public void TryStartAttack_NotEnoughStamina_Fails()
        {
            CombatModel.Settings settings = FastSettings();
            settings.MaxStamina = 10f;
            settings.LightStaminaCost = 12f;
            CombatModel model = new CombatModel(settings);

            Assert.That(model.TryStartLightAttack(), Is.False);
            Assert.That(model.Phase, Is.EqualTo(CombatAttackPhase.Idle));
            Assert.That(model.CurrentStamina, Is.EqualTo(10f).Within(0.001f));
        }

        /// <summary>
        /// Verifies block reduces incoming damage and spends stamina.
        /// </summary>
        [Test]
        public void ResolveIncomingDamage_Blocking_ReducesDamage()
        {
            CombatModel.Settings settings = FastSettings();
            settings.BlockDamageReduction = 0.5f;
            settings.BlockStaminaDrainPerDamage = 0.25f;
            CombatModel model = new CombatModel(settings);
            model.SetBlocking(true);

            DamageInfo incoming = new DamageInfo(20f, DamageType.Physical, Vector3.zero, 2f);
            DamageInfo resolved = model.ResolveIncomingDamage(incoming);

            Assert.That(resolved.Amount, Is.EqualTo(10f).Within(0.001f));
            Assert.That(model.CurrentStamina, Is.EqualTo(settings.MaxStamina - 5f).Within(0.001f));
        }

        /// <summary>
        /// Verifies a follow-up attack can be queued during the combo window.
        /// </summary>
        [Test]
        public void TryStartAttack_DuringComboWindow_QueuesFollowUp()
        {
            CombatModel.Settings settings = FastSettings();
            settings.ComboWindowDuration = 0.5f;
            CombatModel model = new CombatModel(settings);

            model.TryStartLightAttack();
            model.Tick(settings.LightWindupDuration);
            model.Tick(settings.LightActiveDuration);

            Assert.That(model.IsComboWindowOpen, Is.True);
            Assert.That(model.TryStartLightAttack(), Is.True);

            CombatModel.State followUp = model.Tick(settings.LightRecoveryDuration);

            Assert.That(followUp.Phase, Is.EqualTo(CombatAttackPhase.Windup));
            Assert.That(followUp.AttackType, Is.EqualTo(CombatAttackType.Light));
        }

        private static CombatModel.Settings FastSettings()
        {
            CombatModel.Settings settings = CombatModel.Settings.Default;
            settings.LightWindupDuration = 0.1f;
            settings.LightActiveDuration = 0.1f;
            settings.LightRecoveryDuration = 0.1f;
            settings.HeavyWindupDuration = 0.2f;
            settings.HeavyActiveDuration = 0.1f;
            settings.HeavyRecoveryDuration = 0.2f;
            settings.MaxStamina = 100f;
            settings.LightStaminaCost = 10f;
            settings.HeavyStaminaCost = 25f;
            return settings;
        }
    }
}
