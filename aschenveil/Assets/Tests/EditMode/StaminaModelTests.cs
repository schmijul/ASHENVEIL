using Ashenveil.Player;
using NUnit.Framework;

namespace Ashenveil.Tests.EditMode
{
    /// <summary>
    /// EditMode tests for stamina logic.
    /// </summary>
    public sealed class StaminaModelTests
    {
        /// <summary>
        /// Verifies sprint drain, regeneration delay, and later regeneration.
        /// </summary>
        [Test]
        public void Update_SprintThenRest_DrainsWaitsThenRegenerates()
        {
            StaminaModel.Settings settings = StaminaModel.Settings.Default;
            settings.MaxStamina = 10f;
            settings.SprintDrainPerSecond = 5f;
            settings.RegenDelay = 1f;
            settings.RegenPerSecond = 4f;
            StaminaModel model = new StaminaModel(settings);

            model.Update(true, 1f);
            Assert.That(model.CurrentStamina, Is.EqualTo(5f).Within(0.001f));

            model.Update(false, 0.5f);
            Assert.That(model.CurrentStamina, Is.EqualTo(5f).Within(0.001f));

            model.Update(false, 0.5f);
            model.Update(false, 0.5f);
            Assert.That(model.CurrentStamina, Is.EqualTo(7f).Within(0.001f));
        }

        /// <summary>
        /// Verifies discrete stamina costs succeed or fail based on current stamina.
        /// </summary>
        [Test]
        public void Spend_DodgeAndAttackCosts_RequireAvailableStamina()
        {
            StaminaModel.Settings settings = StaminaModel.Settings.Default;
            settings.MaxStamina = 30f;
            settings.DodgeCost = 20f;
            settings.LightAttackCost = 10f;
            settings.HeavyAttackCost = 25f;
            StaminaModel model = new StaminaModel(settings);

            Assert.That(model.TrySpendDodge(), Is.True);
            Assert.That(model.TrySpendLightAttack(), Is.True);
            Assert.That(model.TrySpendHeavyAttack(), Is.False);
            Assert.That(model.CurrentStamina, Is.EqualTo(0f).Within(0.001f));
        }

        /// <summary>
        /// Verifies sprinting cannot start when stamina is empty.
        /// </summary>
        [Test]
        public void Update_EmptyStamina_CannotSprint()
        {
            StaminaModel.Settings settings = StaminaModel.Settings.Default;
            settings.MaxStamina = 10f;
            settings.MinimumSprintStamina = 0f;
            StaminaModel model = new StaminaModel(settings);
            model.Spend(10f);

            StaminaModel.State state = model.Update(true, 1f);

            Assert.That(model.CanSprint, Is.False);
            Assert.That(state.IsSprinting, Is.False);
        }
    }
}
