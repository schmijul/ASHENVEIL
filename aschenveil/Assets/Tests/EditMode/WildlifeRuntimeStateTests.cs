using System.Collections.Generic;
using Ashenveil.AI;
using Ashenveil.Combat;
using NUnit.Framework;

namespace Ashenveil.Tests.EditMode
{
    public class WildlifeRuntimeStateTests
    {
        [Test]
        public void Tick_PlayerDetectedWithinAlertRadius_DeerTransitionsToFlee()
        {
            WildlifeProfile profile = WildlifeProfile.CreateRuntimeDefaults(WildlifeSpecies.Deer);
            WildlifeRuntimeState state = new WildlifeRuntimeState(profile, new ScriptedWildlifeRandomSource());
            WildlifePerception perception = CreateDetectedPerception(10f);

            state.Tick(perception, 0.1f);
            Assert.That(state.CurrentState, Is.EqualTo(WildlifeState.Alert));

            state.Tick(perception, 1.0f);
            Assert.That(state.CurrentState, Is.EqualTo(WildlifeState.Flee));
        }

        [Test]
        public void Tick_PlayerWithinAggroRadius_BoarTransitionsToChaseAndAttack()
        {
            WildlifeProfile profile = WildlifeProfile.CreateRuntimeDefaults(WildlifeSpecies.Boar);
            WildlifeRuntimeState state = new WildlifeRuntimeState(profile, new ScriptedWildlifeRandomSource());
            WildlifePerception perception = CreateDetectedPerception(7f);

            state.Tick(perception, 0.1f);
            Assert.That(state.CurrentState, Is.EqualTo(WildlifeState.Alert));

            state.Tick(perception, 1.0f);
            Assert.That(state.CurrentState, Is.EqualTo(WildlifeState.Chase));

            perception.PlayerDistance = 1.5f;
            state.Tick(perception, 0.1f);

            Assert.That(state.CurrentState, Is.EqualTo(WildlifeState.Attack));
            Assert.That(state.CurrentAttack.AttackKind, Is.EqualTo(WildlifeAttackKind.Charge));
        }

        [Test]
        public void Tick_PlayerBehindTarget_WolfUsesRearAttack()
        {
            WildlifeProfile profile = WildlifeProfile.CreateRuntimeDefaults(WildlifeSpecies.Wolf);
            WildlifeRuntimeState state = new WildlifeRuntimeState(profile, new ScriptedWildlifeRandomSource());
            WildlifePerception perception = CreateDetectedPerception(10f);

            state.Tick(perception, 0.1f);
            state.Tick(perception, 1.0f);
            Assert.That(state.CurrentState, Is.EqualTo(WildlifeState.Chase));

            perception.PlayerDistance = 1.8f;
            perception.IsPlayerBehindTarget = true;
            state.Tick(perception, 0.1f);

            Assert.That(state.CurrentState, Is.EqualTo(WildlifeState.Attack));
            Assert.That(state.CurrentAttack.AnimationTrigger, Is.EqualTo("Bite"));
            Assert.That(state.CurrentAttack.RequiresRearApproach, Is.True);
        }

        [Test]
        public void Tick_HealthFallsBelowHalf_MutatedWolfTriggersHowlAndSpeedBuff()
        {
            WildlifeProfile profile = WildlifeProfile.CreateRuntimeDefaults(WildlifeSpecies.MutatedWolf);
            WildlifeRuntimeState state = new WildlifeRuntimeState(profile, new ScriptedWildlifeRandomSource());
            WildlifePerception perception = WildlifePerception.CreateDefault();
            perception.HealthRatio = 0.49f;

            state.Tick(perception, 0.1f);

            Assert.That(state.CurrentState, Is.EqualTo(WildlifeState.Attack));
            Assert.That(state.CurrentAttack.AttackKind, Is.EqualTo(WildlifeAttackKind.Howl));
            Assert.That(state.HasTriggeredHowl, Is.True);
            Assert.That(state.AttackSpeedMultiplier, Is.EqualTo(1.3f).Within(0.0001f));
        }

        [Test]
        public void ResolveLoot_BoarRollSucceeds_ReturnsGuaranteedDropsAndTusk()
        {
            WildlifeProfile profile = WildlifeProfile.CreateRuntimeDefaults(WildlifeSpecies.Boar);
            List<WildlifeLootResult> loot = profile.ResolveLoot(new ScriptedWildlifeRandomSource(0.1f));

            Assert.That(loot, Has.Count.EqualTo(3));
            Assert.That(loot[0].ItemId, Is.EqualTo("Boar Pelt"));
            Assert.That(loot[1].ItemId, Is.EqualTo("Boar Meat"));
            Assert.That(loot[2].ItemId, Is.EqualTo("Boar Tusk"));
        }

        [Test]
        public void CalculateDamage_MutatedWolfResistances_ApplyPhysicalReductionAndPreserveAether()
        {
            WildlifeProfile profile = WildlifeProfile.CreateRuntimeDefaults(WildlifeSpecies.MutatedWolf);
            WildlifeDamageModel damageModel = profile.CreateDamageModel();

            Assert.That(damageModel.CalculateDamage(20f, DamageType.Slash), Is.EqualTo(10f).Within(0.0001f));
            Assert.That(damageModel.CalculateDamage(20f, DamageType.Aether), Is.EqualTo(20f).Within(0.0001f));
        }

        private static WildlifePerception CreateDetectedPerception(float distance)
        {
            WildlifePerception perception = WildlifePerception.CreateDefault();
            perception.PlayerDetected = true;
            perception.PlayerDistance = distance;
            perception.IsInsideHomeRadius = true;
            perception.HealthRatio = 1f;
            return perception;
        }

        private sealed class ScriptedWildlifeRandomSource : IWildlifeRandomSource
        {
            private readonly Queue<float> _floatValues;

            public ScriptedWildlifeRandomSource(params float[] floatValues)
            {
                _floatValues = new Queue<float>(floatValues ?? new float[0]);
            }

            public float Value()
            {
                return _floatValues.Count > 0 ? _floatValues.Dequeue() : 0f;
            }

            public float Range(float minInclusive, float maxInclusive)
            {
                return minInclusive;
            }

            public int Range(int minInclusive, int maxExclusive)
            {
                return minInclusive;
            }
        }
    }
}
