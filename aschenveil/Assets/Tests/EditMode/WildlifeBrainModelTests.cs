using Ashenveil.AI;
using NUnit.Framework;

namespace Ashenveil.Tests.EditMode
{
    /// <summary>
    /// EditMode tests for deterministic wildlife brain and loot logic.
    /// </summary>
    public sealed class WildlifeBrainModelTests
    {
        /// <summary>
        /// Verifies flee-temperament wildlife flees from visible nearby threats.
        /// </summary>
        [Test]
        public void Tick_FleeSpeciesThreatened_EntersFlee()
        {
            WildlifeBrainModel.Settings settings = WildlifeBrainModel.Settings.Default;
            settings.Temperament = WildlifeTemperament.Flee;
            settings.PerceptionRadius = 10f;
            WildlifeBrainModel model = new WildlifeBrainModel(settings, new SystemRandomSource(1));

            WildlifeBrainModel.Output output = model.Tick(new WildlifeBrainModel.Input
            {
                DistanceToThreat = 5f,
                LineOfSight = true,
                HealthFraction = 1f
            }, 0.1f);

            Assert.That(output.State, Is.EqualTo(WildlifeBrainState.Flee));
            Assert.That(output.ShouldFlee, Is.True);
        }

        /// <summary>
        /// Verifies aggressive wildlife chases visible threats and attacks in range.
        /// </summary>
        [Test]
        public void Tick_AggressiveSpecies_ChasesThenAttacksInRange()
        {
            WildlifeBrainModel.Settings settings = WildlifeBrainModel.Settings.Default;
            settings.Temperament = WildlifeTemperament.Aggressive;
            settings.PerceptionRadius = 12f;
            settings.AttackRange = 2f;
            settings.AttackCooldown = 1f;
            WildlifeBrainModel model = new WildlifeBrainModel(settings, new SystemRandomSource(2));

            WildlifeBrainModel.Output chase = model.Tick(new WildlifeBrainModel.Input
            {
                DistanceToThreat = 6f,
                LineOfSight = true,
                HealthFraction = 1f
            }, 0.1f);

            WildlifeBrainModel.Output attack = model.Tick(new WildlifeBrainModel.Input
            {
                DistanceToThreat = 1f,
                LineOfSight = true,
                HealthFraction = 1f
            }, 0.1f);

            Assert.That(chase.State, Is.EqualTo(WildlifeBrainState.Chase));
            Assert.That(chase.ShouldChase, Is.True);
            Assert.That(attack.State, Is.EqualTo(WildlifeBrainState.Attack));
            Assert.That(attack.ShouldAttack, Is.True);
        }

        /// <summary>
        /// Verifies death is terminal regardless of later threat input.
        /// </summary>
        [Test]
        public void Tick_HealthZero_DeadStateIsTerminal()
        {
            WildlifeBrainModel model = new WildlifeBrainModel(WildlifeBrainModel.Settings.Default, new SystemRandomSource(3));

            WildlifeBrainModel.Output dead = model.Tick(new WildlifeBrainModel.Input
            {
                DistanceToThreat = 1f,
                LineOfSight = true,
                HealthFraction = 0f
            }, 0.1f);

            WildlifeBrainModel.Output later = model.Tick(new WildlifeBrainModel.Input
            {
                DistanceToThreat = 1f,
                LineOfSight = true,
                HealthFraction = 1f
            }, 0.1f);

            Assert.That(dead.State, Is.EqualTo(WildlifeBrainState.Dead));
            Assert.That(later.State, Is.EqualTo(WildlifeBrainState.Dead));
            Assert.That(later.ShouldAttack, Is.False);
        }

        /// <summary>
        /// Verifies seeded loot rolls respect chance and amount bounds.
        /// </summary>
        [Test]
        public void RollAmount_SeededRandom_RespectsChanceAndBounds()
        {
            int guaranteed = WildlifeLootRoller.RollAmount(2, 4, 1f, new SystemRandomSource(4));
            int impossible = WildlifeLootRoller.RollAmount(2, 4, 0f, new SystemRandomSource(4));

            Assert.That(guaranteed, Is.InRange(2, 4));
            Assert.That(impossible, Is.EqualTo(0));
        }
    }
}
