using UnityEngine;

namespace Ashenveil.VFX
{
    /// <summary>
    /// Shared particle tuning values for procedural Ashenveil effects.
    /// Referenced GDD section: Grafik-Stack.
    /// </summary>
    public static class ParticleBudget
    {
        /// <summary>
        /// Default color used when an aether effect is built with a transparent color.
        /// </summary>
        public static readonly Color AetherDefaultColor = new Color(0.25f, 0.95f, 1f, 0.85f);

        /// <summary>
        /// Default color used when a hand glow effect is built with a transparent color.
        /// </summary>
        public static readonly Color HandGlowDefaultColor = new Color(0.35f, 1f, 0.95f, 0.9f);

        /// <summary>
        /// Warm flame color for burning village effects.
        /// </summary>
        public static readonly Color FireStartColor = new Color(1f, 0.82f, 0.25f, 0.95f);

        /// <summary>
        /// Deep orange flame color used as fire particles fade.
        /// </summary>
        public static readonly Color FireEndColor = new Color(1f, 0.2f, 0.04f, 0f);

        /// <summary>
        /// Middle flame color used while flames cool from yellow to orange.
        /// </summary>
        public static readonly Color FireMiddleColor = new Color(1f, 0.45f, 0.08f, 0.8f);

        /// <summary>
        /// Dark grey starting color for smoke columns.
        /// </summary>
        public static readonly Color SmokeStartColor = new Color(0.12f, 0.12f, 0.12f, 0.55f);

        /// <summary>
        /// Middle smoke color used while the plume spreads.
        /// </summary>
        public static readonly Color SmokeMiddleColor = new Color(0.2f, 0.2f, 0.2f, 0.3f);

        /// <summary>
        /// Transparent grey ending color for smoke columns.
        /// </summary>
        public static readonly Color SmokeEndColor = new Color(0.28f, 0.28f, 0.28f, 0f);

        /// <summary>
        /// Bright ember color for sparse sparks above flames.
        /// </summary>
        public static readonly Color EmberStartColor = new Color(1f, 0.42f, 0.08f, 0.95f);

        /// <summary>
        /// Middle ember color used before sparks fade out.
        /// </summary>
        public static readonly Color EmberMiddleColor = new Color(1f, 0.55f, 0.12f, 0.7f);

        /// <summary>
        /// Transparent ember color for dying sparks.
        /// </summary>
        public static readonly Color EmberEndColor = new Color(1f, 0.12f, 0.02f, 0f);

        public const float StartTime = 0f;
        public const float MiddleTime = 0.45f;
        public const float EndTime = 1f;
        public const float InvisibleAlpha = 0f;
        public const float SoftAetherAlpha = 0.85f;
        public const float StrongAetherAlpha = 0.9f;
        public const float ShapeRadiusThickness = 1f;
        public const float DisabledFloat = 0f;
        public const float EnabledFloat = 1f;
        public const float FullSize = 1f;
        public const int MaxParticlesPerEffect = 256;

        public const float AetherShimmerRate = 7f;
        public const float AetherShimmerLifetimeMin = 2.4f;
        public const float AetherShimmerLifetimeMax = 4.2f;
        public const float AetherShimmerSizeMin = 0.025f;
        public const float AetherShimmerSizeMax = 0.075f;
        public const float AetherShimmerRadius = 0.75f;
        public const float AetherShimmerVelocityMin = 0.08f;
        public const float AetherShimmerVelocityMax = 0.24f;
        public const float AetherShimmerSizeStart = 0.15f;
        public const float AetherShimmerSizeMiddle = 1f;
        public const float AetherShimmerSizeEnd = 0.25f;

        public const float HandGlowRate = 18f;
        public const float HandGlowLifetimeMin = 0.25f;
        public const float HandGlowLifetimeMax = 0.65f;
        public const float HandGlowSizeMin = 0.012f;
        public const float HandGlowSizeMax = 0.035f;
        public const float HandGlowRadius = 0.11f;
        public const float HandGlowVelocityMin = 0.02f;
        public const float HandGlowVelocityMax = 0.08f;
        public const float HandGlowSizeStart = 0.35f;
        public const float HandGlowSizeMiddle = 1f;
        public const float HandGlowSizeEnd = 0f;

        public const float FireRate = 48f;
        public const float FireLifetimeMin = 0.55f;
        public const float FireLifetimeMax = 1.15f;
        public const float FireSizeMin = 0.22f;
        public const float FireSizeMax = 0.46f;
        public const float FireConeAngle = 17f;
        public const float FireConeRadius = 0.38f;
        public const float FireVelocityMin = 1.05f;
        public const float FireVelocityMax = 2.1f;
        public const float FireSizeStart = 1f;
        public const float FireSizeMiddle = 0.6f;
        public const float FireSizeEnd = 0f;

        public const float SmokeRate = 18f;
        public const float SmokeLifetimeMin = 2.2f;
        public const float SmokeLifetimeMax = 4.4f;
        public const float SmokeSizeMin = 0.42f;
        public const float SmokeSizeMax = 0.82f;
        public const float SmokeConeAngle = 28f;
        public const float SmokeConeRadius = 0.55f;
        public const float SmokeVelocityMin = 0.55f;
        public const float SmokeVelocityMax = 1.25f;
        public const float SmokeSizeStart = 0.35f;
        public const float SmokeSizeMiddle = 0.9f;
        public const float SmokeSizeEnd = 1.55f;

        public const float EmberRate = 5f;
        public const float EmberLifetimeMin = 1.1f;
        public const float EmberLifetimeMax = 2.3f;
        public const float EmberSizeMin = 0.025f;
        public const float EmberSizeMax = 0.055f;
        public const float EmberRadius = 0.45f;
        public const float EmberVelocityMin = 0.8f;
        public const float EmberVelocityMax = 1.7f;
        public const float EmberSizeStart = 1f;
        public const float EmberSizeMiddle = 0.75f;
        public const float EmberSizeEnd = 0f;

        public const float MinIntensityMultiplier = 0f;
        public const float MaxIntensityMultiplier = 3f;
    }
}
