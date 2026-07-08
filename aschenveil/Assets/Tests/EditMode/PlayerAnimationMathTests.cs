using Ashenveil.Player;
using NUnit.Framework;

namespace Ashenveil.Tests.EditMode
{
    /// <summary>
    /// EditMode tests for player animation math.
    /// </summary>
    public sealed class PlayerAnimationMathTests
    {
        /// <summary>
        /// Verifies raw sprint speed maps to a fully normalized animation value.
        /// </summary>
        [Test]
        public void DampNormalizedSpeed_RawSpeedAtMax_ReturnsOne()
        {
            float value = PlayerAnimationMath.DampNormalizedSpeed(0f, 6.2f, 6.2f, 100f, 1f);

            Assert.That(value, Is.EqualTo(1f).Within(0.001f));
        }

        /// <summary>
        /// Verifies speed values above max stay inside Animator's expected 0..1 range.
        /// </summary>
        [Test]
        public void DampNormalizedSpeed_RawSpeedAboveMax_ClampsToOne()
        {
            float value = PlayerAnimationMath.DampNormalizedSpeed(0f, 12f, 6f, 100f, 1f);

            Assert.That(value, Is.EqualTo(1f).Within(0.001f));
        }

        /// <summary>
        /// Verifies damping limits the frame-to-frame change.
        /// </summary>
        [Test]
        public void DampNormalizedSpeed_DampingLowerThanTarget_MovesByMaxDelta()
        {
            float value = PlayerAnimationMath.DampNormalizedSpeed(0f, 10f, 10f, 2f, 0.1f);

            Assert.That(value, Is.EqualTo(0.2f).Within(0.001f));
        }

        /// <summary>
        /// Verifies invalid max speed eases the value safely back toward idle.
        /// </summary>
        [Test]
        public void DampNormalizedSpeed_InvalidMaxSpeed_DampsTowardZero()
        {
            float value = PlayerAnimationMath.DampNormalizedSpeed(0.5f, 5f, 0f, 1f, 0.2f);

            Assert.That(value, Is.EqualTo(0.3f).Within(0.001f));
        }
    }
}
