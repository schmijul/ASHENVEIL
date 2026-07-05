using Ashenveil.AI;
using Ashenveil.AI.Boss;
using Ashenveil.Core;
using NUnit.Framework;
using UnityEngine;

namespace Ashenveil.Tests.EditMode
{
    public sealed class MutatedWolfBossModelTests
    {
        /// <summary>Deterministic random returning a fixed 0..1 value.</summary>
        private sealed class FixedRandom : IRandomSource
        {
            private readonly float _value;
            public FixedRandom(float value) => _value = value;
            public float Next01() => _value;
            public int RangeInclusive(int min, int max) => min;
        }

        private static MutatedWolfBossModel MakeBoss(float health = 100f)
        {
            return new MutatedWolfBossModel(
                maxHealth: health,
                enrageHealthFraction: 0.4f,
                lungeCooldown: 2f,
                random: new FixedRandom(0.5f));
        }

        private static DamageInfo Damage(float amount, DamageType type)
        {
            return new DamageInfo(amount, type, Vector3.zero, 0f);
        }

        [Test]
        public void PhysicalDamage_IsGatedToTenPercent()
        {
            var boss = MakeBoss();
            float landed = boss.ApplyDamage(Damage(100f, DamageType.Physical));
            Assert.AreEqual(10f, landed, 1e-4f);
            Assert.AreEqual(90f, boss.Health, 1e-4f);
        }

        [Test]
        public void AetherDamage_LandsInFull()
        {
            var boss = MakeBoss();
            float landed = boss.ApplyDamage(Damage(30f, DamageType.Aether));
            Assert.AreEqual(30f, landed, 1e-4f);
            Assert.AreEqual(70f, boss.Health, 1e-4f);
        }

        [Test]
        public void Enrage_TriggersBelowThresholdOnce()
        {
            var boss = MakeBoss();
            int enrageCount = 0;
            boss.PhaseChanged += p => { if (p == BossPhase.Enrage) enrageCount++; };

            boss.ApplyDamage(Damage(70f, DamageType.Aether)); // to 30% (<40%)
            Assert.AreEqual(BossPhase.Enrage, boss.Phase);

            boss.ApplyDamage(Damage(5f, DamageType.Aether));
            Assert.AreEqual(1, enrageCount);
        }

        [Test]
        public void Defeat_RaisesOnceAndBlocksFurtherDamage()
        {
            var boss = MakeBoss();
            int defeats = 0;
            boss.Defeated += () => defeats++;

            boss.ApplyDamage(Damage(200f, DamageType.Aether));
            Assert.IsTrue(boss.IsDefeated);
            Assert.AreEqual(BossPhase.Dead, boss.Phase);

            float landed = boss.ApplyDamage(Damage(50f, DamageType.Aether));
            Assert.AreEqual(0f, landed, 1e-4f);
            Assert.AreEqual(1, defeats);
        }

        [Test]
        public void Tick_FiresImmediatelyThenRespectsCooldown()
        {
            var boss = MakeBoss();

            // Cooldown starts ready, so the first tick attacks and leaves Stalk.
            BossAction first = boss.Tick(1f);
            Assert.AreNotEqual(BossAction.None, first);
            Assert.AreNotEqual(BossPhase.Stalk, boss.Phase);

            // Next tick within the 2s cooldown produces no new action.
            Assert.AreEqual(BossAction.None, boss.Tick(1f));

            // Once the cooldown elapses, it attacks again.
            Assert.AreNotEqual(BossAction.None, boss.Tick(1.5f));
        }
    }
}
