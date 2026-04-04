using System.Linq;
using Ashenveil.World;
using NUnit.Framework;
using UnityEngine;

namespace Ashenveil.Tests.EditMode
{
    public class VillageLayoutMathTests
    {
        [Test]
        public void CreateDefaultBuildingPlan_GrauweilerSpec_ReturnsFifteenPlacements()
        {
            var placements = VillageLayoutMath.CreateDefaultBuildingPlan(new Vector2(30f, 30f), new Vector2(150f, 118f));

            Assert.That(placements.Count, Is.EqualTo(15));
        }

        [Test]
        public void CreateDefaultBuildingPlan_EnterableRoles_FlagsElderAndTraderOnly()
        {
            var placements = VillageLayoutMath.CreateDefaultBuildingPlan(new Vector2(30f, 30f), new Vector2(150f, 118f));

            Assert.That(placements.Count(p => p.Enterable), Is.EqualTo(2));
            Assert.That(placements.Any(p => p.Kind == VillageLayoutMath.VillageBuildingKind.ElderHouse && p.Enterable), Is.True);
            Assert.That(placements.Any(p => p.Kind == VillageLayoutMath.VillageBuildingKind.TraderShop && p.Enterable), Is.True);
        }

        [Test]
        public void CreatePalisadePlan_RectangleWithGate_OmitsSouthWallSegmentsAtGate()
        {
            var placements = VillageLayoutMath.CreatePalisadePlan(new Vector2(150f, 118f), 18f, 7.5f);

            Assert.That(placements.Any(p => p.IsGate), Is.True);
            Assert.That(placements.Any(p => !p.IsGate && Mathf.Abs(p.LocalPosition.z + 59f) < 0.01f && Mathf.Abs(p.LocalPosition.x) < 9f), Is.False);
        }
    }
}
