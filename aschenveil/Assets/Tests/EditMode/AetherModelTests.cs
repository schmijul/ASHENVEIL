using System.Collections.Generic;
using Ashenveil.Aether;
using NUnit.Framework;

namespace Ashenveil.Tests.EditMode
{
    public sealed class AetherModelTests
    {
        private static CorruptionModel MakeCorruption()
        {
            return new CorruptionModel(
                new[] { 25f, 50f, 75f, 100f },
                new[] { 0.1f, 0.2f, 0.35f, 0.5f });
        }

        private static AetherModel MakeAether(CorruptionModel corruption, float corruptionPerCharge = 1f)
        {
            return new AetherModel(
                maxCharge: 100f,
                passiveDecayPerSecond: 5f,
                corruptionPerChargeSpent: corruptionPerCharge,
                corruption: corruption);
        }

        [Test]
        public void Absorb_ClampsToMax()
        {
            var aether = MakeAether(MakeCorruption());
            aether.Absorb(150f);
            Assert.AreEqual(100f, aether.Charge, 1e-4f);
            Assert.AreEqual(1f, aether.ChargeFraction, 1e-4f);
        }

        [Test]
        public void Spend_FailsWithoutEnoughCharge()
        {
            var aether = MakeAether(MakeCorruption());
            aether.Absorb(10f);
            Assert.IsFalse(aether.CanSpend(20f));
            Assert.IsFalse(aether.SpendForEmpoweredAttack(20f));
            Assert.AreEqual(10f, aether.Charge, 1e-4f);
        }

        [Test]
        public void Spend_DeductsChargeAndAddsCorruption()
        {
            var corruption = MakeCorruption();
            var aether = MakeAether(corruption, corruptionPerCharge: 2f);
            aether.Absorb(40f);

            Assert.IsTrue(aether.SpendForEmpoweredAttack(10f));
            Assert.AreEqual(30f, aether.Charge, 1e-4f);
            Assert.AreEqual(20f, corruption.Corruption, 1e-4f);
        }

        [Test]
        public void Corruption_RaisesEachStageExactlyOnce()
        {
            var corruption = MakeCorruption();
            var events = new List<int>();
            corruption.StageChanged += (stage, _) => events.Add(stage);

            corruption.Add(60f); // crosses 25 and 50 in one jump

            Assert.AreEqual(new[] { 1, 2 }, events);
            Assert.AreEqual(2, corruption.Stage);
            Assert.AreEqual(0.2f, corruption.MaxHealthPenalty, 1e-4f);

            corruption.Add(50f); // crosses 75 and 100
            Assert.AreEqual(new[] { 1, 2, 3, 4 }, events);
            Assert.AreEqual(0.5f, corruption.MaxHealthPenalty, 1e-4f);
        }

        [Test]
        public void Tick_DecaysChargeButNotCorruption()
        {
            var corruption = MakeCorruption();
            var aether = MakeAether(corruption);
            aether.Absorb(50f);
            aether.SpendForEmpoweredAttack(10f);
            float corruptionBefore = corruption.Corruption;

            aether.Tick(2f); // 5/s * 2s = 10 decay

            Assert.AreEqual(30f, aether.Charge, 1e-4f);
            Assert.AreEqual(corruptionBefore, corruption.Corruption, 1e-4f);
        }

        [Test]
        public void Glow_ZeroUntilAwakenedThenMonotonic()
        {
            var glow = new AetherGlowModel(baseIntensity: 0.5f, maxIntensity: 3f);
            Assert.AreEqual(0f, glow.IntensityForChargeFraction(1f), 1e-4f);

            glow.Awaken();
            float low = glow.IntensityForChargeFraction(0.2f);
            float high = glow.IntensityForChargeFraction(0.9f);
            Assert.AreEqual(0.5f, glow.IntensityForChargeFraction(0f), 1e-4f);
            Assert.Less(low, high);
            Assert.AreEqual(3f, glow.IntensityForChargeFraction(1f), 1e-4f);
        }
    }
}
