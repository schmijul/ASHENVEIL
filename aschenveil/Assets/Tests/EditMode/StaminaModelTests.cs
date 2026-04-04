using Ashenveil.Player;
using NUnit.Framework;

namespace Ashenveil.Tests.EditMode
{
    public class StaminaModelTests
    {
        [Test]
        public void TryConsume_InsufficientStamina_ReturnsFalseAndPreservesValue()
        {
            var stamina = new StaminaModel(100f, 4f, 15f, 1f, 5f);

            bool result = stamina.TryConsume(10f);

            Assert.That(result, Is.False);
            Assert.That(stamina.CurrentStamina, Is.EqualTo(4f).Within(0.0001f));
        }

        [Test]
        public void Tick_BeforeRegenDelay_DoesNotRestoreStamina()
        {
            var stamina = new StaminaModel(100f, 50f, 15f, 1f, 5f);

            stamina.Tick(0.5f);

            Assert.That(stamina.CurrentStamina, Is.EqualTo(50f).Within(0.0001f));
        }

        [Test]
        public void Tick_AfterRegenDelay_RestoresAndClampsStamina()
        {
            var stamina = new StaminaModel(100f, 50f, 15f, 1f, 5f);

            stamina.Tick(2f);

            Assert.That(stamina.CurrentStamina, Is.EqualTo(65f).Within(0.0001f));
        }
    }
}
