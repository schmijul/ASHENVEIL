using Ashenveil.World;
using NUnit.Framework;
using UnityEngine;

namespace Ashenveil.Tests.EditMode
{
    public class LightingPhaseMathTests
    {
        [Test]
        public void Lerp_HalfBlend_InterpolatesIntensity()
        {
            var from = new LightingPhaseProfile.LightingPhaseDefinition { directionalIntensity = 1f, ambientColor = Color.black };
            var to = new LightingPhaseProfile.LightingPhaseDefinition { directionalIntensity = 3f, ambientColor = Color.white };

            LightingPhaseProfile.LightingPhaseDefinition blended = LightingPhaseMath.Lerp(from, to, 0.5f);

            Assert.That(blended.directionalIntensity, Is.EqualTo(2f).Within(0.0001f));
        }

        [Test]
        public void Lerp_HalfBlend_InterpolatesAmbientColor()
        {
            var from = new LightingPhaseProfile.LightingPhaseDefinition { ambientColor = Color.black };
            var to = new LightingPhaseProfile.LightingPhaseDefinition { ambientColor = Color.white };

            LightingPhaseProfile.LightingPhaseDefinition blended = LightingPhaseMath.Lerp(from, to, 0.5f);

            Assert.That(blended.ambientColor.r, Is.EqualTo(0.5f).Within(0.0001f));
        }

        [Test]
        public void CreateRuntimeDefaults_ProvidesFourPhases()
        {
            LightingPhaseProfile profile = LightingPhaseProfile.CreateRuntimeDefaults();

            Assert.That(profile.PhaseCount, Is.EqualTo(4));
        }
    }
}
