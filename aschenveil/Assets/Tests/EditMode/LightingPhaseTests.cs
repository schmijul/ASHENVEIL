using Ashenveil.World;
using NUnit.Framework;
using UnityEngine;

namespace Ashenveil.Tests.EditMode
{
    public class LightingPhaseTests
    {
        [Test]
        public void CreateRuntimeDefaults_PhaseNamesMatchDemoSpecification()
        {
            LightingPhaseProfile profile = LightingPhaseProfile.CreateRuntimeDefaults();

            Assert.That(profile.PhaseCount, Is.EqualTo(4));
            Assert.That(profile.GetPhase(0).phaseName, Is.EqualTo("Hunt"));
            Assert.That(profile.GetPhase(1).phaseName, Is.EqualTo("Village"));
            Assert.That(profile.GetPhase(2).phaseName, Is.EqualTo("Aether"));
            Assert.That(profile.GetPhase(3).phaseName, Is.EqualTo("Destruction"));
        }

        [Test]
        public void GetPhase_OutOfRangeIndex_ClampsToLastPhase()
        {
            LightingPhaseProfile profile = LightingPhaseProfile.CreateRuntimeDefaults();

            Assert.That(profile.GetPhase(99).phaseName, Is.EqualTo("Destruction"));
        }

        [Test]
        public void Lerp_HalfwayTransition_InterpolatesLightingValues()
        {
            LightingPhaseProfile.LightingPhaseDefinition from = new LightingPhaseProfile.LightingPhaseDefinition
            {
                phaseName = "From",
                directionalLightEnabled = true,
                directionalColor = Color.red,
                directionalIntensity = 0f,
                directionalAngle = 0f,
                ambientColor = Color.black,
                fogColor = Color.black,
                transitionDuration = 1f
            };

            LightingPhaseProfile.LightingPhaseDefinition to = new LightingPhaseProfile.LightingPhaseDefinition
            {
                phaseName = "To",
                directionalLightEnabled = false,
                directionalColor = Color.blue,
                directionalIntensity = 2f,
                directionalAngle = 90f,
                ambientColor = Color.white,
                fogColor = Color.white,
                transitionDuration = 3f
            };

            LightingPhaseProfile.LightingPhaseDefinition blended = LightingPhaseMath.Lerp(from, to, 0.5f);

            Assert.That(blended.directionalLightEnabled, Is.False);
            Assert.That(blended.directionalIntensity, Is.EqualTo(1f).Within(0.0001f));
            Assert.That(blended.directionalAngle, Is.EqualTo(45f).Within(0.0001f));
            Assert.That(blended.transitionDuration, Is.EqualTo(2f).Within(0.0001f));
            Assert.That(blended.directionalColor.r, Is.EqualTo(0.5f).Within(0.0001f));
            Assert.That(blended.directionalColor.g, Is.EqualTo(0f).Within(0.0001f));
            Assert.That(blended.directionalColor.b, Is.EqualTo(0.5f).Within(0.0001f));
        }
    }
}
