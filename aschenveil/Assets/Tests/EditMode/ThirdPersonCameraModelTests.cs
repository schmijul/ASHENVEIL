using Ashenveil.CameraRig;
using NUnit.Framework;
using UnityEngine;

namespace Ashenveil.Tests.EditMode
{
    /// <summary>
    /// EditMode tests for third-person camera logic.
    /// </summary>
    public sealed class ThirdPersonCameraModelTests
    {
        /// <summary>
        /// Verifies pitch is clamped to configured bounds.
        /// </summary>
        [Test]
        public void AddOrbitDelta_LargePitchInput_ClampsPitch()
        {
            ThirdPersonCameraModel.Settings settings = ThirdPersonCameraModel.Settings.Default;
            settings.MinPitch = -10f;
            settings.MaxPitch = 20f;
            settings.DefaultPitch = 0f;
            settings.PitchSensitivity = 1f;
            ThirdPersonCameraModel model = new ThirdPersonCameraModel(settings);

            model.AddOrbitDelta(new Vector2(0f, -100f));
            Assert.That(model.Pitch, Is.EqualTo(20f).Within(0.001f));

            model.AddOrbitDelta(new Vector2(0f, 100f));
            Assert.That(model.Pitch, Is.EqualTo(-10f).Within(0.001f));
        }

        /// <summary>
        /// Verifies collision hits shorten the camera boom.
        /// </summary>
        [Test]
        public void CalculateSafeBoomLength_CollisionHit_ShortensBoom()
        {
            ThirdPersonCameraModel.Settings settings = ThirdPersonCameraModel.Settings.Default;
            settings.MinZoom = 1f;
            settings.MaxZoom = 6f;
            settings.DefaultZoom = 5f;
            settings.CollisionPadding = 0.25f;
            ThirdPersonCameraModel model = new ThirdPersonCameraModel(settings);

            float noHit = model.CalculateSafeBoomLength(5f, 0f);
            float hit = model.CalculateSafeBoomLength(5f, 2f);

            Assert.That(noHit, Is.EqualTo(5f).Within(0.001f));
            Assert.That(hit, Is.EqualTo(1.75f).Within(0.001f));
        }
    }
}
