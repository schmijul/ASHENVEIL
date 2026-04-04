using Ashenveil.World;
using NUnit.Framework;
using UnityEngine;

namespace Ashenveil.Tests.EditMode
{
    public class ForestPlacementPlannerTests
    {
        [Test]
        public void GenerateCluster_CountRequested_ReturnsMatchingSampleCount()
        {
            var samples = ForestPlacementPlanner.GenerateCluster(12, Vector3.zero, 10f, 42, 0.8f, 1.2f);

            Assert.That(samples.Count, Is.EqualTo(12));
        }

        [Test]
        public void GenerateCluster_SameSeed_ReturnsDeterministicPositions()
        {
            var first = ForestPlacementPlanner.GenerateCluster(4, Vector3.zero, 10f, 42, 0.8f, 1.2f);
            var second = ForestPlacementPlanner.GenerateCluster(4, Vector3.zero, 10f, 42, 0.8f, 1.2f);

            Assert.That(first[0].Position, Is.EqualTo(second[0].Position));
            Assert.That(first[0].Scale, Is.EqualTo(second[0].Scale));
        }

        [Test]
        public void GenerateClusters_ZeroClusterCount_ReturnsEmptyList()
        {
            var samples = ForestPlacementPlanner.GenerateClusters(0, 5, Vector3.zero, 12f, 3f, 99, 1f, 1.5f);

            Assert.That(samples, Is.Empty);
        }
    }
}
