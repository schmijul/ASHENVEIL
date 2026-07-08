using UnityEngine;

namespace Ashenveil.CameraRig
{
    /// <summary>
    /// Deterministic third-person camera orbit, zoom, pitch clamp, and collision boom calculation.
    /// Referenced GDD section: Kernsysteme / Movement/Camera.
    /// </summary>
    public sealed class ThirdPersonCameraModel
    {
        private readonly Settings _settings;

        /// <summary>
        /// Creates a camera model with explicit tunables.
        /// </summary>
        public ThirdPersonCameraModel(Settings settings)
        {
            _settings = settings.Normalized();
            Zoom = _settings.DefaultZoom;
            Pitch = _settings.DefaultPitch;
        }

        /// <summary>
        /// Current yaw in degrees.
        /// </summary>
        public float Yaw { get; private set; }

        /// <summary>
        /// Current clamped pitch in degrees.
        /// </summary>
        public float Pitch { get; private set; }

        /// <summary>
        /// Current clamped desired boom length.
        /// </summary>
        public float Zoom { get; private set; }

        /// <summary>
        /// Applies mouse orbit deltas.
        /// </summary>
        public void AddOrbitDelta(Vector2 lookDelta)
        {
            Yaw += lookDelta.x * _settings.YawSensitivity;
            Pitch = Mathf.Clamp(Pitch - lookDelta.y * _settings.PitchSensitivity, _settings.MinPitch, _settings.MaxPitch);
        }

        /// <summary>
        /// Applies a scroll delta to the desired boom length.
        /// </summary>
        public void AddZoomDelta(float zoomDelta)
        {
            Zoom = Mathf.Clamp(Zoom - zoomDelta * _settings.ZoomSensitivity, _settings.MinZoom, _settings.MaxZoom);
        }

        /// <summary>
        /// Sets desired boom length directly.
        /// </summary>
        public void SetZoom(float zoom)
        {
            Zoom = Mathf.Clamp(zoom, _settings.MinZoom, _settings.MaxZoom);
        }

        /// <summary>
        /// Calculates a boom length that avoids a collision hit.
        /// </summary>
        public float CalculateSafeBoomLength(float desiredBoomLength, float hitDistance)
        {
            float desired = Mathf.Clamp(desiredBoomLength, _settings.MinZoom, _settings.MaxZoom);
            if (hitDistance <= 0f)
            {
                return desired;
            }

            return Mathf.Clamp(hitDistance - _settings.CollisionPadding, _settings.MinCollisionBoomLength, desired);
        }

        /// <summary>
        /// Tunables used by the camera model.
        /// </summary>
        public struct Settings
        {
            /// <summary>
            /// Minimum vertical orbit angle.
            /// </summary>
            public float MinPitch { get; set; }

            /// <summary>
            /// Maximum vertical orbit angle.
            /// </summary>
            public float MaxPitch { get; set; }

            /// <summary>
            /// Starting vertical orbit angle.
            /// </summary>
            public float DefaultPitch { get; set; }

            /// <summary>
            /// Minimum desired zoom distance.
            /// </summary>
            public float MinZoom { get; set; }

            /// <summary>
            /// Maximum desired zoom distance.
            /// </summary>
            public float MaxZoom { get; set; }

            /// <summary>
            /// Starting desired zoom distance.
            /// </summary>
            public float DefaultZoom { get; set; }

            /// <summary>
            /// Horizontal look sensitivity.
            /// </summary>
            public float YawSensitivity { get; set; }

            /// <summary>
            /// Vertical look sensitivity.
            /// </summary>
            public float PitchSensitivity { get; set; }

            /// <summary>
            /// Mouse wheel zoom sensitivity.
            /// </summary>
            public float ZoomSensitivity { get; set; }

            /// <summary>
            /// Distance kept between the camera and a collision hit.
            /// </summary>
            public float CollisionPadding { get; set; }

            /// <summary>
            /// Smallest allowed boom length during collision correction.
            /// </summary>
            public float MinCollisionBoomLength { get; set; }

            /// <summary>
            /// Sensible default camera settings for the demo player.
            /// </summary>
            public static Settings Default => new Settings
            {
                MinPitch = -35f,
                MaxPitch = 65f,
                DefaultPitch = 20f,
                MinZoom = 2f,
                MaxZoom = 6f,
                DefaultZoom = 4.5f,
                YawSensitivity = 0.18f,
                PitchSensitivity = 0.16f,
                ZoomSensitivity = 0.2f,
                CollisionPadding = 0.25f,
                MinCollisionBoomLength = 0.35f
            };

            /// <summary>
            /// Returns settings clamped to usable values.
            /// </summary>
            public Settings Normalized()
            {
                Settings settings = this;
                if (settings.MinPitch > settings.MaxPitch)
                {
                    float oldMin = settings.MinPitch;
                    settings.MinPitch = settings.MaxPitch;
                    settings.MaxPitch = oldMin;
                }

                settings.DefaultPitch = Mathf.Clamp(settings.DefaultPitch, settings.MinPitch, settings.MaxPitch);
                settings.MinZoom = Mathf.Max(0.1f, settings.MinZoom);
                settings.MaxZoom = Mathf.Max(settings.MinZoom, settings.MaxZoom);
                settings.DefaultZoom = Mathf.Clamp(settings.DefaultZoom, settings.MinZoom, settings.MaxZoom);
                settings.YawSensitivity = Mathf.Max(0f, settings.YawSensitivity);
                settings.PitchSensitivity = Mathf.Max(0f, settings.PitchSensitivity);
                settings.ZoomSensitivity = Mathf.Max(0f, settings.ZoomSensitivity);
                settings.CollisionPadding = Mathf.Max(0f, settings.CollisionPadding);
                settings.MinCollisionBoomLength = Mathf.Clamp(settings.MinCollisionBoomLength, 0.05f, settings.MaxZoom);
                return settings;
            }
        }
    }
}
