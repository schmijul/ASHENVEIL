using Ashenveil.Camera;
using NUnit.Framework;
using UnityEngine;

namespace Ashenveil.Tests.EditMode
{
    public class ThirdPersonCameraMathTests
    {
        [Test]
        public void ClampPitch_OutOfRangeValue_ReturnsBoundedValue()
        {
            float clamped = ThirdPersonCameraMath.ClampPitch(80f, -30f, 60f);

            Assert.That(clamped, Is.EqualTo(60f).Within(0.0001f));
        }

        [Test]
        public void GetPivotPosition_RightShoulderOffset_ShiftsPivotToTheRight()
        {
            Vector3 pivot = ThirdPersonCameraMath.GetPivotPosition(Vector3.zero, Vector3.right, 1.5f, 0.5f);

            Assert.That(Vector3.Distance(pivot, new Vector3(0.5f, 1.5f, 0f)), Is.LessThan(0.0001f));
        }

        [Test]
        public void GetOrbitPosition_FollowsOrbitRotation_ReturnsOffsetBehindPivot()
        {
            Quaternion rotation = ThirdPersonCameraMath.GetOrbitRotation(0f, 0f);
            Vector3 position = ThirdPersonCameraMath.GetOrbitPosition(Vector3.zero, rotation, 2.5f);

            Assert.That(Vector3.Distance(position, new Vector3(0f, 0f, -2.5f)), Is.LessThan(0.0001f));
        }

        [Test]
        public void GetAdjustedFollowDistance_ClampsWithinConfiguredBounds()
        {
            float distance = ThirdPersonCameraMath.GetAdjustedFollowDistance(2.5f, 1200f, 1.5f, 1.5f, 5f);

            Assert.That(distance, Is.EqualTo(1.5f).Within(0.0001f));
        }

        [Test]
        public void GetAdjustedFollowDistance_MouseWheelDelta_NormalizesToSingleStep()
        {
            float distance = ThirdPersonCameraMath.GetAdjustedFollowDistance(2.5f, 120f, 1.5f, 1.5f, 5f);

            Assert.That(distance, Is.EqualTo(1.5f).Within(0.0001f));
        }
    }
}
