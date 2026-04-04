using Ashenveil.World;
using NUnit.Framework;

namespace Ashenveil.Tests.EditMode
{
    public class VillageEnvironmentProfileTests
    {
        [Test]
        public void CreateRuntimeDefaults_GrauweilerFootprint_IsSizedForVillageArea()
        {
            VillageEnvironmentProfile profile = VillageEnvironmentProfile.CreateRuntimeDefaults();

            Assert.That(profile.VillageFootprint.x, Is.GreaterThanOrEqualTo(140f));
            Assert.That(profile.VillageFootprint.y, Is.GreaterThanOrEqualTo(100f));
        }

        [Test]
        public void CreateRuntimeDefaults_ResidenceArray_ReservesSevenHomes()
        {
            VillageEnvironmentProfile profile = VillageEnvironmentProfile.CreateRuntimeDefaults();

            Assert.That(profile.ResidencePrefabs, Has.Length.EqualTo(7));
        }

        [Test]
        public void CreateRuntimeDefaults_CentralSquare_FitsInsideVillageFootprint()
        {
            VillageEnvironmentProfile profile = VillageEnvironmentProfile.CreateRuntimeDefaults();

            Assert.That(profile.CentralSquareSize.x, Is.LessThan(profile.VillageFootprint.x));
            Assert.That(profile.CentralSquareSize.y, Is.LessThan(profile.VillageFootprint.y));
        }
    }
}
