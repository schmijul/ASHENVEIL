using Ashenveil.Player;
using NUnit.Framework;
using UnityEngine;

namespace Ashenveil.Tests.EditMode
{
    public class PlayerMovementMathTests
    {
        [Test]
        public void GetCameraRelativeMovement_DiagonalInput_NormalizesMagnitude()
        {
            Vector3 movement = PlayerMovementMath.GetCameraRelativeMovement(
                new Vector2(1f, 1f),
                Vector3.forward,
                Vector3.right);

            Assert.That(movement.magnitude, Is.EqualTo(1f).Within(0.0001f));
        }

        [Test]
        public void GetCameraRelativeMovement_RotatedCamera_MapsToWorldForward()
        {
            Vector3 movement = PlayerMovementMath.GetCameraRelativeMovement(
                new Vector2(0f, 1f),
                Vector3.right,
                Vector3.back);

            Assert.That(Vector3.Distance(movement, Vector3.right), Is.LessThan(0.0001f));
        }

        [Test]
        public void CalculateJumpVelocity_HigherJumpHeight_ReturnsHigherVelocity()
        {
            float lowJump = PlayerMovementMath.CalculateJumpVelocity(-19.62f, 1f);
            float highJump = PlayerMovementMath.CalculateJumpVelocity(-19.62f, 2f);

            Assert.That(highJump, Is.GreaterThan(lowJump));
        }
    }
}
