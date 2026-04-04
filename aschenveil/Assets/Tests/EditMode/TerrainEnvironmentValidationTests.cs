using Ashenveil.World;
using NUnit.Framework;

namespace Ashenveil.Tests.EditMode
{
    public class TerrainEnvironmentValidationTests
    {
        [Test]
        public void MatchesGddTerrainTarget_DefaultProfile_ReturnsTrue()
        {
            TerrainEnvironmentProfile profile = TerrainEnvironmentProfile.CreateRuntimeDefaults();

            Assert.That(TerrainEnvironmentValidation.MatchesGddTerrainTarget(profile), Is.True);
        }

        [Test]
        public void HasMinimumLayerCount_DefaultProfileWithFourSlots_ReturnsTrue()
        {
            TerrainEnvironmentProfile profile = TerrainEnvironmentProfile.CreateRuntimeDefaults();

            Assert.That(TerrainEnvironmentValidation.HasMinimumLayerCount(profile, 4), Is.True);
        }

        [Test]
        public void HasPlayableForestDensity_DefaultProfile_ReturnsTrue()
        {
            TerrainEnvironmentProfile profile = TerrainEnvironmentProfile.CreateRuntimeDefaults();

            Assert.That(TerrainEnvironmentValidation.HasPlayableForestDensity(profile), Is.True);
        }
    }
}
