using System.Collections.Generic;
using Ashenveil.Combat;
using NUnit.Framework;

namespace Ashenveil.Tests.EditMode
{
    public class CombatContractsTests
    {
        [Test]
        public void CalculateFinalDamage_ArmorExceedsAttack_ClampsToMinimumDamage()
        {
            float damage = DamageResolver.CalculateFinalDamage(12f, 1f, 50f);

            Assert.That(damage, Is.EqualTo(1f).Within(0.0001f));
        }

        [Test]
        public void TryBeginLightAttack_WithEnoughStamina_StartsFirstComboStep()
        {
            CombatRuntimeState state = CreateRuntimeState();

            bool started = state.TryBeginLightAttack(100f, out AttackStepDefinition attackStep, out float staminaCost);

            Assert.That(started, Is.True);
            Assert.That(state.CurrentAction, Is.EqualTo(CombatActionKind.LightAttack));
            Assert.That(state.CurrentComboStepIndex, Is.EqualTo(1));
            Assert.That(attackStep.AnimationTrigger, Is.EqualTo("LightAttack1"));
            Assert.That(staminaCost, Is.EqualTo(10f).Within(0.0001f));
        }

        [Test]
        public void TryBeginLightAttack_AfterRecoveryWithinComboWindow_AdvancesComboStep()
        {
            CombatRuntimeState state = CreateRuntimeState();

            Assert.That(state.TryBeginLightAttack(100f, out _, out _), Is.True);
            state.Tick(0.41f);

            Assert.That(state.CurrentAction, Is.EqualTo(CombatActionKind.Idle));
            Assert.That(state.IsComboWindowOpen, Is.True);

            bool started = state.TryBeginLightAttack(100f, out AttackStepDefinition secondAttack, out float staminaCost);

            Assert.That(started, Is.True);
            Assert.That(state.CurrentComboStepIndex, Is.EqualTo(2));
            Assert.That(secondAttack.AnimationTrigger, Is.EqualTo("LightAttack2"));
            Assert.That(staminaCost, Is.EqualTo(10f).Within(0.0001f));
        }

        [Test]
        public void TryBeginDodge_DuringOpenComboWindow_ResetsComboAndStartsCooldown()
        {
            CombatRuntimeState state = CreateRuntimeState();

            Assert.That(state.TryBeginLightAttack(100f, out _, out _), Is.True);
            state.Tick(0.41f);

            bool started = state.TryBeginDodge(100f, out DodgeSettings dodgeSettings, out float staminaCost);

            Assert.That(started, Is.True);
            Assert.That(state.CurrentAction, Is.EqualTo(CombatActionKind.Dodging));
            Assert.That(state.CurrentComboStepIndex, Is.EqualTo(0));
            Assert.That(state.CanDodge, Is.False);
            Assert.That(dodgeSettings.Duration, Is.EqualTo(0.5f).Within(0.0001f));
            Assert.That(staminaCost, Is.EqualTo(25f).Within(0.0001f));
        }

        [Test]
        public void TryResolveBlockHit_InsidePerfectWindow_NegatesDamageAndAppliesStagger()
        {
            CombatRuntimeState state = CreateRuntimeState();

            Assert.That(state.TryBeginBlock(), Is.True);

            bool resolved = state.TryResolveBlockHit(
                100f,
                out float staminaDrain,
                out float damageMultiplier,
                out float perfectBlockStaggerDuration,
                out float guardBreakStaggerDuration);

            Assert.That(resolved, Is.True);
            Assert.That(staminaDrain, Is.EqualTo(0f).Within(0.0001f));
            Assert.That(damageMultiplier, Is.EqualTo(0f).Within(0.0001f));
            Assert.That(perfectBlockStaggerDuration, Is.EqualTo(1f).Within(0.0001f));
            Assert.That(guardBreakStaggerDuration, Is.EqualTo(0f).Within(0.0001f));
        }

        [Test]
        public void TryResolveBlockHit_WithLowStaminaFlagsGuardBreak()
        {
            CombatRuntimeState state = CreateRuntimeState();

            Assert.That(state.TryBeginBlock(), Is.True);
            state.Tick(0.25f);

            bool resolved = state.TryResolveBlockHit(
                4f,
                out float staminaDrain,
                out float damageMultiplier,
                out float perfectBlockStaggerDuration,
                out float guardBreakStaggerDuration);

            Assert.That(resolved, Is.True);
            Assert.That(staminaDrain, Is.EqualTo(5f).Within(0.0001f));
            Assert.That(damageMultiplier, Is.EqualTo(0.3f).Within(0.0001f));
            Assert.That(perfectBlockStaggerDuration, Is.EqualTo(0f).Within(0.0001f));
            Assert.That(guardBreakStaggerDuration, Is.EqualTo(1.5f).Within(0.0001f));
        }

        private static CombatRuntimeState CreateRuntimeState()
        {
            return new CombatRuntimeState(
                new List<AttackStepDefinition>
                {
                    AttackStepDefinition.CreateLightAttackStep(0),
                    AttackStepDefinition.CreateLightAttackStep(1),
                    AttackStepDefinition.CreateLightAttackStep(2)
                },
                AttackStepDefinition.CreateHeavyAttackStep(),
                BlockSettings.CreateDefault(),
                DodgeSettings.CreateDefault());
        }
    }
}
